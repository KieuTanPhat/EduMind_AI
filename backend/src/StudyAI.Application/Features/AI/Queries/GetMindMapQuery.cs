using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;

namespace StudyAI.Application.Features.AI.Queries;

public sealed record GetMindMapQuery(Guid UserId, Guid DocumentId) : IRequest<MindMapResponse>;

public sealed class GetMindMapQueryHandler : IRequestHandler<GetMindMapQuery, MindMapResponse>
{
    private readonly IApplicationDbContext _db;

    public GetMindMapQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<MindMapResponse> Handle(GetMindMapQuery query, CancellationToken cancellationToken)
    {
        var map = await _db.MindMaps.AsNoTracking().Include(x => x.Nodes)
            .Where(x => x.DocumentId == query.DocumentId && x.Document.UserId == query.UserId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Mind map was not found.");
        return new MindMapResponse(map.Id, map.DocumentId, map.Title, map.Model, map.Nodes.Select(node => new MindMapNodeResponse(node.Id, node.ParentNodeId, node.Label, node.Description, node.Depth, node.PositionX, node.PositionY)).ToArray(), map.CreatedAtUtc, map.UpdatedAtUtc);
    }
}
