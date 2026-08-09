using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Admin.Commands;

public sealed record MarkSupportTicketsReadCommand(Guid UserId) : IRequest;

public sealed class MarkSupportTicketsReadCommandHandler : IRequestHandler<MarkSupportTicketsReadCommand>
{
    private readonly IApplicationDbContext _db;
    public MarkSupportTicketsReadCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(MarkSupportTicketsReadCommand command, CancellationToken cancellationToken)
    {
        var tickets = await _db.SupportTickets.Where(x => x.UserId == command.UserId && x.Status == "Open").ToListAsync(cancellationToken);
        foreach (var ticket in tickets) ticket.MarkRead(DateTime.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
