using CrudeApi.Data;
using CrudeApi.Models.DomainModels;
using CrudeApi.Models.LoginModels;
using CrudeApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CrudeApi.Controllers
{
    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<UsersModel> _userManager;
        private readonly SignInManager<UsersModel> _signInManager;
        private readonly ILogger<TokenController> _logger;
        private readonly JwtService _jwtService;
        private readonly PizzaContext _context;
        private readonly IConfiguration _configuration;

        public TokenController(
            UserManager<UsersModel> userManager,
            SignInManager<UsersModel> signInManager,
            ILogger<TokenController> logger,
            JwtService jwtService,
            PizzaContext context,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _jwtService = jwtService;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                _logger.LogWarning($"Login attempt failed: User '{model.UserName}' not found");
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning(
                    $"Login attempt failed: Invalid password for user '{model.UserName}'"
                );
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation($"User '{user.UserName}' has roles: {string.Join(", ", roles)}");

            var token = _jwtService.GenerateJwtToken(user, roles);
            _logger.LogInformation(
                $"Generated token for user '{user.UserName}': {token.Substring(0, 20)}..."
            );

            var expiresIn = Convert.ToInt32(_configuration["Jwt:ExpirationMinutes"] ?? "60") * 60;

            return Ok(
                new
                {
                    AccessToken = token,
                    TokenType = "Bearer",
                    ExpiresIn = expiresIn,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList(),
                }
            );
        }

        [HttpGet("test")]
        [Authorize]
        public IActionResult TestAuthorization()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userRoles = User
                .Claims.Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Ok(
                new
                {
                    Message = "Authorization successful!",
                    UserId = userId,
                    UserName = userName,
                    Roles = userRoles,
                    Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                }
            );
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var principal = _jwtService.GetPrincipalFromExpiredToken(model.AccessToken);
            if (principal == null)
            {
                return BadRequest(new { message = "Invalid access token" });
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Invalid access token" });
            }

            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(r =>
            r.Token == model.RefreshToken && r.UserId == int.Parse(userId)
        );

            if (refreshToken == null)
            {
                return BadRequest(new { message = "Invalid refresh token" });
            }

            if (
            refreshToken.ExpiryDate < DateTime.UtcNow
            || refreshToken.IsUsed
            || refreshToken.IsRevoked
        )
            {
                return BadRequest(new { message = "Refresh token has expired or has been used" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            refreshToken.IsUsed = true;
            _context.RefreshTokens.Update(refreshToken);

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _jwtService.GenerateJwtToken(user, roles);
            var newRefreshToken = GenerateRefreshToken();

            await _context.RefreshTokens.AddAsync(
                new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false,
                    IsUsed = false,
                }
            );

            await _context.SaveChangesAsync();

            var expiresIn = Convert.ToInt32(_configuration["Jwt:ExpirationMinutes"] ?? "60") * 60;

            return Ok(
                new TokenResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = expiresIn,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList(),
                }
            );
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke(string refreshToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var token = await _context.RefreshTokens.FirstOrDefaultAsync(r =>
                r.Token == refreshToken && r.UserId == int.Parse(userId)
            );

            if (token == null)
            {
                return BadRequest(new { message = "Invalid refresh token" });
            }

            token.IsRevoked = true;

            _context.RefreshTokens.Update(token);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Refresh token revoked" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(Register model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new UsersModel
            {
                UserName = model.UserName,
                Email = model.Email,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = "User registered successfully" });
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserMetadata()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(
                new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles,
                }
            );
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
