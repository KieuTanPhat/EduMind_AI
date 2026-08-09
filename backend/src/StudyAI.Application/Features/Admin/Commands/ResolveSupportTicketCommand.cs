using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Admin.Commands;

public sealed record ResolveSupportTicketCommand(Guid TicketId, string Reply) : IRequest;
public sealed class ResolveSupportTicketCommandHandler : IRequestHandler<ResolveSupportTicketCommand>
{
    private readonly IApplicationDbContext _db;
    public ResolveSupportTicketCommandHandler(IApplicationDbContext db) => _db = db;
    public async Task Handle(ResolveSupportTicketCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Reply) || command.Reply.Length > 4000) throw new BadRequestException("A reply is required.");
        var ticket = await _db.SupportTickets.SingleOrDefaultAsync(x => x.Id == command.TicketId, cancellationToken) ?? throw new NotFoundException("Support ticket was not found.");
        ticket.Reply(DateTime.UtcNow, command.Reply.Trim());
        await _db.SaveChangesAsync(cancellationToken);
    }
}
