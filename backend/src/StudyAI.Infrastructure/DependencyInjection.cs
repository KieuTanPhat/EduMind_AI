using Hangfire;
using Hangfire.SqlServer;
using Amazon.Runtime;
using Amazon.S3;
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
        var storageProvider = configuration["Storage:Provider"] ?? "Local";
        if (storageProvider.Equals("R2", StringComparison.OrdinalIgnoreCase))
        {
            var r2Options = configuration.GetSection(R2Options.SectionName).Get<R2Options>() ?? new R2Options();
            r2Options.Endpoint = configuration["R2_ENDPOINT"] ?? r2Options.Endpoint;
            r2Options.Bucket = configuration["R2_BUCKET"] ?? r2Options.Bucket;
            r2Options.AccessKeyId = configuration["R2_ACCESS_KEY_ID"] ?? r2Options.AccessKeyId;
            r2Options.SecretAccessKey = configuration["R2_SECRET_ACCESS_KEY"] ?? r2Options.SecretAccessKey;
            if (string.IsNullOrWhiteSpace(r2Options.Endpoint) || string.IsNullOrWhiteSpace(r2Options.Bucket) || string.IsNullOrWhiteSpace(r2Options.AccessKeyId) || string.IsNullOrWhiteSpace(r2Options.SecretAccessKey))
            {
                throw new InvalidOperationException("Storage:R2 endpoint, bucket, access key and secret key are required when Storage:Provider is R2.");
            }

            services.AddSingleton(r2Options);
            services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(new BasicAWSCredentials(r2Options.AccessKeyId, r2Options.SecretAccessKey), new AmazonS3Config
            {
                ServiceURL = r2Options.Endpoint,
                ForcePathStyle = true,
                AuthenticationRegion = "auto"
            }));
            services.AddSingleton<IFileStorageService, R2FileStorageService>();
        }
        else
        {
            services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        }
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
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.PostConfigure<OpenAiOptions>(options =>
        {
            options.ApiKey = configuration["OPENAI_API_KEY"] ?? options.ApiKey;
        });
        services.AddHttpClient("OpenAI", (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : $"{options.BaseUrl}/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });
        var aiProvider = configuration["AI:Provider"] ?? "OpenAI";
        if (aiProvider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IAiService, GeminiService>();
        }
        else
        {
            services.AddScoped<IAiService, OpenAiService>();
        }
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
