using CrudeApi.Models.DomainModels;
using CrudeApi.Models.LoginModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CrudeApi.Controllers
{

    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<UsersModel> _userManager;
        private readonly SignInManager<UsersModel> _signInManager;
        private readonly ILogger<TokenController> _logger;

        public TokenController(IConfiguration configuration, UserManager<UsersModel> userManager, SignInManager<UsersModel> signInManager, ILogger<TokenController> logger)
        {
            _configuration=configuration;
            _userManager=userManager;
            _signInManager=signInManager;
            _logger=logger;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Post(Login _userData)
        {
            if ( _userData!=null&&!string.IsNullOrEmpty(_userData.UserName)&&!string.IsNullOrEmpty(_userData.Password) )
            {
                var user = await _userManager.FindByNameAsync(_userData.UserName);
                if ( user!=null&&await _userManager.CheckPasswordAsync(user, _userData.Password) )
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(), ClaimValueTypes.Integer, user.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("UserName", user.UserName),
                        new Claim("Email", user.Email)
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    _logger.LogInformation($"User ID: {user.Id}");
                    _logger.LogInformation($"Claims: {claims.ToList()}");

                    var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: DateTime.UtcNow.AddMinutes(60),
                        signingCredentials: signIn
                    );


                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
                else
                {
                    return BadRequest("Invalid credentials");
                }
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Post(Register _userData)
        {
            if ( _userData!=null&&!string.IsNullOrEmpty(_userData.UserName)&&!string.IsNullOrEmpty(_userData.Password) )
            {
                var user = new UsersModel
                {
                    UserName=_userData.UserName,
                    Email=_userData.Email,
                    DateCreated=DateTime.UtcNow,
                    DateUpdated=DateTime.UtcNow

                    // Add any additional properties you have in your UsersModel
                };

                var result = await _userManager.CreateAsync(user, _userData.Password);

                if ( result.Succeeded )
                {
                    // User registration successful
                    return Ok();
                }
                else
                {
                    // Registration failed, return the errors
                    return BadRequest(result.Errors);
                }
            }
            else
            {
                return BadRequest();
            }


        }


    }
}
