﻿using CrudeApi.Data;
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
    [ApiController]
    [Route("Api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly PizzaContext _context;

        private readonly UserManager<UsersModel> _userManager;

        private readonly ILogger<CartController> _logger;

        public CartController(PizzaContext dbContext, UserManager<UsersModel> userManager, ILogger<CartController> logger)
        {
            _context = dbContext;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("GetUserName")]
        public async Task<IActionResult> GetUserName()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId);

                if (user != null)
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation($"Product ID: {productId}");

            _logger.LogInformation($"User ID: {userId}");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id, CartItems = new List<CartItem>() };
                _context.Carts.Add(cart);
            }

            var cartItem = cart.CartItems.FirstOrDefault(c => c.ProductId == productId);

            if (cartItem == null)
            {
                var product = _context.Products.SingleOrDefault(p => p.Id == productId);

                cartItem = new CartItem
                {
                    ProductId = productId,
                    Pizza = product,
                    Quantity = 1,
                    DateCreated = DateTime.Now,
                    CartId = cart.CartId

                };

                cart.CartItems.Add(cartItem);

                await _context.CartItems.AddAsync(cartItem);
            }
            else
            {
                cartItem.Quantity++;
            }

            cartItem.Cost = cartItem.Quantity * cartItem.Pizza?.Price ?? 8;

            await _context.SaveChangesAsync();

            return Ok("Item successfully added to the cart!");
        }

        [HttpGet("GetCartById")]
        public async Task<IActionResult> ToGetCartItemsbyId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation($"User ID: {userId}");
            if (userId == null)
            {
                return BadRequest("User not found");
            }
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Pizza)
                .FirstOrDefaultAsync(c => c.UserId.ToString() == userId);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
            };

            string json = JsonSerializer.Serialize(cart, options);

            return Content(json, "application/json");
        }

        [HttpGet]
        [Route("CalculateCartCalculations")]
        public async Task<IActionResult> CalculateCartCost()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User not found");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Pizza)
                .FirstOrDefaultAsync(c => c.UserId.ToString() == userId);

            if (cart == null)
            {
                return BadRequest("Cart not found");
            }
            decimal totalCost = 0;
            foreach (var cartItem in cart.CartItems)
            {
                cartItem.Cost = cartItem.Quantity * cartItem.Pizza.Price;

                totalCost += cartItem.Cost;

            }
            await _context.SaveChangesAsync();
            return Ok(new { TotalCost = totalCost });
        }

        [HttpGet("ItemTotal/{itemId}")]
        public async Task<IActionResult> CalculateItemTotal(int itemId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User not found");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Pizza)
                .FirstOrDefaultAsync(c => c.UserId.ToString() == userId);

            if (cart == null)
            {
                return BadRequest("Cart not found");
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ItemId == itemId);

            if (cartItem == null)
            {
                return BadRequest("Item not found in the cart");
            }

            cartItem.Cost = cartItem.Quantity * cartItem.Pizza.Price;

            await _context.SaveChangesAsync();

            return Ok(new { TotalCost = cartItem.Cost });
        }


        [HttpDelete("delete/{productId}")]
        public async Task<IActionResult> DeleteCartItem(Guid productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId.ToString() == userId);

            if (cart != null)
            {
                var cartItem = cart.CartItems.FirstOrDefault(c => c.ProductId == productId);
                if (cartItem != null)
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
                .FirstOrDefaultAsync(c => c.UserId.ToString() == userId);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();
                return Ok("Cart successfully cleared!");
            }

            return NotFound($"The user with ID: {userId} does not have a cart!");
        }
    }
}