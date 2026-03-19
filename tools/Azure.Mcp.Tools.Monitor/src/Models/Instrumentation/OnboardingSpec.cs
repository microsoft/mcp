using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models;

public record OnboardingSpec
{
    public string Version { get; init; } = "0.1";
    public string? AgentMustExecuteFirst { get; init; }
    public Analysis Analysis { get; init; } = null!;
    public Decision Decision { get; init; } = null!;
    public List<OnboardingAction> Actions { get; init; } = [];
    public List<string> Warnings { get; init; } = [];
}
