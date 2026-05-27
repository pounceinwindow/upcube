namespace UpperCube.Application.Abstractions.AI;

public interface ILocalLlmClient
{
    Task<LocalLlmResult> GenerateAsync(
        LocalLlmRequest request,
        CancellationToken ct = default);
}

public sealed record LocalLlmRequest(
    string SystemPrompt,
    string UserPrompt,
    string Culture);

public sealed record LocalLlmResult(
    string Text,
    bool IsSuccess,
    string? ErrorMessage = null);