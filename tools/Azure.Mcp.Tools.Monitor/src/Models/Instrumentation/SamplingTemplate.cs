namespace Azure.Mcp.Tools.Monitor.Models;

public sealed record SamplingTemplate
{
    public required string HasCustomSampling { get; init; }
    public required string Type { get; init; }
    public required string Details { get; init; }
    public required string File { get; init; }
}
