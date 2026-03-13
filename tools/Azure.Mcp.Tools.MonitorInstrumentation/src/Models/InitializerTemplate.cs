namespace Azure.Mcp.Tools.MonitorInstrumentation.Models;

public sealed record InitializerTemplate
{
    public required string Found { get; init; }
    public required List<ImplementationTemplate> Implementations { get; init; }
    public required List<string> Registrations { get; init; }
}
