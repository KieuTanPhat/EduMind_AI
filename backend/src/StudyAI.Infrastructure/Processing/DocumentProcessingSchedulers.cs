using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using StudyAI.Application.Abstractions;

namespace StudyAI.Infrastructure.Processing;

public sealed class HangfireDocumentProcessingScheduler : IDocumentProcessingScheduler
{
    private readonly IBackgroundJobClient _backgroundJobs;

    public HangfireDocumentProcessingScheduler(IBackgroundJobClient backgroundJobs)
    {
        _backgroundJobs = backgroundJobs;
    }

    public Task EnqueueAsync(Guid documentId, CancellationToken cancellationToken)
    {
        _backgroundJobs.Enqueue<IDocumentProcessingJob>(job => job.ProcessAsync(documentId));
        return Task.CompletedTask;
    }
}

public sealed class InlineDocumentProcessingScheduler : IDocumentProcessingScheduler
{
    private readonly IServiceScopeFactory _scopeFactory;

    public InlineDocumentProcessingScheduler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task EnqueueAsync(Guid documentId, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var job = scope.ServiceProvider.GetRequiredService<IDocumentProcessingJob>();
            await job.ProcessAsync(documentId);
        }, CancellationToken.None);

        return Task.CompletedTask;
    }
}
