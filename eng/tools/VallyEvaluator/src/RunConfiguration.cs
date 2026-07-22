using Microsoft.Extensions.Configuration;

namespace VallyEvaluator;

public class RunConfiguration
{
    /// <summary>
    /// Tool namespaces to create evaluations for. Comma-separated list. For example: "storage,acr"
    /// </summary>
    [ConfigurationKeyName("namespaces")]
    public string NamespacesValue { get; set; } = string.Empty;

    public List<string> Namespaces { get; set; } = new List<string>();

    /// <summary>
    /// Path to prompts file. If not set, will default to /servers/Azure.Mcp.Server/docs/e2eTestPrompts.md
    /// </summary>
    [ConfigurationKeyName("promptFile")]
    public string PromptFilePath { get; set; } = string.Empty;

    [ConfigurationKeyName("workingDirectory")]
    public string WorkingDirectory { get; set; } = string.Empty;

    [ConfigurationKeyName("buildInfo")]
    public string BuildInfo { get; set; } = string.Empty;
}
