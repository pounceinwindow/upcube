namespace UpperCube.Domain.Entities;

public sealed class ErrorLogEntry
{
    public string? Id { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? StackTrace { get; set; }

    public string? Url { get; set; }

    public string? HttpMethod { get; set; }

    public string? UserId { get; set; }

    public string Severity { get; set; } = "Error";

    public DateTime Timestamp { get; set; }
}
