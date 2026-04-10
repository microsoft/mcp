namespace Azure.Mcp.Tools.Monitor.Models;

public sealed record ClientUsageTemplate
{
    public required string DirectUsage { get; init; }
    public required List<ClientUsageEntryTemplate> Usages { get; init; }
}
