using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Models;

namespace HrBackOffice.Extentions
{
    public static class SeedDataExtention
    {
        public static void Seed(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            SeedData.Initialize(scope.ServiceProvider, userManager, roleManager).Wait();
        }
    }
}
