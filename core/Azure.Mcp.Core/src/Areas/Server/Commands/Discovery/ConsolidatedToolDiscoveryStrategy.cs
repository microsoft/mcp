// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Core.Areas.Server.Models;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.Mcp.Core.Areas.Server.Commands.Discovery;

/// <summary>
/// Discovery strategy that exposes command groups as MCP servers.
/// This strategy converts Azure CLI command groups into MCP servers, allowing them to be accessed via the MCP protocol.
/// </summary>
/// <param name="commandFactory">The command factory used to access available command groups.</param>
/// <param name="options">Options for configuring the service behavior.</param>
/// <param name="logger">Logger instance for this discovery strategy.</param>
public sealed class ConsolidatedToolDiscoveryStrategy(CommandFactory commandFactory, IOptions<ServiceStartOptions> options, ILogger<ConsolidatedToolDiscoveryStrategy> logger) : BaseDiscoveryStrategy(logger)
{
    private readonly CommandFactory _commandFactory = commandFactory;
    private readonly IOptions<ServiceStartOptions> _options = options;

    /// <summary>
    /// Gets or sets the entry point to use for the command group servers.
    /// This can be used to specify a custom entry point for the commands.
    /// </summary>
    public string? EntryPoint { get; set; } = null;
    public static readonly string[] IgnoredCommandGroups = ["server", "tools"];

