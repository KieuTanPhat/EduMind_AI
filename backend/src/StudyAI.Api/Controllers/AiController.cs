using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyAI.Application.Features.AI.Commands;
using StudyAI.Application.Features.AI.Queries;
using StudyAI.Contracts.AI;

namespace StudyAI.Api.Controllers;

[ApiController]
[Authorize]
public sealed class AiController : ControllerBase
{
    private readonly ISender _sender;

    public AiController(ISender sender) => _sender = sender;

    [HttpPost("api/documents/{documentId:guid}/summary")]
    public async Task<ActionResult<SummaryResponse>> GenerateSummary(Guid documentId, GenerateAiRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GenerateSummaryCommand(GetUserId(), documentId, request.ForceRegenerate), cancellationToken));

    [HttpGet("api/documents/{documentId:guid}/summary")]
    public async Task<ActionResult<SummaryResponse>> GetSummary(Guid documentId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetSummaryQuery(GetUserId(), documentId), cancellationToken));

    [HttpPost("api/documents/{documentId:guid}/mindmap")]
    public async Task<ActionResult<MindMapResponse>> GenerateMindMap(Guid documentId, GenerateAiRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GenerateMindMapCommand(GetUserId(), documentId, request.ForceRegenerate), cancellationToken));

    [HttpGet("api/documents/{documentId:guid}/mindmap")]
    public async Task<ActionResult<MindMapResponse>> GetMindMap(Guid documentId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetMindMapQuery(GetUserId(), documentId), cancellationToken));

    [HttpPost("api/documents/{documentId:guid}/flashcards")]
    public async Task<ActionResult<FlashcardsResponse>> GenerateFlashcards(Guid documentId, GenerateAiRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GenerateFlashcardsCommand(GetUserId(), documentId, request.ForceRegenerate), cancellationToken));

    [HttpGet("api/documents/{documentId:guid}/flashcards")]
    public async Task<ActionResult<FlashcardsResponse>> GetFlashcards(Guid documentId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetFlashcardsQuery(GetUserId(), documentId), cancellationToken));

    [HttpPost("api/documents/{documentId:guid}/quiz")]
    public async Task<ActionResult<QuizResponse>> GenerateQuiz(Guid documentId, GenerateAiRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GenerateQuizCommand(GetUserId(), documentId, request.ForceRegenerate), cancellationToken));

    [HttpGet("api/documents/{documentId:guid}/quiz")]
    public async Task<ActionResult<QuizResponse>> GetQuiz(Guid documentId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetQuizQuery(GetUserId(), documentId), cancellationToken));

    [HttpPost("api/quizzes/{quizId:guid}/submit")]
    public async Task<ActionResult<QuizResultResponse>> SubmitQuiz(Guid quizId, SubmitQuizRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new SubmitQuizCommand(GetUserId(), quizId, request), cancellationToken));

    [HttpPost("api/documents/{documentId:guid}/chat/sessions")]
    public async Task<ActionResult<ChatSessionResponse>> CreateChatSession(Guid documentId, CreateChatSessionRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new CreateChatSessionCommand(GetUserId(), documentId, request), cancellationToken));

    [HttpGet("api/documents/{documentId:guid}/chat/sessions")]
    public async Task<ActionResult<IReadOnlyCollection<ChatSessionResponse>>> GetChatSessions(Guid documentId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetChatSessionsQuery(GetUserId(), documentId), cancellationToken));

    [HttpPost("api/chat/sessions/{sessionId:guid}/messages")]
    public async Task<ActionResult<ChatMessageResponse>> SendChatMessage(Guid sessionId, SendChatMessageRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new SendChatMessageCommand(GetUserId(), sessionId, request), cancellationToken));

    [HttpGet("api/chat/sessions/{sessionId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyCollection<ChatMessageResponse>>> GetChatMessages(Guid sessionId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetChatMessagesQuery(GetUserId(), sessionId), cancellationToken));

    private Guid GetUserId()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            throw new UnauthorizedAccessException("The authenticated user identifier is missing.");
        }

        return userId;
    }
}
