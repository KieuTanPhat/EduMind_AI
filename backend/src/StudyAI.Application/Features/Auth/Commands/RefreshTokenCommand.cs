using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Auth;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.Auth.Commands;

public sealed record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<AuthResponse>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Request.RefreshToken).NotEmpty();
    }
}

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var currentTokenHash = _tokenService.HashRefreshToken(command.Request.RefreshToken);
        var currentToken = await _db.RefreshTokens
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.TokenHash == currentTokenHash, cancellationToken);

        if (currentToken is null || !currentToken.IsActive || !currentToken.User.IsActive)
        {
            throw new UnauthorizedException("The refresh token is invalid or expired.");
        }

        var roles = currentToken.User.UserRoles.Select(x => x.Role.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var replacement = _tokenService.GenerateRefreshToken();
        var replacementHash = _tokenService.HashRefreshToken(replacement.Token);
        currentToken.Revoke(DateTime.UtcNow, replacementHash);
        _db.RefreshTokens.Add(new RefreshToken(currentToken.UserId, replacementHash, replacement.ExpiresAtUtc));
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            currentToken.User.Id,
            currentToken.User.Email,
            currentToken.User.FirstName,
            currentToken.User.LastName,
            roles,
            _tokenService.GenerateAccessToken(currentToken.User, roles),
            _tokenService.GetAccessTokenExpiresAtUtc(),
            replacement.Token,
            replacement.ExpiresAtUtc);
    }
}
