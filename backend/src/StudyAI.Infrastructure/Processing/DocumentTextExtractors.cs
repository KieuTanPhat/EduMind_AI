using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using StudyAI.Application.Abstractions;
using StudyAI.Domain.Enums;
using UglyToad.PdfPig;

namespace StudyAI.Infrastructure.Processing;

public sealed class PdfTextExtractor : IDocumentTextExtractor
{
    public bool CanHandle(DocumentFileType fileType) => fileType == DocumentFileType.Pdf;

    public Task<string> ExtractAsync(Stream content, CancellationToken cancellationToken)
    {
        using var document = PdfDocument.Open(content);
        var pages = document.GetPages().Select(page => page.Text);
        return Task.FromResult(string.Join(Environment.NewLine, pages));
    }
}

public sealed class DocxTextExtractor : IDocumentTextExtractor
{
    public bool CanHandle(DocumentFileType fileType) => fileType == DocumentFileType.Docx;

    public Task<string> ExtractAsync(Stream content, CancellationToken cancellationToken)
    {
        using var document = WordprocessingDocument.Open(content, false);
        var text = document.MainDocumentPart?.Document.Body?
            .Descendants<Text>()
            .Select(node => node.Text)
            .ToArray() ?? Array.Empty<string>();
        return Task.FromResult(string.Join(" ", text));
    }
}

public sealed class TxtTextExtractor : IDocumentTextExtractor
{
    public bool CanHandle(DocumentFileType fileType) => fileType == DocumentFileType.Txt;

    public async Task<string> ExtractAsync(Stream content, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(content, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
