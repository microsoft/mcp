// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading.Filters;

/// <summary>
/// A filter that controls inclusion of extension tools based on configuration.
/// Extension tools are additional functionality like Azure Quick Review (azqr)
/// that are only included when explicitly requested.
/// </summary>
/// <param name="includeExtensions">True to include extension tools; false to exclude them.</param>
public class ExtensionFilter(bool includeExtensions) : ICommandFilter
{
    private readonly bool _includeExtensions = includeExtensions;

    /// <summary>
    /// Gets the priority of this filter. Extension filtering has medium priority (30)
    /// to allow core infrastructure to be included first, but before mode-specific filters.
    /// </summary>
    public int Priority => 30;

    /// <summary>
    /// Gets the name of this filter for debugging and diagnostics.
    /// </summary>
    public string Name => "Extension";

    /// <summary>
    /// Determines whether a command should be included based on extension inclusion settings.
    /// </summary>
    /// <param name="commandName">The full name of the command (e.g., "extension_azqr").</param>
    /// <param name="command">The command implementation.</param>
    /// <returns>True if the command should be included; false if it's an extension and extensions are disabled.</returns>
    public bool ShouldIncludeCommand(string commandName, IBaseCommand command)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            return false;
        }

        // Check if this is an extension command
        var isExtension = commandName.StartsWith("extension_", StringComparison.OrdinalIgnoreCase);

        // If it's not an extension command, always include it (this filter only excludes extensions)
        if (!isExtension)
        {
            return true;
        }

        // If it is an extension command, only include it if extensions are enabled
        return _includeExtensions;
    }

    /// <summary>
    /// Gets whether this filter is configured to include extension tools.
    /// </summary>
    public bool IncludeExtensions => _includeExtensions;
}
