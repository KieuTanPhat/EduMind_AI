using System.Security.Cryptography;
using System.Text;
using System.Net;
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
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var answer = new string(Enumerable.Range(0, 5).Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]).ToArray());
        var answerHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(answer)));
        var challenge = new CaptchaChallenge("image", answerHash, DateTime.UtcNow.AddMinutes(5));
        _db.CaptchaChallenges.Add(challenge);
        await _db.SaveChangesAsync(cancellationToken);
        return new CaptchaResponse(challenge.Id, BuildImageDataUrl(answer), challenge.ExpiresAtUtc);
    }

    private static string BuildImageDataUrl(string answer)
    {
        var random = new Random();
        var width = 270;
        var height = 88;
        var background = random.Next(245, 253);
        var text = new StringBuilder();
        for (var index = 0; index < answer.Length; index++)
        {
            var x = 28 + index * 45;
            var y = random.Next(54, 68);
            var rotation = random.Next(-18, 19);
            var color = $"#{random.Next(30, 100):X2}{random.Next(55, 130):X2}{random.Next(100, 190):X2}";
            text.Append($"<text x='{x}' y='{y}' fill='{color}' transform='rotate({rotation} {x} {y})'>{WebUtility.HtmlEncode(answer[index].ToString())}</text>");
        }

        var lines = new StringBuilder();
        for (var index = 0; index < 7; index++)
        {
            lines.Append($"<path d='M{random.Next(0, width / 2)} {random.Next(10, height - 10)} Q{random.Next(60, 210)} {random.Next(0, height)} {random.Next(width / 2, width)} {random.Next(10, height - 10)}' />");
        }

        var svg = $"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}' viewBox='0 0 {width} {height}'><rect width='100%' height='100%' rx='14' fill='rgb({background},{background},{background})'/><g fill='none' stroke='#9fb5d0' stroke-width='2' opacity='.65'>{lines}</g><g font-family='Arial,sans-serif' font-size='42' font-weight='700' letter-spacing='3'>{text}</g></svg>";
        return $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(svg))}";
    }
}
