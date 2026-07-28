// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    /// Path to prompts file. If not set, will default to "${repoRoot}/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md"
    /// </summary>
    [ConfigurationKeyName("promptFile")]
    public string PromptFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Path to working directory where eval files and workspace will be created. If not set, will default to "${repoRoot}/.work/vally"
    /// </summary>
    [ConfigurationKeyName("workingDirectory")]
    public string WorkingDirectory { get; set; } = string.Empty;

    /// <summary>
    /// (Optional) Path to build info file.  If set, will create evals based on the "pathsToTest" in the build info file.  If not set, will create evals for all prompts in the prompts file.
    /// </summary>
    [ConfigurationKeyName("buildInfo")]
    public string BuildInfo { get; set; } = string.Empty;

    /// <summary>
    /// Gets the base directory for Vally artifacts, which is a subdirectory of the working directory.
    /// </summary>
    public string VallyBaseDirectory
    {
        get
        {
            return Path.Combine(WorkingDirectory, "vally");
        }
    }
}
