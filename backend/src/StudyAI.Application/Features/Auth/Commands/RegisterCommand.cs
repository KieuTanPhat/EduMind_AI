using FluentValidation;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Auth;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.Auth.Commands;

public sealed record RegisterCommand(RegisterRequest Request) : IRequest<RegisterResponse>;

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
            .Matches("[A-Z]").WithMessage("Mật khẩu phải có ít nhất một chữ hoa.")
            .Matches("[a-z]").WithMessage("Mật khẩu phải có ít nhất một chữ thường.")
            .Matches("[0-9]").WithMessage("Mật khẩu phải có ít nhất một chữ số.");

        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.LastName).NotEmpty().MaximumLength(100);
    }
}

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, IEmailService emailService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
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

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        _db.EmailVerificationTokens.Add(new EmailVerificationToken(user.Id, tokenHash, DateTime.UtcNow.AddHours(24)));
        await _db.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailVerificationAsync(user.Email, $"{user.FirstName} {user.LastName}".Trim(), rawToken, cancellationToken);
        return new RegisterResponse(user.Id, user.Email, true);
    }
}
