namespace Azure.Mcp.Tools.Monitor.Models;

public sealed record TelemetryPipelineTemplate
{
    public required string Found { get; init; }
    public required string HasCustomChannel { get; init; }
    public required string HasTelemetrySinks { get; init; }
    public required string ClassName { get; init; }
    public required string File { get; init; }
    public required string Details { get; init; }
}
