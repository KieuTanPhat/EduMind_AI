using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyAI.Application.Features.Documents.Commands;
using StudyAI.Application.Features.Documents.Queries;
using StudyAI.Contracts.Documents;

namespace StudyAI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/documents")]
public sealed class DocumentsController : ControllerBase
{
    private readonly ISender _sender;

    public DocumentsController(ISender sender) => _sender = sender;

    [HttpPost]
    [RequestSizeLimit(25 * 1024 * 1024)]
    [ProducesResponseType(typeof(UploadDocumentResponse), StatusCodes.Status202Accepted)]
    public async Task<ActionResult<UploadDocumentResponse>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        if (file is null)
        {
            return BadRequest(new ProblemDetails { Title = "A document file is required." });
        }

        await using var stream = file.OpenReadStream();
        var response = await _sender.Send(
            new UploadDocumentCommand(userId, file.FileName, file.ContentType, file.Length, stream),
            cancellationToken);

        return AcceptedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DocumentListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<DocumentListItemResponse>>> GetList(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _sender.Send(new GetDocumentsQuery(GetRequiredUserId(), search, page, pageSize), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentDetailResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DocumentDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetDocumentQuery(GetRequiredUserId(), id), cancellationToken));
    }

    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(DocumentStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DocumentStatusResponse>> GetStatus(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetDocumentStatusQuery(GetRequiredUserId(), id), cancellationToken));
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var document = await _sender.Send(new DownloadDocumentQuery(GetRequiredUserId(), id), cancellationToken);
        return File(document.Content, document.ContentType, document.FileName, enableRangeProcessing: true);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteDocumentCommand(GetRequiredUserId(), id), cancellationToken);
        return NoContent();
    }

    private Guid GetRequiredUserId()
    {
        if (!TryGetUserId(out var userId))
        {
            throw new UnauthorizedAccessException("The authenticated user identifier is missing.");
        }

        return userId;
    }

    private bool TryGetUserId(out Guid userId) => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
