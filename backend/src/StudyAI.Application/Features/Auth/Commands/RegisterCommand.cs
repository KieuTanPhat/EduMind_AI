using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Auth;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.Auth.Commands;

public sealed record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]");

        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.LastName).NotEmpty().MaximumLength(100);
    }
}

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();

        if (await _db.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var userRole = await _db.Roles.SingleOrDefaultAsync(x => x.NormalizedName == "USER", cancellationToken)
            ?? throw new InvalidOperationException("The default User role has not been seeded.");

        var user = new User(
            email,
            normalizedEmail,
            _passwordHasher.Hash(command.Request.Password),
            command.Request.FirstName.Trim(),
            command.Request.LastName.Trim());

        user.UserRoles.Add(new UserRole(user.Id, userRole.Id));
        user.SetPreference(new UserPreference(user.Id));
        _db.Users.Add(user);

        var refreshToken = _tokenService.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken(user.Id, _tokenService.HashRefreshToken(refreshToken.Token), refreshToken.ExpiresAtUtc));
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            new[] { userRole.Name },
            _tokenService.GenerateAccessToken(user, new[] { userRole.Name }),
            _tokenService.GetAccessTokenExpiresAtUtc(),
            refreshToken.Token,
            refreshToken.ExpiresAtUtc);
    }
}
