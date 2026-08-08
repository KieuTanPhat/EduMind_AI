using Microsoft.EntityFrameworkCore;
using StudyAI.Domain.Entities;

namespace StudyAI.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(StudyAiDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (!await dbContext.Roles.AnyAsync(cancellationToken))
        {
            dbContext.Roles.AddRange(new Role("User"), new Role("Admin"));
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var adminRole = await dbContext.Roles.SingleAsync(x => x.NormalizedName == "ADMIN", cancellationToken);
        var admin = await dbContext.Users.Include(x => x.UserRoles).SingleOrDefaultAsync(x => x.NormalizedEmail == "ADMIN@EDUMIND.LOCAL", cancellationToken);
        if (admin is null)
        {
            admin = new User("admin@edumind.local", "ADMIN@EDUMIND.LOCAL", BCrypt.Net.BCrypt.HashPassword("123"), "System", "Admin");
            admin.VerifyEmail();
            admin.UserRoles.Add(new UserRole(admin.Id, adminRole.Id));
            admin.SetPreference(new UserPreference(admin.Id));
            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Keep the development bootstrap account usable after previous local runs.
            admin.ChangePassword(BCrypt.Net.BCrypt.HashPassword("123"));
            admin.VerifyEmail();

            if (!admin.UserRoles.Any(x => x.RoleId == adminRole.Id))
            {
                admin.UserRoles.Add(new UserRole(admin.Id, adminRole.Id));
            }

            if (!await dbContext.UserPreferences.AnyAsync(x => x.UserId == admin.Id, cancellationToken))
            {
                dbContext.UserPreferences.Add(new UserPreference(admin.Id));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
