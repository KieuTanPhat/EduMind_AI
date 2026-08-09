using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;
using StudyAI.Domain.Entities;
using StudyAI.Domain.Enums;

namespace StudyAI.Application.Features.AI.Commands;

public sealed record GenerateCvScoreCommand(
    Guid UserId,
    Guid DocumentId,
    string TargetRole,
    string ExperienceLevel,
    string? JobDescription) : IRequest<CvScoreResponse>;

public sealed class GenerateCvScoreCommandHandler : IRequestHandler<GenerateCvScoreCommand, CvScoreResponse>
{
    private const string PromptVersion = "it-resume-v2-evidence-rubric";
    private static readonly JsonSerializerOptions CacheJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly string[] AllowedRoles =
    [
        "Backend Developer", "Frontend Developer", "Full-stack Developer", "Software Engineer",
        "Mobile Developer", "DevOps Engineer", "Cloud Engineer", "Data Analyst", "Data Engineer",
        "AI/ML Engineer", "QA Engineer", "Cybersecurity Engineer"
    ];

    private static readonly (string Name, string Property, int MaxScore)[] Categories =
    [
        ("Technical Competence", "technical_competence", 30),
        ("Project Quality & Technical Depth", "project_quality", 20),
        ("Relevant Experience", "relevant_experience", 15),
        ("Role Relevance & Technical Alignment", "role_relevance", 10),
        ("Engineering Practices", "engineering_practices", 10),
        ("Resume Quality & ATS Readability", "resume_quality_ats", 10)
    ];

    private readonly IApplicationDbContext _db;
    private readonly IAiService _aiService;

    public GenerateCvScoreCommandHandler(IApplicationDbContext db, IAiService aiService)
    {
        _db = db;
        _aiService = aiService;
    }

    public async Task<CvScoreResponse> Handle(GenerateCvScoreCommand command, CancellationToken cancellationToken)
    {
        var targetRole = AllowedRoles.FirstOrDefault(role => role.Equals(command.TargetRole.Trim(), StringComparison.OrdinalIgnoreCase));
        if (targetRole is null)
        {
            throw new BadRequestException("CV scoring only supports software and IT roles.");
        }

        var experienceLevel = command.ExperienceLevel.Trim().ToLowerInvariant() switch
        {
            "intern" => "Intern",
            "fresher" => "Fresher",
            "junior" => "Junior",
            "mid-level" or "mid" => "Mid-level",
            "senior" => "Senior",
            _ => throw new BadRequestException("Choose a supported IT experience level.")
        };

        var document = await _db.Documents.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == command.DocumentId && x.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");
        if (document.FileType != DocumentFileType.Pdf)
        {
            throw new BadRequestException("CV scoring only accepts PDF documents.");
        }
        if (document.Status != DocumentStatus.Processed || string.IsNullOrWhiteSpace(document.ExtractedText))
        {
            throw new BadRequestException("The PDF must finish processing before it can be scored.");
        }

        var jobDescription = NormalizeJobDescription(command.JobDescription);
        var jobDescriptionHash = HashKey($"{PromptVersion}\n{jobDescription}");
        var cached = await _db.CvScoreSnapshots.AsNoTracking().SingleOrDefaultAsync(x =>
            x.UserId == command.UserId &&
            x.DocumentId == command.DocumentId &&
            x.TargetRole == targetRole &&
            x.ExperienceLevel == experienceLevel &&
            x.JobDescriptionHash == jobDescriptionHash, cancellationToken);

        if (cached is not null)
        {
            try
            {
                var cachedResponse = JsonSerializer.Deserialize<CvScoreResponse>(cached.ResponseJson, CacheJsonOptions);
                if (cachedResponse is not null)
                {
                    return cachedResponse;
                }
            }
            catch (JsonException)
            {
                // Re-score only if an old/corrupt snapshot cannot be read.
            }
        }

        await EntitlementPolicy.EnsureDailyPlanOperationAllowanceAsync(_db, command.UserId, "cv_score", 3, 9, cancellationToken);
        await EntitlementPolicy.EnsureDailyAiTokenAllowanceAsync(_db, command.UserId, cancellationToken);

        var prompt = $"{AiPromptTemplates.CvScore}\n\nTARGET ROLE: {targetRole}\nCAREER LEVEL: {experienceLevel}\nJOB DESCRIPTION: {jobDescription}\n\nAll narrative fields must be concise Vietnamese. Return integer scores within each category maximum. The six category weights total 95 by design; the server will normalize their raw sum to the final 0-100 score.";
        var result = await _aiService.GenerateAsync(
            new AiGenerationRequest("cv_score", CompactResume(document.ExtractedText), prompt, true, 5200),
            cancellationToken);

        using var json = AiJsonHelpers.Parse(result.Text);
        var categories = MapCategories(json.RootElement);
        var total = NormalizeToHundred(categories.Sum(category => category.Score));
        var jobMatch = ReadJobMatch(json.RootElement);
        var response = new CvScoreResponse(
            document.Id,
            document.OriginalFileName,
            targetRole,
            experienceLevel,
            total,
            Grade(total),
            ReadFinalSummary(json.RootElement),
            categories,
            ReadObjectArray(json.RootElement, "strengths", "title", "evidence", 5),
            ReadWeaknesses(json.RootElement),
            ReadRecommendations(json.RootElement),
            ReadCareerReadiness(json.RootElement),
            jobMatch,
            ReadStringArray(json.RootElement, "warnings", 4),
            result.Model,
            result.InputTokens,
            result.OutputTokens);

        _db.CvScoreSnapshots.Add(new CvScoreSnapshot(
            command.UserId,
            command.DocumentId,
            targetRole,
            experienceLevel,
            jobDescriptionHash,
            JsonSerializer.Serialize(response, CacheJsonOptions),
            result.Model,
            result.InputTokens,
            result.OutputTokens));
        _db.AiUsageLogs.Add(new AiUsageLog(command.UserId, "cv_score", result.Model, result.InputTokens, result.OutputTokens));
        await _db.SaveChangesAsync(cancellationToken);
        return response;
    }

