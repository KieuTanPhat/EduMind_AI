using Amazon.S3;
using Amazon.S3.Model;
using StudyAI.Application.Abstractions;

namespace StudyAI.Infrastructure.Storage;

public sealed class R2FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _client;
    private readonly string _bucket;

    public R2FileStorageService(IAmazonS3 client, R2Options options)
    {
        _client = client;
        _bucket = options.Bucket;
    }

    public async Task<string> SaveAsync(Guid userId, string fileName, Stream content, CancellationToken cancellationToken)
    {
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName) || !string.Equals(safeFileName, fileName, StringComparison.Ordinal))
        {
            throw new ArgumentException("The file name is invalid.", nameof(fileName));
        }

        var key = $"{userId:N}/documents/{Guid.NewGuid():N}{Path.GetExtension(safeFileName).ToLowerInvariant()}";
        await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = content,
            AutoCloseStream = false,
            UseChunkEncoding = false,
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true
        }, cancellationToken);
        return key;
    }

    public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        await _client.DeleteObjectAsync(new DeleteObjectRequest { BucketName = _bucket, Key = ValidateKey(storagePath) }, cancellationToken);
    }

    public async Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        var response = await _client.GetObjectAsync(new GetObjectRequest { BucketName = _bucket, Key = ValidateKey(storagePath) }, cancellationToken);
        return response.ResponseStream;
    }

    private static string ValidateKey(string storagePath)
    {
        var key = storagePath.Replace('\\', '/').Trim('/');
        if (string.IsNullOrWhiteSpace(key) || key.Contains("../", StringComparison.Ordinal) || key.StartsWith("../", StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("The storage key is invalid.");
        }

        return key;
    }
}
