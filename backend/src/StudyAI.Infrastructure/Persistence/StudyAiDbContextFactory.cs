using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StudyAI.Infrastructure.Persistence;

public sealed class StudyAiDbContextFactory : IDesignTimeDbContextFactory<StudyAiDbContext>
{
    public StudyAiDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<StudyAiDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=StudyAI;Trusted_Connection=True;TrustServerCertificate=True;",
            sql => sql.MigrationsAssembly(typeof(StudyAiDbContext).Assembly.FullName));

        return new StudyAiDbContext(optionsBuilder.Options);
    }
}