    private static string NormalizeJobDescription(string? value)
    {
        var normalized = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(normalized)
            ? "No job description supplied. Evaluate against general expectations for the target IT role."
            : normalized[..Math.Min(normalized.Length, 2500)];
    }

    private static string HashKey(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static int NormalizeToHundred(int rawScore)
        => Math.Clamp((int)Math.Round(rawScore * 100d / 95d, MidpointRounding.AwayFromZero), 0, 100);

    private static string CompactResume(string text)
    {
        var normalized = string.Join('\n', text.Split('\n').Select(line => string.Join(' ', line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))).Where(line => line.Length > 0));
        const int maxCharacters = 16000;
        if (normalized.Length <= maxCharacters) return normalized;
        var head = normalized[..12000];
        var tail = normalized[^4000..];
        return $"{head}\n\n[RESUME CONTENT COMPACTED TO CONTROL TOKEN USAGE]\n\n{tail}";
    }

    private static IReadOnlyCollection<CvScoreCategoryResponse> MapCategories(JsonElement root)
    {
        var baseScore = root.TryGetProperty("base_score", out var value) && value.ValueKind == JsonValueKind.Object
            ? value
            : default;
        var legacyCategories = root.TryGetProperty("categories", out var categories) && categories.ValueKind == JsonValueKind.Array
            ? categories.EnumerateArray().ToArray()
            : Array.Empty<JsonElement>();

        return Categories.Select(expected =>
        {
            var score = Math.Clamp(ReadInt(baseScore, expected.Property), 0, expected.MaxScore);
            var item = legacyCategories.FirstOrDefault(candidate => string.Equals(ReadString(candidate, "name", string.Empty), expected.Name, StringComparison.OrdinalIgnoreCase));
            return new CvScoreCategoryResponse(
                expected.Name,
                expected.MaxScore,
                score,
                ScoreLevel(score, expected.MaxScore),
                ReadString(item, "reason", "Điểm được xác định từ bằng chứng cụ thể trong CV."),
                ReadStringArray(item, "evidence_found", 3),
                ReadStringArray(item, "evidence_missing", 3));
        }).ToArray();
    }