    /// <summary>
    /// Discovers available command groups and converts them to MCP server providers.
    /// </summary>
    /// <returns>A collection of command group server providers.</returns>
    public override Task<IEnumerable<IMcpServerProvider>> DiscoverServersAsync()
    {
        // Load consolidated tools from JSON file
        var consolidatedTools = new List<ConsolidatedToolDefinition>();
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Azure.Mcp.Core.Areas.Server.Resources.consolidated-tools.json";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                var errorMessage = $"Failed to load embedded resource '{resourceName}'";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            using var jsonDoc = JsonDocument.Parse(json);
            if (!jsonDoc.RootElement.TryGetProperty("consolidated_tools", out var toolsArray))
            {
                var errorMessage = "Property 'consolidated_tools' not found in consolidated-tools.json";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            consolidatedTools = JsonSerializer.Deserialize(toolsArray.GetRawText(), ServerJsonContext.Default.ListConsolidatedToolDefinition) ?? new List<ConsolidatedToolDefinition>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load consolidated tools from JSON file");
            return Task.FromResult<IEnumerable<IMcpServerProvider>>(new List<IMcpServerProvider>());
        }

        var providers = new List<IMcpServerProvider>();
        var allCommands = _commandFactory.AllCommands;

        // Filter out commands that belong to ignored command groups
        var filteredCommands = allCommands
            .Where(kvp =>
            {
                var serviceArea = _commandFactory.GetServiceArea(kvp.Key);
                return serviceArea == null || !IgnoredCommandGroups.Contains(serviceArea, StringComparer.OrdinalIgnoreCase);
            })
            .Where(kvp => _options.Value.ReadOnly == false || kvp.Value.Metadata.ReadOnly == true)
            .Where(kvp =>
            {
                // Filter by namespace if specified
                if (_options.Value.Namespace == null || _options.Value.Namespace.Length == 0)
                {
                    return true;
                }
                var serviceArea = _commandFactory.GetServiceArea(kvp.Key);
                return serviceArea != null && _options.Value.Namespace.Contains(serviceArea, StringComparer.OrdinalIgnoreCase);
            })
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Track unmatched commands
        var unmatchedCommands = new HashSet<string>(filteredCommands.Keys, StringComparer.OrdinalIgnoreCase);

        // Iterate through each consolidated tool definition
        foreach (var consolidatedTool in consolidatedTools)
        {
            // Find all commands that match this consolidated tool's mapped tool list
            var matchingCommands = filteredCommands
                .Where(kvp => consolidatedTool.MappedToolList != null &&
                            consolidatedTool.MappedToolList.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (matchingCommands.Count == 0)
            {
                continue;
            }

#if DEBUG
            // In debug mode, validate that all tools in MappedToolList found a match when conditions are met
            if (_options.Value.ReadOnly == false && (_options.Value.Namespace == null || _options.Value.Namespace.Length == 0))
            {
                if (consolidatedTool.MappedToolList != null)
                {
                    var matchedToolNames = new HashSet<string>(matchingCommands.Select(mc => mc.Key), StringComparer.OrdinalIgnoreCase);
                    var unmatchedToolsInList = consolidatedTool.MappedToolList
                        .Where(toolName => !matchedToolNames.Contains(toolName))
                        .ToList();

                    if (unmatchedToolsInList.Count > 0)
                    {
                        var unmatchedToolsList = string.Join(", ", unmatchedToolsInList);
                        var errorMessage = $"Consolidated tool '{consolidatedTool.Name}' has {unmatchedToolsInList.Count} tools in MappedToolList that didn't find a match in filteredCommands: {unmatchedToolsList}";
                        _logger.LogError(errorMessage);
                        throw new InvalidOperationException(errorMessage);
                    }
                }
            }
#endif

            // Create a new CommandGroup and add the matching commands
            var commandGroup = new CommandGroup(consolidatedTool.Name, consolidatedTool.Description);
            _commandFactory.RootGroup.AddSubGroup(commandGroup);

            foreach (var (commandName, command) in matchingCommands)
            {
                // Validate that the command's metadata matches the consolidated tool's metadata
                if (!AreMetadataEqual(command.Metadata, consolidatedTool.ToolMetadata))
                {
                    var errorMessage = $"Command '{commandName}' has mismatched ToolMetadata for consolidated tool '{consolidatedTool.Name}'. " +
                                     $"Command metadata: [Destructive={command.Metadata.Destructive}, Idempotent={command.Metadata.Idempotent}, " +
                                     $"OpenWorld={command.Metadata.OpenWorld}, ReadOnly={command.Metadata.ReadOnly}, Secret={command.Metadata.Secret}, " +
                                     $"LocalRequired={command.Metadata.LocalRequired}], " +
                                     $"Consolidated tool metadata: [Destructive={consolidatedTool.ToolMetadata?.Destructive}, " +
                                     $"Idempotent={consolidatedTool.ToolMetadata?.Idempotent}, OpenWorld={consolidatedTool.ToolMetadata?.OpenWorld}, " +
                                     $"ReadOnly={consolidatedTool.ToolMetadata?.ReadOnly}, Secret={consolidatedTool.ToolMetadata?.Secret}, " +
                                     $"LocalRequired={consolidatedTool.ToolMetadata?.LocalRequired}]";
#if DEBUG
                    _logger.LogError(errorMessage);
                    throw new InvalidOperationException(errorMessage);
#else
                    _logger.LogWarning(errorMessage);
#endif
                }

                commandGroup.AddCommand(commandName, command);
                // Remove matched commands from the unmatched list
                unmatchedCommands.Remove(commandName);
            }

            commandGroup.ToolMetadata = consolidatedTool.ToolMetadata;

            ConsolidatedToolServerProvider serverProvider = new ConsolidatedToolServerProvider(commandGroup)
            {
                ReadOnly = _options.Value.ReadOnly ?? false,
                EntryPoint = EntryPoint,
            };

            providers.Add(serverProvider);
        }

        // Check for unmatched commands
        if (unmatchedCommands.Count > 0)
        {
            var unmatchedList = string.Join(", ", unmatchedCommands.OrderBy(c => c));
            var errorMessage = $"Found {unmatchedCommands.Count} unmatched commands: {unmatchedList}";
#if DEBUG
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
#else
            _logger.LogWarning("Found {Count} unmatched commands: {Commands}", unmatchedCommands.Count, unmatchedList);
#endif
        }

        return Task.FromResult<IEnumerable<IMcpServerProvider>>(providers);
    }

    /// <summary>
    /// Compares two ToolMetadata objects for equality.
    /// </summary>
    /// <param name="metadata1">The first ToolMetadata to compare.</param>
    /// <param name="metadata2">The second ToolMetadata to compare.</param>
    /// <returns>True if the metadata objects are equal, false otherwise.</returns>
    private static bool AreMetadataEqual(ToolMetadata? metadata1, ToolMetadata? metadata2)
    {
        if (metadata1 == null && metadata2 == null)
        {
            return true;
        }

        if (metadata1 == null || metadata2 == null)
        {
            return false;
        }

        return metadata1.Destructive == metadata2.Destructive &&
               metadata1.Idempotent == metadata2.Idempotent &&
               metadata1.OpenWorld == metadata2.OpenWorld &&
               metadata1.ReadOnly == metadata2.ReadOnly &&
               metadata1.Secret == metadata2.Secret &&
               metadata1.LocalRequired == metadata2.LocalRequired;
    }
}
