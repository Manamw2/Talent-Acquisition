using DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace TalentAcquisitionModule.Extentions
{
    public static class MigrationsExtension
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        }
    }
}
