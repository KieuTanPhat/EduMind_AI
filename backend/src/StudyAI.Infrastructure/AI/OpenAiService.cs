using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Infrastructure.AI;

public sealed class OpenAiService : IAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiService> _logger;

    public OpenAiService(IHttpClientFactory httpClientFactory, IOptions<OpenAiOptions> options, ILogger<OpenAiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ExternalServiceException("OPENAI_API_KEY is not configured.");
        }

        var prompt = $"{request.Prompt}\n\nDOCUMENT CONTEXT:\n{request.DocumentContext}";
        var body = new Dictionary<string, object?>
        {
            ["model"] = _options.Model,
            ["messages"] = new[] { new { role = "user", content = prompt } },
            ["max_completion_tokens"] = _options.MaxOutputTokens
        };
        if (request.StructuredJson)
        {
            body["response_format"] = new { type = "json_object" };
        }

        var client = _httpClientFactory.CreateClient("OpenAI");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(body)
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI request failed with status code {StatusCode}", response.StatusCode);
            throw new ExternalServiceException(response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "OpenAI API key was rejected.",
                HttpStatusCode.TooManyRequests => "OpenAI quota or rate limit was exceeded.",
                _ => "The AI provider rejected the request."
            });
        }

        OpenAiResponse? payload;
        try
        {
            payload = JsonSerializer.Deserialize<OpenAiResponse>(responseBody);
        }
        catch (JsonException exception)
        {
            throw new ExternalServiceException("The AI provider returned an invalid response.", exception);
        }

        var text = payload?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ExternalServiceException("The AI provider returned an empty response.");
        }

        return new AiGenerationResult(
            text.Trim(),
            _options.Model,
            payload?.Usage?.PromptTokens ?? 0,
            payload?.Usage?.CompletionTokens ?? 0);
    }

    private sealed class OpenAiResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private sealed class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private sealed class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }
    }
}
