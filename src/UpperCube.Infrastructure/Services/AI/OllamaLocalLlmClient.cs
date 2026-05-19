using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UpperCube.Application.Abstractions.AI;

namespace UpperCube.Infrastructure.Services.AI;

public sealed class OllamaLocalLlmClient(
    HttpClient httpClient,
    IOptions<AiOptions> options,
    ILogger<OllamaLocalLlmClient> logger) : ILocalLlmClient
{
    public async Task<LocalLlmResult> GenerateAsync(LocalLlmRequest request, CancellationToken ct = default)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            return new LocalLlmResult(string.Empty, false, "Local AI is unavailable.");
        }

        if (!string.Equals(settings.Provider, "Ollama", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Unsupported local AI provider: {Provider}", settings.Provider);
            return new LocalLlmResult(string.Empty, false, "Local AI is unavailable.");
        }

        if (string.IsNullOrWhiteSpace(settings.BaseUrl) || string.IsNullOrWhiteSpace(settings.Model))
        {
            logger.LogWarning("Local AI settings are incomplete.");
            return new LocalLlmResult(string.Empty, false, "Local AI is unavailable.");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(settings.TimeoutSeconds, 1)));

        try
        {
            var body = new OllamaChatRequest(
                settings.Model,
                false,
                [
                    new OllamaChatMessage("system", request.SystemPrompt),
                    new OllamaChatMessage("user", request.UserPrompt)
                ]);

            using var response = await httpClient.PostAsJsonAsync("api/chat", body, timeoutCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                var message = $"Local AI returned HTTP {(int)response.StatusCode}.";
                logger.LogWarning(message);
                return new LocalLlmResult(string.Empty, false, "Local AI is unavailable.");
            }

            var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
                cancellationToken: timeoutCts.Token);
            var content = ollamaResponse?.Message?.Content;

            if (string.IsNullOrWhiteSpace(content))
            {
                const string message = "Local AI returned an empty response.";
                logger.LogWarning(message);
                return new LocalLlmResult(string.Empty, false, "Local AI is unavailable.");
            }

            return new LocalLlmResult(content.Trim(), true);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            var message = $"Local AI request timed out after {settings.TimeoutSeconds} seconds.";
            logger.LogWarning(message);
            return new LocalLlmResult(string.Empty, false, "Local AI is unavailable.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to call local AI.");
            return new LocalLlmResult(string.Empty, false, "Local AI is unavailable.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unexpected local AI error.");
            return new LocalLlmResult(string.Empty, false, "Local AI is unavailable.");
        }
    }

    private sealed record OllamaChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("messages")] IReadOnlyList<OllamaChatMessage> Messages);

    private sealed record OllamaChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaChatMessage? Message);
}
