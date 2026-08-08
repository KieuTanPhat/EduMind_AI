using System.Security.Cryptography;
using System.Text;
using MediatR;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Auth;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.Auth.Queries;

public sealed record GetCaptchaQuery : IRequest<CaptchaResponse>;

public sealed class GetCaptchaQueryHandler : IRequestHandler<GetCaptchaQuery, CaptchaResponse>
{
    private readonly IApplicationDbContext _db;

    public GetCaptchaQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CaptchaResponse> Handle(GetCaptchaQuery query, CancellationToken cancellationToken)
    {
        var first = RandomNumberGenerator.GetInt32(2, 10);
        var second = RandomNumberGenerator.GetInt32(1, 10);
        var isAddition = RandomNumberGenerator.GetInt32(0, 2) == 0;
        var question = isAddition ? $"{first} + {second} = ?" : $"{first + second} - {second} = ?";
        var answer = isAddition ? first + second : first;
        var answerHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(answer.ToString())));
        var challenge = new CaptchaChallenge(question, answerHash, DateTime.UtcNow.AddMinutes(5));
        _db.CaptchaChallenges.Add(challenge);
        await _db.SaveChangesAsync(cancellationToken);
        return new CaptchaResponse(challenge.Id, challenge.Question, challenge.ExpiresAtUtc);
    }
}
