// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading.Filters;

/// <summary>
/// A filter that includes core infrastructure tools that should always be available
/// regardless of namespace filtering or server mode (except SingleToolProxy mode).
/// Core infrastructure includes subscription and group commands only.
/// </summary>
public class CoreInfrastructureFilter : ICommandFilter
{
    /// <summary>
    /// The names of core infrastructure areas.
    /// </summary>
    private static readonly string[] CoreAreaNames = ["subscription", "group"];

    /// <summary>
    /// Gets the priority of this filter. Core infrastructure has high priority (10)
    /// to ensure these essential tools are always considered for inclusion.
    /// </summary>
    public int Priority => 10;

    /// <summary>
    /// Gets the name of this filter for debugging and diagnostics.
    /// </summary>
    public string Name => "CoreInfrastructure";

    /// <summary>
    /// Determines whether a command is a core infrastructure command that should be included.
    /// </summary>
    /// <param name="commandName">The full name of the command (e.g., "subscription_list").</param>
    /// <param name="command">The command implementation.</param>
    /// <returns>True if this is a core infrastructure command; false otherwise.</returns>
    public bool ShouldIncludeCommand(string commandName, IBaseCommand command)
    {
        if (string.IsNullOrWhiteSpace(commandName) || command == null)
        {
            return false;
        }

        // Commands are named like "subscription_list", "group_list"
        return CoreAreaNames.Any(area =>
            commandName.StartsWith($"{area}_", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the list of core area names for testing and validation purposes.
    /// </summary>
    /// <returns>An array of core area names.</returns>
    internal static string[] GetCoreAreaNames() => CoreAreaNames;
}
