using StudyAI.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace StudyAI.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _rootPath = Path.GetFullPath(configuration["Storage:RootPath"] ?? Path.Combine("storage", "users"));
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Guid userId, string fileName, Stream content, CancellationToken cancellationToken)
    {
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName) || !string.Equals(safeFileName, fileName, StringComparison.Ordinal))
        {
            throw new ArgumentException("The file name is invalid.", nameof(fileName));
        }

        var userDirectory = Path.Combine(_rootPath, userId.ToString("N"), "documents");
        Directory.CreateDirectory(userDirectory);

        var storedFileName = $"{Guid.NewGuid():N}{Path.GetExtension(safeFileName).ToLowerInvariant()}";
        var fullPath = Path.Combine(userDirectory, storedFileName);
        await using var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true);
        await content.CopyToAsync(output, cancellationToken);

        return Path.GetRelativePath(_rootPath, fullPath);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        var fullPath = GetSafeFullPath(storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        var fullPath = GetSafeFullPath(storagePath);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, useAsync: true);
        return Task.FromResult(stream);
    }

    private string GetSafeFullPath(string storagePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, storagePath));
        if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("The storage path is outside the configured storage root.");
        }

        return fullPath;
    }
}
