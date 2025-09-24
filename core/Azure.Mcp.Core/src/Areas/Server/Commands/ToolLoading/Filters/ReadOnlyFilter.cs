// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading.Filters;

/// <summary>
/// A filter that enforces ReadOnly mode by excluding commands that are not marked as read-only
/// when the server is configured in ReadOnly mode. This preserves the existing ReadOnly behavior
/// from the original CommandFactoryToolLoader.
/// </summary>
/// <param name="readOnlyMode">True if the server is in ReadOnly mode; false otherwise.</param>
public class ReadOnlyFilter(bool readOnlyMode) : ICommandFilter
{
    private readonly bool _readOnlyMode = readOnlyMode;

    /// <summary>
    /// Gets the priority of this filter. ReadOnly filtering has lower priority (40)
    /// to allow core infrastructure and extension filtering to happen first.
    /// </summary>
    public int Priority => 40;

    /// <summary>
    /// Gets the name of this filter for debugging and diagnostics.
    /// </summary>
    public string Name => "ReadOnly";

    /// <summary>
    /// Determines whether a command should be included based on ReadOnly mode settings.
    /// In ReadOnly mode, only commands marked as ReadOnly are included.
    /// </summary>
    /// <param name="commandName">The full name of the command.</param>
    /// <param name="command">The command implementation to check for ReadOnly metadata.</param>
    /// <returns>True if the command should be included; false if ReadOnly mode excludes it.</returns>
    public bool ShouldIncludeCommand(string commandName, IBaseCommand command)
    {
        if (command == null)
        {
            return false;
        }

        // If not in ReadOnly mode, include all commands (this filter doesn't restrict)
        if (!_readOnlyMode)
        {
            return true;
        }

        // In ReadOnly mode, only include commands marked as ReadOnly
        // This preserves the existing behavior: .Where(tool => !_options.Value.ReadOnly || (tool.Annotations?.ReadOnlyHint == true))
        return command.Metadata.ReadOnly;
    }

    /// <summary>
    /// Gets whether this filter is configured for ReadOnly mode.
    /// </summary>
    public bool IsReadOnlyMode => _readOnlyMode;
}
