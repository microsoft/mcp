namespace Azure.Mcp.Tools.Monitor.Models;

public sealed record ProcessorTemplate
{
    public required string Found { get; init; }
    public required List<ImplementationTemplate> Implementations { get; init; }
    public required List<string> Registrations { get; init; }
}
