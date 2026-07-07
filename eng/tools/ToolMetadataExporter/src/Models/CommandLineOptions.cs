// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ToolMetadataExporter.Models;

/// <summary>
/// Options specified via command line arguments. Supported options are:
/// <list type="bullet">
///     <item><c>--dry-run</c>: If specified, the tool will run in dry-run mode, meaning no changes will be made to the target datastore.</item>
///     <item><c>--azmcp-exe &lt;path&gt;</c>: The path to the azmcp executable to use for interacting with the MCP server.</item>
/// </list>
/// </summary>
internal class CommandLineOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the tool analysis should be performed as a dry run.
    /// </summary>
    /// <remarks>When set to <see langword="true"/>, the operation is performed, output to the console, but not persisted to the datastore. When set to
    /// <see langword="false"/>, the operation is executed normally.</remarks>
    public bool? IsDryRun { get; set; }

    /// <summary>
    /// Gets or sets the full path to the AzMcp executable file.
    /// </summary>
    public string? AzmcpExe { get; set; }

    /// <summary>
    /// True to use the analysis datetime for <see cref="Kusto.McpToolEvent.EventTime"/>, else the date of <see cref="AzmcpProgram.AzMcpBuildDateTime"/> is used.  Default value is true.
    /// </summary>
    /// <remarks>Setting value to false is useful for post-release analysis where the <see cref="AzmcpExe"/> was downloaded as a NuGet package.</remarks>
    public bool? UseAnalysisTime { get; set; } = true;
}
