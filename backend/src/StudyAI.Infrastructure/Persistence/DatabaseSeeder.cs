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
    }
}
