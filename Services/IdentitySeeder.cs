using DoAnMonLTWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace DoAnMonLTWeb.Services
{
    public static class IdentitySeeder
    {
        private const string AdminRole = "Admin";
        private const string UserRole = "User";

        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { AdminRole, UserRole })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminSection = configuration.GetSection("AdminAccount");
            var adminEmail = adminSection["Email"] ?? "admin@shoplinhkien.com";
            var adminPassword = adminSection["Password"] ?? "Admin@123";
            var adminFullName = adminSection["FullName"] ?? "System Administrator";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = adminFullName,
                    Address = adminSection["Address"] ?? "Admin Office"
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Khong tao duoc tai khoan admin mac dinh: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
            {
                await userManager.AddToRoleAsync(adminUser, AdminRole);
            }

            foreach (var user in userManager.Users.ToList())
            {
                if (!await userManager.IsInRoleAsync(user, AdminRole) && !await userManager.IsInRoleAsync(user, UserRole))
                {
                    await userManager.AddToRoleAsync(user, UserRole);
                }
            }
        }
    }
}
