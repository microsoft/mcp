// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading.Filters;

/// <summary>
/// A filter that enforces command visibility rules by using the existing
/// CommandFactory.GetVisibleCommands logic. This preserves the existing behavior
/// for hidden commands and ensures consistency with the original implementation.
/// </summary>
public class VisibilityFilter : ICommandFilter
{
    /// <summary>
    /// Gets the priority of this filter. Visibility filtering has the lowest priority (50)
    /// to ensure it's applied last after all other filtering logic.
    /// </summary>
    public int Priority => 50;

    /// <summary>
    /// Gets the name of this filter for debugging and diagnostics.
    /// </summary>
    public string Name => "Visibility";

    /// <summary>
    /// Determines whether a command should be included based on visibility rules.
    /// Uses the existing CommandFactory.GetVisibleCommands logic to maintain consistency.
    /// </summary>
    /// <param name="commandName">The full name of the command.</param>
    /// <param name="command">The command implementation to check for visibility.</param>
    /// <returns>True if the command is visible and should be included; false otherwise.</returns>
    public bool ShouldIncludeCommand(string commandName, IBaseCommand command)
    {
        if (string.IsNullOrWhiteSpace(commandName) || command == null)
        {
            return false;
        }

        // Use the existing CommandFactory visibility logic to determine if this command should be visible
        // This creates a temporary dictionary with just this command and checks if it's visible
        var tempCommandDict = new Dictionary<string, IBaseCommand>
        {
            [commandName] = command
        };

        var visibleCommands = CommandFactory.GetVisibleCommands(tempCommandDict);

        // If the command is in the visible commands enumerable, it should be included
        return visibleCommands.Any(kvp => kvp.Key == commandName);
    }
}
