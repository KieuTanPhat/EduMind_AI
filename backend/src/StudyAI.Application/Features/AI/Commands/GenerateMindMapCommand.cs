using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.AI.Commands;

public sealed record GenerateMindMapCommand(Guid UserId, Guid DocumentId, bool ForceRegenerate) : IRequest<MindMapResponse>;

public sealed class GenerateMindMapCommandHandler : IRequestHandler<GenerateMindMapCommand, MindMapResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IAiService _aiService;
    private readonly ITextProcessingService _textProcessing;

    public GenerateMindMapCommandHandler(IApplicationDbContext db, IAiService aiService, ITextProcessingService textProcessing)
    {
        _db = db;
        _aiService = aiService;
        _textProcessing = textProcessing;
    }

    public async Task<MindMapResponse> Handle(GenerateMindMapCommand command, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.Include(x => x.MindMap).ThenInclude(x => x!.Nodes)
            .SingleOrDefaultAsync(x => x.Id == command.DocumentId && x.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");
        EnsureProcessed(document);

        if (document.MindMap is not null && !command.ForceRegenerate)
        {
            return Map(document.MindMap);
        }

        var result = await _aiService.GenerateAsync(
            new AiGenerationRequest("mindmap", BuildContext(document.ExtractedText!), AiPromptTemplates.MindMap, true),
            cancellationToken);
        using var json = AiJsonHelpers.Parse(result.Text);
        var title = AiJsonHelpers.RequiredString(json.RootElement, "title", 500);
        if (!json.RootElement.TryGetProperty("children", out var children) || children.ValueKind != JsonValueKind.Array)
        {
            throw new BadRequestException("AI mind map output must contain a children array.");
        }

        MindMap map;
        if (document.MindMap is not null)
        {
            map = document.MindMap;
            _db.MindMapNodes.RemoveRange(map.Nodes);
            map.Nodes.Clear();
            map.UpdateTitle(title, result.Model);
        }
        else
        {
            map = new MindMap(document.Id, title, result.Model);
            document.SetMindMap(map);
        }

        AddNodes(map, children, null, 0, 0);
        _db.AiUsageLogs.Add(new AiUsageLog(command.UserId, "mindmap", result.Model, result.InputTokens, result.OutputTokens));
        await _db.SaveChangesAsync(cancellationToken);
        return Map(map);
    }

    private string BuildContext(string text) => string.Join("\n\n--- CHUNK ---\n\n", _textProcessing.Chunk(text).Take(6));

    private void AddNodes(MindMap map, JsonElement children, Guid? parentId, int depth, int siblingIndex)
    {
        if (depth > 7 || map.Nodes.Count >= 200)
        {
            return;
        }

        var index = 0;
        foreach (var child in children.EnumerateArray())
        {
            var label = AiJsonHelpers.RequiredString(child, "label", 500);
            var description = AiJsonHelpers.OptionalString(child, "description", 2000);
            var node = new MindMapNode(map.Id, label, depth, parentId);
            node.SetDescription(description);
            map.Nodes.Add(node);
            node.SetPosition(index * 220, depth * 140);
            index++;

            if (child.TryGetProperty("children", out var nested) && nested.ValueKind == JsonValueKind.Array)
            {
                AddNodes(map, nested, node.Id, depth + 1, index);
            }
        }
    }

    private static void EnsureProcessed(Domain.Entities.Document document)
    {
        if (document.ExtractedText is null || document.Status != Domain.Enums.DocumentStatus.Processed)
        {
            throw new BadRequestException("The document is not processed yet.");
        }
    }

    private static MindMapResponse Map(MindMap map) => new(
        map.Id,
        map.DocumentId,
        map.Title,
        map.Model,
        map.Nodes.Select(node => new MindMapNodeResponse(node.Id, node.ParentNodeId, node.Label, node.Description, node.Depth, node.PositionX, node.PositionY)).ToArray(),
        map.CreatedAtUtc,
        map.UpdatedAtUtc);
}
