using Dot.Net.WebApi.Domain;
using Microsoft.AspNetCore.Identity;

namespace Dot.Net.WebApi.Security
{
    public static class IdentitySeeder
    {
        public const string AdminUserNameKey = "Seed:AdminUserName";
        public const string AdminPasswordKey = "Seed:AdminPassword";
        public const string DefaultAdminUserName = "admin";

        public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

            foreach (string role in Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole<int>(role)), $"create role {role}");
                }
            }

            string? password = configuration[AdminPasswordKey];
            if (string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning(
                    "{Key} is not set: no admin account was created. Set it with dotnet user-secrets.",
                    AdminPasswordKey);
                return;
            }

            string userName = configuration[AdminUserNameKey] ?? DefaultAdminUserName;
            var userManager = services.GetRequiredService<UserManager<User>>();

            if (await userManager.FindByNameAsync(userName) != null)
            {
                return;
            }

            var admin = new User { UserName = userName, Fullname = "Administrator" };
            EnsureSucceeded(await userManager.CreateAsync(admin, password), "create the admin account");
            EnsureSucceeded(await userManager.AddToRoleAsync(admin, Roles.Admin), "give the admin role");
        }

        private static void EnsureSucceeded(IdentityResult result, string action)
        {
            if (!result.Succeeded)
            {
                string errors = string.Join("; ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Identity seed failed to {action}: {errors}");
            }
        }
    }
}
