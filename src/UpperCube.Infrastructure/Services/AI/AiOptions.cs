namespace UpperCube.Infrastructure.Services.AI;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public bool Enabled { get; set; } = true;

    public string Provider { get; set; } = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";

    public string Model { get; set; } = "llama3.1";

    public int TimeoutSeconds { get; set; } = 60;

    public int MaxTokens { get; set; } = 48;

    public int ContextLength { get; set; } = 1024;

    public double Temperature { get; set; } = 0.2;
}