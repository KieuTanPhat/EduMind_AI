using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Auth;

namespace StudyAI.Application.Features.Auth.Commands;

public sealed record LogoutCommand(LogoutRequest Request) : IRequest;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.Request.RefreshToken).NotEmpty();
    }
}

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(IApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashRefreshToken(command.Request.RefreshToken);
        var token = await _db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (token is not null && token.RevokedAtUtc is null)
        {
            token.Revoke(DateTime.UtcNow);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
