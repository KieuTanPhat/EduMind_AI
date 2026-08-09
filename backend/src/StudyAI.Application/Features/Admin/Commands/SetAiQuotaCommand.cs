using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Admin.Commands;

public sealed record SetAiQuotaCommand(Guid UserId, long? TokenLimitPerDay) : IRequest;

public sealed class SetAiQuotaCommandHandler : IRequestHandler<SetAiQuotaCommand>
{
    private readonly IApplicationDbContext _db;

    public SetAiQuotaCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(SetAiQuotaCommand command, CancellationToken cancellationToken)
    {
        if (command.TokenLimitPerDay is < 0)
        {
            throw new BadRequestException("Token quota cannot be negative.");
        }

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        user.SetAiTokenLimitPerDay(command.TokenLimitPerDay);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
