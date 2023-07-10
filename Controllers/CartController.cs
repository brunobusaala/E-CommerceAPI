using CrudeApi.Data;
using CrudeApi.Models.DomainModels;
using CrudeApi.Models.ShoppingCartModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrudeApi.Controllers
{

    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("Api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly PizzaContext _context;
        private readonly UserManager<UsersModel> _userManager;
        private readonly ILogger<CartController> _logger;

        public CartController(PizzaContext dbContext, UserManager<UsersModel> userManager, ILogger<CartController> logger)
        {
            _context=dbContext;
            _userManager=userManager;
            _logger=logger;
        }

        [HttpGet("GetUserName")]
        public async Task<IActionResult> GetUserName()
        {
            if ( User.Identity.IsAuthenticated )
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId);

                if ( user!=null )
                {
                    var userName = user.UserName;
                    return Ok(userName);
                }
                else
                {
                    return Ok("Sign In");
                }
            }
            else
            {
                return Ok("Sign In");
            }
        }



        [HttpPost("AddItemToCart/{productId}")]
        public async Task<IActionResult> AddToCart([FromRoute] Guid productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Get the userId
            _logger.LogInformation($"Product ID: {productId}");
            _logger.LogInformation($"User ID: {userId}");


            var user = await _userManager.FindByIdAsync(userId);

            if ( user==null )
            {
                return BadRequest("User not found");
            }

            // Retrieve the user's cart or create a new one if it doesn't exist
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId==user.Id);

            if ( cart==null )
            {
                cart=new Cart { UserId=user.Id, CartItems=new List<CartItem>() };
                _context.Carts.Add(cart);
            }

            // Get the cart item from the database
            var cartItem = cart.CartItems.FirstOrDefault(c => c.ProductId==productId);

            if ( cartItem==null )
            {
                var product = _context.Products.SingleOrDefault(p => p.Id==productId);
                cartItem=new CartItem
                {
                    ProductId=productId,
                    Pizza=product,
                    Quantity=1,
                    DateCreated=DateTime.Now,
                    CartId=cart.CartId // Associate the cart item with the cart

                };

                cart.CartItems.Add(cartItem); // Add the cart item to the cart's collection
                await _context.CartItems.AddAsync(cartItem);
            }
            else
            {
                // Increment the quantity of the cart item
                cartItem.Quantity++;

            }

            //Calculate and set the cost
            cartItem.Cost=cartItem.Quantity*cartItem.Pizza?.Price??8;

            await _context.SaveChangesAsync();
            return Ok("Item successfully added to the cart!");
        }

        [HttpGet("GetCartById")]
        public async Task<IActionResult> ToGetCartItemsbyId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation($"User ID: {userId}");
            if ( userId==null )
            {
                return BadRequest("User not found");
            }
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Pizza) // Include the Pizza property
                .FirstOrDefaultAsync(c => c.UserId.ToString()==userId);

            // Set up the JsonSerializerOptions
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                ReferenceHandler=ReferenceHandler.Preserve,
                // Add any other desired options
            };

            // Serialize the cart object to JSON
            string json = JsonSerializer.Serialize(cart, options);

            // Return the JSON as an IActionResult
            return Content(json, "application/json");
        }

        [HttpGet]
        [Route("CalculateCartCalculations")]
        public async Task<IActionResult> CalculateCartCost()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if ( string.IsNullOrEmpty(userId) )
            {
                return BadRequest("User not found");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Pizza)
                .FirstOrDefaultAsync(c => c.UserId.ToString()==userId);

            if ( cart==null )
            {
                return BadRequest("Cart not found");
            }
            decimal totalCost = 0;
            foreach ( var cartItem in cart.CartItems )
            {
                //Calculate the cost of each cart item
                cartItem.Cost=cartItem.Quantity*cartItem.Pizza.Price;
                totalCost+=cartItem.Cost;

            }
            await _context.SaveChangesAsync();
            return Ok(new { TotalCost = totalCost/*, CartItems = cart.CartItems */});
        }

        [HttpDelete("delete/{productId}")]
        public async Task<IActionResult> DeleteCartItem(Guid productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId.ToString()==userId);

            if ( cart!=null )
            {
                var cartItem = cart.CartItems.FirstOrDefault(c => c.ProductId==productId);
                if ( cartItem!=null )
                {
                    _context.CartItems.Remove(cartItem);
                    await _context.SaveChangesAsync();
                    return Ok("Item successfully deleted from the cart!");
                }
            }

            return NotFound($"The item with ProductId: {productId} is not in the user's cart!");
        }

        [HttpDelete]
        [Route("ClearCart")]
        public async Task<IActionResult> ClearCart()
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId.ToString()==userId);

            if ( cart!=null )
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();
                return Ok("Cart successfully cleared!");
            }

            return NotFound($"The user with ID: {userId} does not have a cart!");
        }
    }
}