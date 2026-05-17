namespace Azure.Mcp.Tools.Monitor.Models;

public record Decision
{
    public string Intent { get; init; } = null!;  // "onboard" | "migrate"
    public string TargetApproach { get; init; } = null!;
    public string Rationale { get; init; } = null!;
}
