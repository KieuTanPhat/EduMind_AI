using FluentAssertions;
using StudyAI.Infrastructure.Processing;

namespace StudyAI.Infrastructure.Tests;

public sealed class TextProcessingServiceTests
{
    private readonly TextProcessingService _service = new();

    [Fact]
    public void Clean_ShouldNormalizeWhitespaceAndNewlines()
    {
        var result = _service.Clean("  First\r\n\r\n\r\n  second\t\t line  ");

        result.Should().Be("First\n\nsecond line");
    }

    [Fact]
    public void Chunk_ShouldRespectLimitAndKeepOverlap()
    {
        var text = string.Join(' ', Enumerable.Range(1, 40).Select(index => $"word{index}"));

        var chunks = _service.Chunk(text, maxCharacters: 60, overlapCharacters: 10);

        chunks.Should().HaveCountGreaterThan(1);
        chunks.Should().OnlyContain(chunk => chunk.Length <= 60);
        chunks[0].Should().NotBeEmpty();
        chunks[1].Should().Contain(chunks[0][^10..].Trim());
    }

    [Fact]
    public void Chunk_ShouldRejectInvalidOverlap()
    {
        var act = () => _service.Chunk("content", maxCharacters: 10, overlapCharacters: 10);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
