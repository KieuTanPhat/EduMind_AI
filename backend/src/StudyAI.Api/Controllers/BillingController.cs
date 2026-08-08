using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyAI.Application.Features.Billing.Commands;
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

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
