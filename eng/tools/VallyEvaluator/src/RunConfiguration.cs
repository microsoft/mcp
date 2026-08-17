// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;

namespace VallyEvaluator;

public class RunConfiguration
{
    /// <summary>
    /// (Optional) Tool namespaces to create evaluations for. Comma-separated list. For example: "storage,acr"
    /// </summary>
    [ConfigurationKeyName("namespaces")]
    public string NamespacesValue { get; set; } = string.Empty;

    public List<string> Namespaces { get; set; } = new List<string>();

    /// <summary>
    /// (Optional) Path to working directory where eval files and workspace will be created. If not set, will default to "${RepositoryRoot}/.work"
    /// </summary>
    [ConfigurationKeyName("workingDirectory")]
    public string WorkingDirectory { get; set; } = string.Empty;

    /// <summary>
    /// (Optional) Path to build info file.  If set, will create evals based on the "pathsToTest" in the build info file.  If not set, will create evals for all prompts in the prompts file.
    /// </summary>
    [ConfigurationKeyName("buildInfo")]
    public string BuildInfo { get; set; } = string.Empty;

    /// <summary>
    /// (REQUIRED) Name of the MCP server to use for evals.
    /// </summary>
    [ConfigurationKeyName("serverName")]
    public string ServerName { get; set; } = string.Empty;

    /// <summary>
    /// The root of the repository.
    /// </summary>
    public string RepositoryRoot { get; set; } = string.Empty;

    /// <summary>
    /// Gets the base directory for Vally artifacts, which is a subdirectory of the working directory
    /// in the format "$(WorkingDirectory)/vally".
    /// </summary>
    public string VallyBaseDirectory
    {
        get
        {
            return Path.Combine(WorkingDirectory, "vally");
        }
    }

    /// <summary>
    /// Gets the build directory for server artifacts, which is a subdirectory of the working directory
    /// in the format "$(WorkingDirectory)/build".
    /// </summary>
    public string BuildDirectory
    {
        get
        {
            return Path.Combine(WorkingDirectory, "build");
        }
    }

    /// <summary>
    /// Gets the file path for the prompts.  It is constructed from <see cref="ServerName" /> in the
    /// format "servers/$(ServerName)/docs/e2eTestPrompts.md"
    /// </summary>
    public string PromptFilePath
    {
        get
        {
            return Path.Combine(RepositoryRoot, "servers", ServerName, "docs", "e2eTestPrompts.md");
        }
    }
}
