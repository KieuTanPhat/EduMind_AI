using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        RuleFor(x => x.Length).GreaterThan(0).LessThanOrEqualTo(25 * 1024 * 1024);
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
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);
        var uploadedToday = await _db.Documents.CountAsync(x => x.UserId == command.UserId && x.CreatedAtUtc >= todayUtc && x.CreatedAtUtc < tomorrowUtc, cancellationToken);
        if (!user.HasActivePlus(DateTime.UtcNow) && uploadedToday >= 2)
        {
            throw new BadRequestException("Free plan allows up to 2 documents per day. Upgrade to Plus for unlimited uploads.");
        }

        var fileType = ResolveFileType(command.FileName, command.ContentType);
        var storagePath = await _fileStorage.SaveAsync(command.UserId, command.FileName, command.Content, cancellationToken);
        var storedFileName = Path.GetFileName(storagePath);
        var document = new Document(command.UserId, Path.GetFileName(command.FileName), storedFileName, storagePath, fileType, command.Length);

        try
        {
            _db.Documents.Add(document);
            await _db.SaveChangesAsync(cancellationToken);
            await _processingScheduler.EnqueueAsync(document.Id, cancellationToken);
        }
        catch
        {
            await _fileStorage.DeleteAsync(storagePath, CancellationToken.None);
            throw;
        }

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
