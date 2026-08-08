using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyAI.Application.Features.Learning.Commands;
using StudyAI.Application.Features.Learning.Queries;
using StudyAI.Contracts.Learning;

namespace StudyAI.Api.Controllers;

[ApiController]
[Authorize]
public sealed class LearningController : ControllerBase
{
    private readonly ISender _sender;

    public LearningController(ISender sender) => _sender = sender;

    [HttpGet("api/dashboard")]
    public async Task<ActionResult<DashboardResponse>> GetDashboard(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetDashboardQuery(GetUserId()), cancellationToken));

    [HttpGet("api/progress")]
    public async Task<ActionResult<ProgressResponse>> GetProgress(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetProgressQuery(GetUserId()), cancellationToken));

    [HttpPut("api/documents/{documentId:guid}/progress")]
    public async Task<ActionResult<ProgressDocumentResponse>> UpdateProgress(Guid documentId, UpdateProgressRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new UpdateProgressCommand(GetUserId(), documentId, request), cancellationToken));

    [HttpGet("api/recommendations")]
    public async Task<ActionResult<IReadOnlyCollection<RecommendationResponse>>> GetRecommendations(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetRecommendationsQuery(GetUserId()), cancellationToken));

    [HttpPost("api/flashcards/{flashcardId:guid}/review")]
    public async Task<IActionResult> ReviewFlashcard(Guid flashcardId, ReviewFlashcardRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new ReviewFlashcardCommand(GetUserId(), flashcardId, request), cancellationToken);
        return NoContent();
    }

    [HttpGet("api/preferences")]
    public async Task<ActionResult<UserPreferenceResponse>> GetPreferences(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetUserPreferenceQuery(GetUserId()), cancellationToken));

    [HttpPut("api/preferences")]
    public async Task<ActionResult<UserPreferenceResponse>> UpdatePreferences(UpdateUserPreferenceRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new UpdateUserPreferenceCommand(GetUserId(), request), cancellationToken));

    private Guid GetUserId()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            throw new UnauthorizedAccessException("The authenticated user identifier is missing.");
        }

        return userId;
    }
}
