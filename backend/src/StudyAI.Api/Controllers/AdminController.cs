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

    [HttpPost("users/{userId:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid userId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateUserCommand(userId), cancellationToken);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/plus")]
    public async Task<IActionResult> GrantPlus(Guid userId, GrantPlusUserRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new GrantPlusToUserCommand(userId, request.DurationDays), cancellationToken);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/pro")]
    public async Task<IActionResult> GrantPro(Guid userId, GrantPlusUserRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new GrantProToUserCommand(userId, request.DurationDays), cancellationToken);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/quota")]
    public async Task<IActionResult> SetAiQuota(Guid userId, SetAiQuotaRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new SetAiQuotaCommand(userId, request.TokenLimitPerDay), cancellationToken);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/plan")]
    public async Task<IActionResult> SetUserPlan(Guid userId, SetUserPlanRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new SetUserPlanCommand(userId, request), cancellationToken);
        return NoContent();
    }

    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> PermanentlyDeleteUser(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == GetUserId())
        {
            return BadRequest(new { detail = "An administrator cannot permanently delete their own account." });
        }

        await _sender.Send(new PermanentlyDeleteUserCommand(userId), cancellationToken);
        return NoContent();
    }

    [HttpGet("documents/{documentId:guid}/download")]
    public async Task<IActionResult> DownloadDocument(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await _sender.Send(new DownloadAdminDocumentQuery(documentId), cancellationToken);
        return File(document.Content, document.ContentType, document.FileName, enableRangeProcessing: true);
    }

    [HttpGet("documents")]
    public async Task<ActionResult<PagedResponse<AdminDocumentResponse>>> GetDocuments([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _sender.Send(new GetAdminDocumentsQuery(search, page, pageSize), cancellationToken));

    [HttpDelete("documents/{documentId:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid documentId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAdminDocumentCommand(documentId), cancellationToken);
        return NoContent();
    }

    [HttpGet("ai-usage")]
    public async Task<ActionResult<IReadOnlyCollection<AiUsageSummaryResponse>>> GetAiUsage(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAiUsageQuery(), cancellationToken));

    [HttpGet("statistics")]
    public async Task<ActionResult<AdminStatsResponse>> GetStatistics(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAdminStatsQuery(), cancellationToken));

    [HttpGet("plan-policies")]
    public async Task<ActionResult<IReadOnlyCollection<PlanPolicyResponse>>> GetPlanPolicies(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPlanPoliciesQuery(), cancellationToken));

    [HttpPut("plan-policies/{plan}")]
    public async Task<ActionResult<PlanPolicyResponse>> UpdatePlanPolicy(string plan, UpdatePlanPolicyRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new UpdatePlanPolicyCommand(plan, request), cancellationToken));

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

    [HttpPost("support-users/{userId:guid}/read")]
    public async Task<IActionResult> MarkSupportRead(Guid userId, CancellationToken cancellationToken)
    {
        await _sender.Send(new MarkSupportTicketsReadCommand(userId), cancellationToken);
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
