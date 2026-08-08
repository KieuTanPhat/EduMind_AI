using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Auth;

namespace StudyAI.Application.Features.Auth.Commands;

public sealed record VerifyEmailOtpCommand(VerifyEmailOtpRequest Request) : IRequest;

public sealed class VerifyEmailOtpCommandValidator : AbstractValidator<VerifyEmailOtpCommand>
{
    public VerifyEmailOtpCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Code).Matches("^[0-9]{6}$").WithMessage("OTP phải gồm 6 chữ số.");
    }
}

public sealed class VerifyEmailOtpCommandHandler : IRequestHandler<VerifyEmailOtpCommand>
{
    private readonly IApplicationDbContext _db;

    public VerifyEmailOtpCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(VerifyEmailOtpCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim().ToUpperInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == email, cancellationToken)
            ?? throw new BadRequestException("Email hoặc OTP không chính xác.");
        var otp = await _db.EmailVerificationOtps.Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new BadRequestException("OTP không chính xác hoặc đã hết hạn.");

        if (!otp.IsUsable(DateTime.UtcNow))
        {
            throw new BadRequestException("OTP đã hết hạn hoặc vượt quá số lần thử.");
        }

        var actualHash = SHA256.HashData(Encoding.UTF8.GetBytes(command.Request.Code.Trim()));
        var expectedHash = Convert.FromBase64String(otp.CodeHash);
        if (!CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
        {
            otp.RecordFailedAttempt();
            await _db.SaveChangesAsync(cancellationToken);
            throw new BadRequestException("OTP không chính xác.");
        }

        user.VerifyEmail();
        otp.MarkUsed(DateTime.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
