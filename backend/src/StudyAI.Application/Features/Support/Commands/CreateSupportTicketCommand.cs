using MediatR;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Support;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.Support.Commands;

public sealed record CreateSupportTicketCommand(Guid UserId, CreateSupportTicketRequest Request) : IRequest<SupportTicketResponse>;

public sealed class CreateSupportTicketCommandHandler : IRequestHandler<CreateSupportTicketCommand, SupportTicketResponse>
{
    private readonly IApplicationDbContext _db;
    public CreateSupportTicketCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<SupportTicketResponse> Handle(CreateSupportTicketCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Request.Subject) || command.Request.Subject.Length > 200 || string.IsNullOrWhiteSpace(command.Request.Message) || command.Request.Message.Length > 4000)
            throw new Application.Common.Exceptions.BadRequestException("Subject and message are required.");
        var ticket = new SupportTicket(command.UserId, command.Request.Subject.Trim(), command.Request.Message.Trim());
        _db.SupportTickets.Add(ticket);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(ticket);
    }

    private static SupportTicketResponse Map(SupportTicket ticket) => new(ticket.Id, ticket.Subject, ticket.Message, ticket.Status, ticket.AdminReply, ticket.CreatedAtUtc, ticket.ResolvedAtUtc);
}
