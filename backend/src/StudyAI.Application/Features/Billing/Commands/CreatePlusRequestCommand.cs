using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Billing;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.Billing.Commands;

public sealed record CreatePlusRequestCommand(Guid UserId, PlusRequestRequest Request) : IRequest<PlusRequestResponse>;

public sealed class CreatePlusRequestCommandHandler : IRequestHandler<CreatePlusRequestCommand, PlusRequestResponse>
{
    private readonly IApplicationDbContext _db;
    public CreatePlusRequestCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<PlusRequestResponse> Handle(CreatePlusRequestCommand command, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == command.UserId && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        if (user.HasActivePlus(DateTime.UtcNow)) throw new BadRequestException("Your Plus plan is already active.");
        var existing = await _db.PlusRequests.Where(x => x.UserId == command.UserId && x.Status == "Pending").OrderByDescending(x => x.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        if (existing is not null) return Map(existing);
        var request = new PlusRequest(user.Id, user.Email, 49000m, $"{user.Email} DKI PLUS");
        if (!string.IsNullOrWhiteSpace(command.Request.Note)) request.SetNote(command.Request.Note.Trim());
        _db.PlusRequests.Add(request);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(request);
    }

    private static PlusRequestResponse Map(PlusRequest request) => new(request.Id, request.AmountVnd, request.TransferContent, request.Status, request.CreatedAtUtc);
}
