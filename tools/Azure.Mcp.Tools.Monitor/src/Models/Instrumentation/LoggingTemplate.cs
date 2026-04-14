namespace Azure.Mcp.Tools.Monitor.Models;

public sealed record LoggingTemplate
{
    public required string Found { get; init; }
    public required string HasExplicitLoggerProvider { get; init; }
    public required List<string> LogFilters { get; init; }
    public required string File { get; init; }
}
