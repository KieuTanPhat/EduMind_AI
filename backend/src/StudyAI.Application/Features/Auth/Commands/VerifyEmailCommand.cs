using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Domain.Entities;

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
        var pending = await _db.PendingRegistrations.SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (pending is not null)
        {
            if (!pending.IsUsable(DateTime.UtcNow))
            {
                throw new BadRequestException("This verification link is invalid or expired.");
            }

            if (await _db.Users.AnyAsync(x => x.NormalizedEmail == pending.NormalizedEmail, cancellationToken))
            {
                _db.PendingRegistrations.Remove(pending);
                await _db.SaveChangesAsync(cancellationToken);
                throw new ConflictException("An account with this email already exists.");
            }

            var userRole = await _db.Roles.SingleOrDefaultAsync(x => x.NormalizedName == "USER", cancellationToken)
                ?? throw new InvalidOperationException("The default User role has not been seeded.");
            var user = new User(pending.Email, pending.NormalizedEmail, pending.PasswordHash, pending.FirstName, pending.LastName);
            user.VerifyEmail();
            user.UserRoles.Add(new UserRole(user.Id, userRole.Id));
            user.SetPreference(new UserPreference(user.Id));
            _db.Users.Add(user);
            _db.PendingRegistrations.Remove(pending);
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        // Keep previously issued links working after the new pending-registration flow is deployed.
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
