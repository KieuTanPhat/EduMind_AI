using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Auth.Commands;

public sealed record VerifyEmailCommand(string Token) : IRequest;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand>
{
    private readonly IApplicationDbContext _db;

    public VerifyEmailCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
        {
            throw new BadRequestException("Verification token is required.");
        }

        var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(command.Token)));
        var token = await _db.EmailVerificationTokens.Include(x => x.User).SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken)
            ?? throw new BadRequestException("This verification link is invalid or expired.");
        if (!token.IsUsable(DateTime.UtcNow))
        {
            throw new BadRequestException("This verification link is invalid or expired.");
        }

        token.User.VerifyEmail();
        token.MarkUsed(DateTime.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
