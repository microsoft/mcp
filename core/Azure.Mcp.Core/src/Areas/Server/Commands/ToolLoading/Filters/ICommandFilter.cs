// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;

namespace Azure.Mcp.Core.Areas.Server.Commands.ToolLoading.Filters;

/// <summary>
/// Defines a filter for determining which commands should be included in tool loading.
/// Filters are applied in priority order to provide composable tool inclusion logic.
/// </summary>
public interface ICommandFilter
{
    /// <summary>
    /// Determines whether a command should be included based on this filter's criteria.
    /// </summary>
    /// <param name="commandName">The full name of the command (e.g., "subscription_list").</param>
    /// <param name="command">The command implementation.</param>
    /// <returns>True if the command should be included; false otherwise.</returns>
    bool ShouldIncludeCommand(string commandName, IBaseCommand command);

    /// <summary>
    /// Gets the priority of this filter. Filters are applied in ascending priority order.
    /// Lower numbers are applied first, higher numbers are applied last.
    /// </summary>
    /// <remarks>
    /// Recommended priority ranges:
    /// - 10-19: Core infrastructure filters (always include essential tools)
    /// - 20-29: Service-specific filters (include/exclude by service area)
    /// - 30-39: Extension filters (include/exclude extension tools)
    /// - 40-49: Mode-specific filters (ReadOnly, etc.)
    /// - 50-59: Visibility and final filters
    /// </remarks>
    int Priority { get; }

    /// <summary>
    /// Gets the name of this filter for debugging and diagnostics.
    /// </summary>
    string Name { get; }
}
