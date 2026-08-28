// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

public sealed partial class CommandFactoryToolLoader
{
    private static readonly string[] s_ignoredConsolidatedCommandGroups = ["server", "tools"];

    private static IReadOnlyList<CommandGroup> CreateConsolidatedCommandGroups(
        ICommandFactory commandFactory,
        IConsolidatedToolDefinitionProvider definitionProvider,
        ServerRuntimeConfiguration configuration,
        ILogger logger)
    {
        var filteredCommands = commandFactory.AllCommands
            .Where(command =>
            {
                var serviceArea = commandFactory.GetServiceArea(command.Key);
                return serviceArea == null || !s_ignoredConsolidatedCommandGroups.Contains(serviceArea, StringComparer.OrdinalIgnoreCase);
            })
            .Where(command => !configuration.ReadOnly || command.Value.Metadata.ReadOnly)
            .Where(command => !configuration.IsHttpMode || !command.Value.Metadata.LocalRequired)
            .Where(command =>
            {
                if (configuration.Namespace == null || configuration.Namespace.Length == 0)
                {
                    return true;
                }

                var serviceArea = commandFactory.GetServiceArea(command.Key);
                return serviceArea != null && configuration.Namespace.Contains(serviceArea, StringComparer.OrdinalIgnoreCase);
            })
            .ToDictionary(command => command.Key, command => command.Value);

        var unmatchedCommands = new HashSet<string>(filteredCommands.Keys, StringComparer.OrdinalIgnoreCase);
        var commandGroups = new List<CommandGroup>();

        foreach (var definition in definitionProvider.GetToolDefinitions())
        {
            var matchingCommands = filteredCommands
                .Where(command => definition.MappedToolList.Contains(command.Key, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(command => command.Key, command => command.Value);

            if (matchingCommands.Count == 0)
            {
                continue;
            }

#if DEBUG
            if (!configuration.ReadOnly && (configuration.Namespace == null || configuration.Namespace.Length == 0))
            {
                var unmatchedDefinitions = definition.MappedToolList
                    .Where(toolName => !matchingCommands.ContainsKey(toolName))
                    .ToList();

                if (unmatchedDefinitions.Count > 0)
                {
                    throw new InvalidOperationException(
                        $"Consolidated tool '{definition.Name}' has mappings without matching commands: {string.Join(", ", unmatchedDefinitions)}");
                }
            }
#endif

            foreach (var (commandName, command) in matchingCommands)
            {
                ValidateConsolidatedMetadata(commandName, command.Metadata, definition.Name, definition.ToolMetadata, logger);
                unmatchedCommands.Remove(commandName);
            }

            var commandGroup = new CommandGroup(definition.Name, definition.Description, definition.Name)
            {
                ToolMetadata = definition.ToolMetadata
            };

            foreach (var command in matchingCommands)
            {
                commandGroup.AddCommand(command.Key, command.Value);
            }

            commandGroups.Add(commandGroup);
        }

        if (unmatchedCommands.Count > 0)
        {
            var unmatchedList = string.Join(", ", unmatchedCommands.OrderBy(command => command));
#if DEBUG
            throw new InvalidOperationException($"Found {unmatchedCommands.Count} unmatched commands: {unmatchedList}");
#else
            logger.LogWarning("Found {Count} unmatched commands: {Commands}", unmatchedCommands.Count, unmatchedList);
#endif
        }

        return commandGroups;
    }

    private static void ValidateConsolidatedMetadata(
        string commandName,
        ToolMetadata commandMetadata,
        string consolidatedToolName,
        ToolMetadata consolidatedMetadata,
        ILogger logger)
    {
        if (AreMetadataEqual(commandMetadata, consolidatedMetadata))
        {
            return;
        }

        var errorMessage = $"Command '{commandName}' has mismatched ToolMetadata for consolidated tool '{consolidatedToolName}'.";
#if DEBUG
        throw new InvalidOperationException(errorMessage);
#else
        logger.LogWarning("{Message}", errorMessage);
#endif
    }

    internal static bool AreMetadataEqual(ToolMetadata? first, ToolMetadata? second)
    {
        if (first == null || second == null)
        {
            return first == second;
        }

        return first.Destructive == second.Destructive &&
               first.Idempotent == second.Idempotent &&
               first.OpenWorld == second.OpenWorld &&
               first.ReadOnly == second.ReadOnly &&
               first.Secret == second.Secret &&
               first.LocalRequired == second.LocalRequired;
    }
}
