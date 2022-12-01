using Allup.DAL.Entities;
using Allup.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Allup.DAL
{
    public class DataInitializer
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; 
        private readonly AppDbContext _dbContext;
        private readonly AdminUser _adminUser;

        public DataInitializer(IServiceProvider serviceProvider)
        {
            _adminUser = serviceProvider.GetService<IOptions<AdminUser>>().Value;
            _userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        }

        public async Task SeedData()
        {
            await _dbContext.Database.MigrateAsync();

            var roles = new List<string> { Constants.AdminRole, Constants.UserRole };

            foreach (var role in roles)
            {
                if (await _roleManager.RoleExistsAsync(role))
                    continue;

                var result = await _roleManager.CreateAsync(new IdentityRole { Name = role });

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        //logging
                        Console.WriteLine(error.Description);
                    }
                }
            }

            var userExist = await _userManager.FindByNameAsync(_adminUser.Username);

            if (userExist != null)
                return;

            var userResult = await _userManager.CreateAsync(new User
            {
                UserName = _adminUser.Username,
                Email = _adminUser.Email
            }, _adminUser.Password);

            if (!userResult.Succeeded)
            {
                foreach (var error in userResult.Errors)
                {
                    //logging
                    Console.WriteLine(error.Description);
                }
            }
            else
            {
                var existUser = await _userManager.FindByNameAsync(_adminUser.Username);

                await _userManager.AddToRoleAsync(existUser, Constants.AdminRole);
            }
        }
    }
}
