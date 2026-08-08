namespace StudyAI.Application.Abstractions;

public interface IFileStorageService
{
    Task<string> SaveAsync(Guid userId, string fileName, Stream content, CancellationToken cancellationToken);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken);
}
