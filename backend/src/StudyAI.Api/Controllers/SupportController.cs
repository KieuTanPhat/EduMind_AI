using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyAI.Application.Features.Support.Commands;
using StudyAI.Contracts.Support;

namespace StudyAI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/support")]
public sealed class SupportController : ControllerBase
{
    private readonly ISender _sender;
    public SupportController(ISender sender) => _sender = sender;

    [HttpPost("tickets")]
    public async Task<ActionResult<SupportTicketResponse>> Create(CreateSupportTicketRequest request, CancellationToken cancellationToken) => Ok(await _sender.Send(new CreateSupportTicketCommand(GetUserId(), request), cancellationToken));

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
