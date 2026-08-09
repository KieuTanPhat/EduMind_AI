using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Documents;
using StudyAI.Domain.Entities;
using StudyAI.Domain.Enums;

namespace StudyAI.Application.Features.Documents.Commands;

public sealed record UploadDocumentCommand(
    Guid UserId,
    string FileName,
    string ContentType,
    long Length,
    Stream Content) : IRequest<UploadDocumentResponse>;

public sealed class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Length).GreaterThan(0).LessThanOrEqualTo(200 * 1024 * 1024);
        RuleFor(x => x.Content).NotNull();
    }
}

public sealed class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, UploadDocumentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly IDocumentProcessingScheduler _processingScheduler;

    public UploadDocumentCommandHandler(
        IApplicationDbContext db,
        IFileStorageService fileStorage,
        IDocumentProcessingScheduler processingScheduler)
    {
        _db = db;
        _fileStorage = fileStorage;
        _processingScheduler = processingScheduler;
    }

    public async Task<UploadDocumentResponse> Handle(UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == command.UserId && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        var policy = await _db.PlanPolicies.AsNoTracking().SingleOrDefaultAsync(x => x.Plan == user.Plan, cancellationToken);
        var maxUploadBytes = (policy?.MaxUploadSizeMb ?? (user.IsPro ? 50 : 25)) * 1024L * 1024L;
        if (command.Length > maxUploadBytes)
        {
            throw new BadRequestException($"Your {user.Plan} plan allows uploads up to {maxUploadBytes / (1024 * 1024)} MB.");
        }
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);
        var uploadedToday = await _db.Documents.CountAsync(x => x.UserId == command.UserId && x.CreatedAtUtc >= todayUtc && x.CreatedAtUtc < tomorrowUtc, cancellationToken);
        var dailyDocumentLimit = policy?.DailyDocumentLimit ?? (user.HasActivePlus(DateTime.UtcNow) ? null : 2);
        if (dailyDocumentLimit.HasValue && uploadedToday >= dailyDocumentLimit.Value)
        {
            throw new BadRequestException($"{user.Plan} plan allows up to {dailyDocumentLimit.Value} documents per day. Upgrade your plan to continue.");
        }

        MemoryStream? bufferedContent = null;
        var content = command.Content;
        if (!content.CanSeek)
        {
            bufferedContent = new MemoryStream();
            await content.CopyToAsync(bufferedContent, cancellationToken);
            bufferedContent.Position = 0;
            content = bufferedContent;
        }

        content.Position = 0;
        var contentHash = Convert.ToHexString(await SHA256.HashDataAsync(content, cancellationToken));
        content.Position = 0;
        var existingDocuments = await _db.Documents.Where(x => x.UserId == command.UserId).ToListAsync(cancellationToken);
        var duplicateExists = false;
        foreach (var existingDocument in existingDocuments)
        {
            if (existingDocument.ContentHash == contentHash)
            {
                duplicateExists = true;
                break;
            }

            if (existingDocument.ContentHash is not null)
            {
                continue;
            }

            try
            {
                await using var existingContent = await _fileStorage.OpenReadAsync(existingDocument.StoragePath, cancellationToken);
                var existingHash = Convert.ToHexString(await SHA256.HashDataAsync(existingContent, cancellationToken));
                existingDocument.SetContentHash(existingHash);
                duplicateExists = existingHash == contentHash;
                if (duplicateExists)
                {
                    break;
                }
            }
            catch
            {
                // A missing legacy file should not prevent new uploads.
            }
        }

        if (existingDocuments.Any(x => x.ContentHash is not null))
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        if (duplicateExists)
        {
            bufferedContent?.Dispose();
            throw new BadRequestException("This document has already been uploaded to your account.");
        }

        var fileType = ResolveFileType(command.FileName, command.ContentType);
        var storagePath = await _fileStorage.SaveAsync(command.UserId, command.FileName, content, cancellationToken);
        var storedFileName = Path.GetFileName(storagePath);
        var document = new Document(command.UserId, Path.GetFileName(command.FileName), storedFileName, storagePath, fileType, command.Length, contentHash);

        try
        {
            _db.Documents.Add(document);
            await _db.SaveChangesAsync(cancellationToken);
            await _processingScheduler.EnqueueAsync(document.Id, cancellationToken);
        }
        catch
        {
            await _fileStorage.DeleteAsync(storagePath, CancellationToken.None);
            bufferedContent?.Dispose();
            throw;
        }

        bufferedContent?.Dispose();

        return new UploadDocumentResponse(document.Id, document.OriginalFileName, document.Status.ToString(), document.CreatedAtUtc);
    }

    private static DocumentFileType ResolveFileType(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" when contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) => DocumentFileType.Pdf,
            ".docx" when contentType.Equals("application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase) => DocumentFileType.Docx,
            ".txt" when contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) => DocumentFileType.Txt,
            _ => throw new BadRequestException("Only PDF, DOCX and TXT files with a matching MIME type are supported.")
        };
    }
}
