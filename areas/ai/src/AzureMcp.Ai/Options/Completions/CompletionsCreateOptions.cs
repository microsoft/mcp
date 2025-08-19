using AzureMcp.Ai.Options;

namespace AzureMcp.Ai.Options.Completions;

public sealed class CompletionsCreateOptions : BaseAiOptions
{
    public string? ResourceName { get; set; }
    public string? DeploymentName { get; set; }
    public string? PromptText { get; set; }

    public int? MaxTokens { get; set; }
    public double? Temperature { get; set; }
    public string? ApiVersion { get; set; }
}