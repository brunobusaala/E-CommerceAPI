//using AutoMapper;
//using CrudeApi.Data;
//using CrudeApi.Models.DomainModels;
//using CrudeApi.Models.LoginModels;
//using CrudeApi.Models.ShoppingCartModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Cors;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace CrudeApi.Controllers
//{
//    [EnableCors]
//    [Route("api/[controller]")]
//    [ApiController]
//    public class LoginController : ControllerBase
//    {
//        private readonly IConfiguration _config;
//        private readonly PizzaContext _db;
//        private readonly IMapper _mapper;

//        public LoginController(IConfiguration config, PizzaContext db, IMapper mapper)
//        {
//            _config=config;
//            _db=db;
//            _mapper=mapper;
//        }

//        [AllowAnonymous]
//        [HttpPost]
//        public IActionResult Login([FromBody] Login login)
//        {
//            IActionResult response = Unauthorized();
//            var user = AuthenticateUser(login);
//            if ( user!=null )
//            {
//                var tokenString = GenerateJSONWebToken(user);
//                response=Ok(new { token = tokenString });
//            }
//            return response;
//        }
//        private string GenerateJSONWebToken(UsersModel userInfo)
//        {
//            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
//            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
//            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
//                _config["Jwt:Issuer"],
//                new[]
//                {
//            new Claim("UserName", userInfo.UserName),
//            new Claim("EmailAddress", userInfo.EmailAddress)
//                    // Add more claims as needed
//                },
//                expires: DateTime.Now.AddMinutes(2),
//                signingCredentials: credentials);
//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }

//        private UsersModel AuthenticateUser(Login login)
//        {
//            //Validate the UserCredentials
//            var user = _db.UsersModels.FirstOrDefault(u => u.UserName==login.UserName&&u.Password==login.Password);
//            return user;

//        }


//        [HttpPost("AddItemToCarts{id}")]
//        public async Task<IActionResult> AddToCart(Guid id)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //Get the user ID from the User object

//            var cartItem = _db.ShoppingCartItems.SingleOrDefault(c => c.ProductId==id&&c.UserId.ToString()==userId);
//            if ( cartItem==null )
//            {
//                //Create a new cart item if it doesn't exist for the user.
//                cartItem=new CartItem
//                {
//                    ProductId=id,
//                    Pizza=_db.Product.SingleOrDefault(p => p.Id==id),
//                    Quantity=1,
//                    DateCreated=DateTime.Now,
//                    UserId=int.Parse(userId)

//                };
//                await _db.ShoppingCartItems.AddAsync(cartItem);
//            }
//            else
//            {
//                //if the cart item exists, increase the quantity
//                cartItem.Quantity++;
//            }
//            await _db.SaveChangesAsync();
//            return Ok();
//        }
//        [HttpGet("allCartCart")]
//        [Authorize]
//        public async Task<IActionResult> ToGetCartItems()
//        {
//            var cartItems = await _db.ShoppingCartItems.Include(c => c.Pizza).ToListAsync();
//            return Ok(cartItems);
//        }
//    }
//}