    private static CvScoreJobMatchResponse ReadJobMatch(JsonElement root)
    {
        if (!root.TryGetProperty("ats_analysis", out var ats) || ats.ValueKind != JsonValueKind.Object)
        {
            return new CvScoreJobMatchResponse(0, "Chưa có mô tả công việc để đối chiếu.");
        }

        var score = ats.TryGetProperty("job_match_score", out var rawScore) && rawScore.ValueKind == JsonValueKind.Number && rawScore.TryGetInt32(out var number)
            ? Math.Clamp(number, 0, 100)
            : 0;
        var summary = ReadString(ats, "ats_readability", score == 0 ? "Chưa đủ bằng chứng để đối chiếu." : "Mức phù hợp được đối chiếu với mô tả công việc.");
        return new CvScoreJobMatchResponse(score, summary);
    }

    private static string ReadFinalSummary(JsonElement root)
    {
        if (root.TryGetProperty("final_assessment", out var assessment) && assessment.ValueKind == JsonValueKind.Object)
        {
            return ReadString(assessment, "summary", "CV được đánh giá dựa trên bằng chứng rõ ràng trong nội dung PDF.");
        }

        return ReadString(root, "summary", "CV được đánh giá dựa trên bằng chứng rõ ràng trong nội dung PDF.");
    }

    private static IReadOnlyCollection<string> ReadWeaknesses(JsonElement root)
        => ReadObjectArray(root, "weaknesses", "title", "evidence", 5);

    private static IReadOnlyCollection<string> ReadRecommendations(JsonElement root)
    {
        if (!root.TryGetProperty("recommendations", out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return value.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object).Take(5).Select(item =>
        {
            var problem = ReadString(item, "problem", "");
            var action = ReadString(item, "action", "");
            var evidence = ReadString(item, "evidence_to_add", "");
            return string.Join(" ", new[] { problem, action, evidence }.Where(part => !string.IsNullOrWhiteSpace(part)));
        }).Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
    }

    private static IReadOnlyCollection<string> ReadCareerReadiness(JsonElement root)
    {
        if (!root.TryGetProperty("final_assessment", out var value) || value.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<string>();
        }

        return new[]
        {
            ReadString(value, "competitive_level", ""),
            ReadString(value, "interview_readiness", "")
        }.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
    }

    private static IReadOnlyCollection<string> ReadObjectArray(JsonElement root, string property, string titleProperty, string evidenceProperty, int limit)
    {
        if (!root.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return value.EnumerateArray().Take(limit).Select(item => item.ValueKind == JsonValueKind.Object
            ? string.Join(": ", new[] { ReadString(item, titleProperty, ""), ReadString(item, evidenceProperty, "") }.Where(part => !string.IsNullOrWhiteSpace(part)))
            : item.ValueKind == JsonValueKind.String ? item.GetString()?.Trim() ?? "" : "")
            .Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
    }

    private static string ReadString(JsonElement element, string property, string fallback)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()!.Trim()[..Math.Min(value.GetString()!.Trim().Length, 1200)]
            : fallback;

    private static int ReadInt(JsonElement element, string property)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value) && value.TryGetInt32(out var number) ? number : 0;

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string property, int limit)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return value.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString()?.Trim()).Where(item => !string.IsNullOrWhiteSpace(item)).Take(limit).Cast<string>().ToArray();
    }

    private static string Grade(int score) => score switch
    {
        >= 90 => "Exceptional",
        >= 80 => "Very strong",
        >= 70 => "Good",
        >= 60 => "Average / acceptable",
        >= 50 => "Weak",
        >= 40 => "Very weak",
        _ => "Insufficient evidence"
    };

    private static string ScoreLevel(int score, int maxScore) => score >= maxScore * .8 ? "Strong" : score >= maxScore * .6 ? "Adequate" : score >= maxScore * .4 ? "Limited" : "Weak";
}
