using CrudeApi.Models.DomainModels;
using Microsoft.AspNetCore.Identity;

namespace CrudeApi.Services
{
    public class RoleInitializationService
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<UsersModel> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RoleInitializationService> _logger;

        public RoleInitializationService(
            RoleManager<IdentityRole<int>> roleManager,
            UserManager<UsersModel> userManager,
            IConfiguration configuration,
            ILogger<RoleInitializationService> logger
        )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InitializeRoles()
        {
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                    _logger.LogInformation($"Created role: {roleName}");
                }
            }

            // Create admin user if configured
            var adminUserName = _configuration["AdminUser:UserName"];
            var adminEmail = _configuration["AdminUser:Email"];
            var adminPassword = _configuration["AdminUser:Password"];

            if (
                !string.IsNullOrEmpty(adminUserName)
                && !string.IsNullOrEmpty(adminEmail)
                && !string.IsNullOrEmpty(adminPassword)
            )
            {
                var adminUser = await _userManager.FindByNameAsync(adminUserName);
                if (adminUser == null)
                {
                    var user = new UsersModel
                    {
                        UserName = adminUserName,
                        Email = adminEmail,
                        DateCreated = DateTime.UtcNow,
                        DateUpdated = DateTime.UtcNow,
                        EmailConfirmed = true,
                    };

                    var result = await _userManager.CreateAsync(user, adminPassword);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                        _logger.LogInformation($"Created admin user: {adminUserName}");
                    }
                    else
                    {
                        _logger.LogError(
                            $"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                        );
                    }
                }
            }
        }
    }
}
