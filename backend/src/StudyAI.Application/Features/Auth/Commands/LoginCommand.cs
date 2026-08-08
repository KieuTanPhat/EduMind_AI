using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Auth;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.Auth.Commands;

public sealed record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty()
            .Must(value => value.Equals("admin", StringComparison.OrdinalIgnoreCase) || System.Net.Mail.MailAddress.TryCreate(value, out _))
            .WithMessage("Enter a valid email or the local admin username.");
        RuleFor(x => x.Request.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Request.Email.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase)
            ? "ADMIN@EDUMIND.LOCAL"
            : command.Request.Email.Trim().ToUpperInvariant();
        var user = await _db.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !_passwordHasher.Verify(command.Request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.IsEmailVerified)
        {
            throw new UnauthorizedException("Please verify your email before signing in.");
        }

        var roles = user.UserRoles.Select(x => x.Role.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var refreshToken = _tokenService.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken(user.Id, _tokenService.HashRefreshToken(refreshToken.Token), refreshToken.ExpiresAtUtc));
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            roles,
            _tokenService.GenerateAccessToken(user, roles),
            _tokenService.GetAccessTokenExpiresAtUtc(),
            refreshToken.Token,
            refreshToken.ExpiresAtUtc);
    }
}
