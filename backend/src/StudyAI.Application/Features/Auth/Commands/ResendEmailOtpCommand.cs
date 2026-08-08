using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Auth;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.Auth.Commands;

public sealed record ResendEmailOtpCommand(ResendEmailOtpRequest Request) : IRequest<ResendOtpResponse>;

public sealed record ResendOtpResponse(DateTime OtpExpiresAtUtc, string? DevelopmentOtp);

public sealed class ResendEmailOtpCommandValidator : AbstractValidator<ResendEmailOtpCommand>
{
    public ResendEmailOtpCommandValidator() => RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
}

public sealed class ResendEmailOtpCommandHandler : IRequestHandler<ResendEmailOtpCommand, ResendOtpResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _emailService;

    public ResendEmailOtpCommandHandler(IApplicationDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<ResendOtpResponse> Handle(ResendEmailOtpCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim().ToUpperInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == email, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy tài khoản.");
        if (user.IsEmailVerified)
        {
            throw new BadRequestException("Email này đã được xác nhận.");
        }

        var lastOtp = await _db.EmailVerificationOtps.Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        if (lastOtp is not null && lastOtp.CreatedAtUtc > DateTime.UtcNow.AddSeconds(-60))
        {
            throw new BadRequestException("Vui lòng đợi 60 giây trước khi gửi lại OTP.");
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(10);
        _db.EmailVerificationOtps.Add(new EmailVerificationOtp(user.Id, Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(code))), expiresAtUtc));
        await _db.SaveChangesAsync(cancellationToken);
        var developmentOtp = await _emailService.SendEmailVerificationOtpAsync(user.Email, $"{user.FirstName} {user.LastName}".Trim(), code, cancellationToken);
        return new ResendOtpResponse(expiresAtUtc, developmentOtp);
    }
}
