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

public sealed record ResendEmailVerificationCommand(ResendEmailVerificationRequest Request) : IRequest<ResendVerificationResponse>;

public sealed record ResendVerificationResponse(DateTime VerificationExpiresAtUtc);

public sealed class ResendEmailVerificationCommandValidator : AbstractValidator<ResendEmailVerificationCommand>
{
    public ResendEmailVerificationCommandValidator() => RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
}

public sealed class ResendEmailVerificationCommandHandler : IRequestHandler<ResendEmailVerificationCommand, ResendVerificationResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _emailService;

    public ResendEmailVerificationCommandHandler(IApplicationDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<ResendVerificationResponse> Handle(ResendEmailVerificationCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim().ToUpperInvariant();
        var pending = await _db.PendingRegistrations.SingleOrDefaultAsync(x => x.NormalizedEmail == email, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy yêu cầu đăng ký đang chờ xác nhận.");

        var lastToken = await _db.PendingRegistrations.Where(x => x.NormalizedEmail == email)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        if (lastToken is not null && lastToken.CreatedAtUtc > DateTime.UtcNow.AddSeconds(-60))
        {
            throw new BadRequestException("Vui lòng đợi 60 giây trước khi gửi lại email.");
        }

        var verificationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var expiresAtUtc = DateTime.UtcNow.AddHours(24);
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(verificationToken)));
        _db.PendingRegistrations.Remove(pending);
        _db.PendingRegistrations.Add(new PendingRegistration(pending.Email, pending.NormalizedEmail, pending.PasswordHash, pending.FirstName, pending.LastName, tokenHash, expiresAtUtc));
        await _db.SaveChangesAsync(cancellationToken);
        await _emailService.SendEmailVerificationAsync(pending.Email, $"{pending.FirstName} {pending.LastName}".Trim(), verificationToken, cancellationToken);
        return new ResendVerificationResponse(expiresAtUtc);
    }
}
