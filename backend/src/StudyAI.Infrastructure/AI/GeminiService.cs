using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Infrastructure.AI;

public sealed class GeminiService : IAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(IHttpClientFactory httpClientFactory, IOptions<GeminiOptions> options, ILogger<GeminiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ExternalServiceException("GEMINI_API_KEY is not configured.");
        }

        var prompt = $"{request.Prompt}\n\nDOCUMENT CONTEXT:\n{request.DocumentContext}";
        var body = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                responseMimeType = request.StructuredJson ? "application/json" : "text/plain",
                temperature = request.StructuredJson ? 0.2 : 0.3,
                maxOutputTokens = request.MaxOutputTokens ?? _options.MaxOutputTokens
            }
        };

        var client = _httpClientFactory.CreateClient("Gemini");
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"models/{Uri.EscapeDataString(_options.Model)}:generateContent");
        httpRequest.Headers.Add("x-goog-api-key", _options.ApiKey);
        httpRequest.Content = JsonContent.Create(body);

        using var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini request failed with status code {StatusCode}", response.StatusCode);
            throw new ExternalServiceException("The AI provider rejected the request.");
        }

        GeminiResponse? payload;
        try
        {
            payload = JsonSerializer.Deserialize<GeminiResponse>(responseBody);
        }
        catch (JsonException exception)
        {
            throw new ExternalServiceException("The AI provider returned an invalid response.", exception);
        }

        var text = payload?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ExternalServiceException("The AI provider returned an empty response.");
        }

        return new AiGenerationResult(
            text.Trim(),
            _options.Model,
            payload?.UsageMetadata?.PromptTokenCount ?? 0,
            payload?.UsageMetadata?.CandidatesTokenCount ?? 0);
    }

    private sealed class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }

        [JsonPropertyName("usageMetadata")]
        public UsageMetadata? UsageMetadata { get; set; }
    }

    private sealed class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }

    private sealed class Content
    {
        [JsonPropertyName("parts")]
        public List<Part>? Parts { get; set; }
    }

    private sealed class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class UsageMetadata
    {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokenCount { get; set; }

        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }
    }
}
