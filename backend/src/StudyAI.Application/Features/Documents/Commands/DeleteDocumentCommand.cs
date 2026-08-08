using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Documents.Commands;

public sealed record DeleteDocumentCommand(Guid UserId, Guid DocumentId) : IRequest;

public sealed class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public DeleteDocumentCommandHandler(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task Handle(DeleteDocumentCommand command, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.SingleOrDefaultAsync(x => x.Id == command.DocumentId && x.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");

        _db.Documents.Remove(document);
        await _db.SaveChangesAsync(cancellationToken);
        await _fileStorage.DeleteAsync(document.StoragePath, cancellationToken);
    }
}
