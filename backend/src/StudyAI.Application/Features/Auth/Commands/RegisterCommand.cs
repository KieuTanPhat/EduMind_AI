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
        RuleFor(x => x.Request.CaptchaId).NotEmpty();
        RuleFor(x => x.Request.CaptchaAnswer).NotEmpty().MaximumLength(20);
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
        var captcha = await _db.CaptchaChallenges.SingleOrDefaultAsync(x => x.Id == command.Request.CaptchaId, cancellationToken)
            ?? throw new BadRequestException("CAPTCHA không hợp lệ hoặc đã hết hạn.");
        if (!captcha.IsUsable(DateTime.UtcNow))
        {
            throw new BadRequestException("CAPTCHA đã hết hạn. Vui lòng lấy mã CAPTCHA mới.");
        }

        var captchaHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(command.Request.CaptchaAnswer.Trim().ToUpperInvariant())));
        if (!CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(captcha.AnswerHash), Convert.FromBase64String(captchaHash)))
        {
            throw new BadRequestException("CAPTCHA không chính xác.");
        }
        captcha.MarkUsed(DateTime.UtcNow);

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

        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var otpHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(otp)));
        var otpExpiresAtUtc = DateTime.UtcNow.AddMinutes(10);
        _db.EmailVerificationOtps.Add(new EmailVerificationOtp(user.Id, otpHash, otpExpiresAtUtc));
        await _db.SaveChangesAsync(cancellationToken);

        var developmentOtp = await _emailService.SendEmailVerificationOtpAsync(user.Email, $"{user.FirstName} {user.LastName}".Trim(), otp, cancellationToken);
        return new RegisterResponse(user.Id, user.Email, true, otpExpiresAtUtc, developmentOtp);
    }
}
