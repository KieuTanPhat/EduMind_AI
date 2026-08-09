using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Billing;

namespace StudyAI.Application.Features.Billing.Commands;

public sealed record ProcessSepayWebhookCommand(SepayWebhookPayload Payload) : IRequest<SepayWebhookResult>;

public sealed class ProcessSepayWebhookCommandHandler : IRequestHandler<ProcessSepayWebhookCommand, SepayWebhookResult>
{
    private readonly IApplicationDbContext _db;

    public ProcessSepayWebhookCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<SepayWebhookResult> Handle(ProcessSepayWebhookCommand command, CancellationToken cancellationToken)
    {
        var payload = command.Payload;
        if (!string.Equals(payload.TransferType, "in", StringComparison.OrdinalIgnoreCase))
        {
            return new SepayWebhookResult(false, "Ignored: transaction is not incoming.");
        }

        var sepayTransactionId = payload.Id > 0
            ? payload.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : payload.ReferenceCode?.Trim();
        if (!string.IsNullOrWhiteSpace(sepayTransactionId) && await _db.PlusRequests.AnyAsync(x => x.SepayTransactionId == sepayTransactionId, cancellationToken))
        {
            return new SepayWebhookResult(true, "Already processed.");
        }

        var code = payload.Code?.Trim();
        var content = payload.Content?.Trim() ?? string.Empty;
        var request = await _db.PlusRequests
            .Include(x => x.User)
            .Where(x => x.Status == "Pending")
            .OrderBy(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(x => (code != null && x.TransferContent == code) || content.Contains(x.TransferContent), cancellationToken);

        if (request is null)
        {
            return new SepayWebhookResult(false, "Ignored: no matching pending payment order.");
        }

        if (request.IsExpired(DateTime.UtcNow))
        {
            request.Expire(DateTime.UtcNow);
            await _db.SaveChangesAsync(cancellationToken);
            return new SepayWebhookResult(false, "Ignored: payment order has expired.");
        }

        if (payload.TransferAmount != request.AmountVnd)
        {
            throw new BadRequestException("Payment amount does not match the payment order.");
        }

        var now = DateTime.UtcNow;
        var expires = now.AddDays(30);
        if (string.Equals(request.Plan, "Pro", StringComparison.OrdinalIgnoreCase)) request.User.GrantPro(now, expires);
        else request.User.GrantPlus(now, expires);
        request.ApproveAutomatically(now, sepayTransactionId ?? $"test:{Guid.NewGuid():N}", $"Tự động xác nhận SePay {sepayTransactionId ?? "test"}.");
        await _db.SaveChangesAsync(cancellationToken);

        return new SepayWebhookResult(true, "Payment processed and plan activated.");
    }
}
