namespace Azure.Mcp.Tools.MonitorInstrumentation.Models;

public sealed record ImplementationTemplate
{
    public required string ClassName { get; init; }
    public required string File { get; init; }
    public required string Purpose { get; init; }
}
