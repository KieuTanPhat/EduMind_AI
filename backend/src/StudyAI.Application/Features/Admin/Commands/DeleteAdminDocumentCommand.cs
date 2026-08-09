using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Admin.Commands;

public sealed record DeleteAdminDocumentCommand(Guid DocumentId) : IRequest;

public sealed class DeleteAdminDocumentCommandHandler : IRequestHandler<DeleteAdminDocumentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public DeleteAdminDocumentCommandHandler(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task Handle(DeleteAdminDocumentCommand command, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.SingleOrDefaultAsync(x => x.Id == command.DocumentId, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");

        _db.Documents.Remove(document);
        await _db.SaveChangesAsync(cancellationToken);
        await _fileStorage.DeleteAsync(document.StoragePath, cancellationToken);
    }
}
