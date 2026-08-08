using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyAI.Application.Features.Admin.Commands;
using StudyAI.Application.Features.Admin.Queries;
using StudyAI.Contracts.Billing;
using StudyAI.Contracts.Support;
using StudyAI.Contracts.Admin;
using StudyAI.Contracts.Documents;

namespace StudyAI.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly ISender _sender;

    public AdminController(ISender sender) => _sender = sender;

    [HttpGet("users")]
    public async Task<ActionResult<PagedResponse<AdminUserResponse>>> GetUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _sender.Send(new GetAdminUsersQuery(search, page, pageSize), cancellationToken));

    [HttpPost("users/{userId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == GetUserId())
        {
            return BadRequest(new { detail = "An administrator cannot deactivate their own account." });
        }

        await _sender.Send(new DeactivateUserCommand(userId), cancellationToken);
        return NoContent();
    }

    [HttpGet("documents")]
    public async Task<ActionResult<PagedResponse<AdminDocumentResponse>>> GetDocuments([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _sender.Send(new GetAdminDocumentsQuery(search, page, pageSize), cancellationToken));

    [HttpGet("ai-usage")]
    public async Task<ActionResult<IReadOnlyCollection<AiUsageSummaryResponse>>> GetAiUsage(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAiUsageQuery(), cancellationToken));

    [HttpGet("statistics")]
    public async Task<ActionResult<AdminStatsResponse>> GetStatistics(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAdminStatsQuery(), cancellationToken));

    [HttpGet("plus-requests")]
    public async Task<ActionResult<IReadOnlyCollection<PlusRequestAdminResponse>>> GetPlusRequests(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPlusRequestsQuery(), cancellationToken));

    [HttpPost("plus-requests/{requestId:guid}/process")]
    public async Task<IActionResult> ProcessPlusRequest(Guid requestId, GrantPlusRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new ProcessPlusRequestCommand(GetUserId(), requestId, request.Approve, request.Note, request.DurationDays), cancellationToken);
        return NoContent();
    }

    [HttpGet("support-tickets")]
    public async Task<ActionResult<IReadOnlyCollection<SupportTicketResponse>>> GetSupportTickets(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetSupportTicketsQuery(), cancellationToken));

    [HttpPost("support-tickets/{ticketId:guid}/resolve")]
    public async Task<IActionResult> ResolveSupportTicket(Guid ticketId, [FromBody] string reply, CancellationToken cancellationToken)
    {
        await _sender.Send(new ResolveSupportTicketCommand(ticketId, reply), cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            throw new UnauthorizedAccessException("The authenticated user identifier is missing.");
        }

        return userId;
    }
}
