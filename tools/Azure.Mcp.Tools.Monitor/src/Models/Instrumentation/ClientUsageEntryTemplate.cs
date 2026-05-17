namespace Azure.Mcp.Tools.Monitor.Models;

public sealed record ClientUsageEntryTemplate
{
    public required string File { get; init; }
    public required string Pattern { get; init; }
    public required List<string> Methods { get; init; }
}
