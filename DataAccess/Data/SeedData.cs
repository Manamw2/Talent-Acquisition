using Microsoft.AspNetCore.Identity;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Initialize Roles
            string[] roleNames = { "HR", "applicant" ,"Admin"};
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            /*
            // Add First Admin (Heidi)
            var admin1 = await userManager.FindByEmailAsync("heidi.elshafei@soft-trend.com");
            if (admin1 == null)
            {
                admin1 = new AppUser
                {
                    UserName = "heidi.elshafei@soft-trend.com",
                    Email = "heidi.elshafei@soft-trend.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin1, "Admin1234@");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin1, "Admin");
                }
            }*/

            // Add Second Admin (Mahmoud)
            var admin2 = await userManager.FindByEmailAsync("mahmoud.nabil@soft-trend.com");
            if (admin2 == null)
            {
                admin2 = new AppUser
                {
                    UserName = "mahmoud.nabil@soft-trend.com",
                    Email = "mahmoud.nabil@soft-trend.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin2, "Admin1234@");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin2, "Admin");
                }
            }
        }
    }
}
