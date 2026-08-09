using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyAI.Application.Features.Billing.Commands;
using StudyAI.Application.Features.Billing.Queries;
using StudyAI.Application.Features.Admin.Queries;
using StudyAI.Contracts.Admin;
using StudyAI.Contracts.Billing;

namespace StudyAI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/billing")]
public sealed class BillingController : ControllerBase
{
    private readonly ISender _sender;
    public BillingController(ISender sender) => _sender = sender;

    [HttpPost("plus-requests")]
    public async Task<ActionResult<PlusRequestResponse>> Create(PlusRequestRequest request, CancellationToken cancellationToken) => Ok(await _sender.Send(new CreatePlusRequestCommand(GetUserId(), request), cancellationToken));

    [HttpGet("plus-requests/{requestId:guid}")]
    public async Task<ActionResult<PlusRequestResponse>> Get(Guid requestId, CancellationToken cancellationToken) => Ok(await _sender.Send(new GetPaymentOrderQuery(GetUserId(), requestId), cancellationToken));

    [HttpDelete("plus-requests/{requestId:guid}")]
    public async Task<IActionResult> Cancel(Guid requestId, CancellationToken cancellationToken)
    {
        await _sender.Send(new CancelPlusRequestCommand(GetUserId(), requestId), cancellationToken);
        return NoContent();
    }

    [HttpGet("plan-policies")]
    public async Task<ActionResult<IReadOnlyCollection<PlanPolicyResponse>>> GetPlanPolicies(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPlanPoliciesQuery(), cancellationToken));

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
