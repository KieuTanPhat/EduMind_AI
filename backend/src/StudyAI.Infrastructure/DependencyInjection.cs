using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StudyAI.Application.Abstractions;
using StudyAI.Infrastructure.Authentication;
using StudyAI.Infrastructure.AI;
using StudyAI.Infrastructure.Persistence;
using StudyAI.Infrastructure.Processing;
using StudyAI.Infrastructure.Storage;

namespace StudyAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DATABASE_CONNECTION_STRING"]
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

        services.AddDbContext<StudyAiDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(StudyAiDbContext).Assembly.FullName)));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<StudyAiDbContext>());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.PostConfigure<JwtOptions>(options =>
        {
            options.Secret = configuration["JWT_SECRET"] ?? options.Secret;
        });
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        services.PostConfigure<GeminiOptions>(options =>
        {
            options.ApiKey = configuration["GEMINI_API_KEY"] ?? options.ApiKey;
        });
        services.AddHttpClient("Gemini", (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<GeminiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : $"{options.BaseUrl}/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });
        services.AddScoped<IAiService, GeminiService>();
        services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
        services.AddScoped<IDocumentProcessingJob, DocumentProcessingJob>();
        services.AddScoped<ITextProcessingService, TextProcessingService>();
        services.AddScoped<IDocumentTextExtractor, PdfTextExtractor>();
        services.AddScoped<IDocumentTextExtractor, DocxTextExtractor>();
        services.AddScoped<IDocumentTextExtractor, TxtTextExtractor>();

        if (configuration.GetValue("Hangfire:Enabled", false))
        {
            services.AddHangfire(options => options
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));
            services.AddHangfireServer();
            services.AddScoped<IDocumentProcessingScheduler, HangfireDocumentProcessingScheduler>();
        }
        else
        {
            services.AddScoped<IDocumentProcessingScheduler, InlineDocumentProcessingScheduler>();
        }

        return services;
    }
}
