// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Areas.Server.Models;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.DependencyInjection;
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
public sealed class ConsolidatedToolDiscoveryStrategy(CommandFactory commandFactory, IServiceProvider serviceProvider, IOptions<ServiceStartOptions> options, ILogger<ConsolidatedToolDiscoveryStrategy> logger) : BaseDiscoveryStrategy(logger)
{
    private readonly CommandFactory _commandFactory = commandFactory;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IOptions<ServiceStartOptions> _options = options;
    private CommandFactory? _consolidatedCommandFactory;

    /// <summary>
    /// Gets or sets the entry point to use for the command group servers.
    /// This can be used to specify a custom entry point for the commands.
    /// </summary>
    public string? EntryPoint { get; set; } = null;
    public static readonly string[] IgnoredCommandGroups = ["server", "tools"];

    /// <summary>
    /// Creates a new CommandFactory with consolidated command groups.
    /// This method builds command groups from the consolidated tools definition
    /// without mutating the original CommandFactory.
    /// </summary>
    /// <returns>A new CommandFactory instance with consolidated command groups.</returns>
    public CommandFactory CreateConsolidatedCommandFactory()
    {
        if (_consolidatedCommandFactory != null)
        {
            return _consolidatedCommandFactory;
        }

        // Load consolidated tool definitions from JSON file
        var consolidatedTools = LoadConsolidatedToolDefinitions();
        
        // Filter commands based on options
        var allCommands = _commandFactory.AllCommands;
        var filteredCommands = FilterCommands(allCommands);

        // Create individual area setups for each consolidated tool
        // This way, each consolidated tool becomes a top-level namespace
        var consolidatedAreas = new List<IAreaSetup>();
        
        var unmatchedCommands = new HashSet<string>(filteredCommands.Keys, StringComparer.OrdinalIgnoreCase);
        
        foreach (var consolidatedTool in consolidatedTools)
        {
            var matchingCommands = filteredCommands
                .Where(kvp => consolidatedTool.MappedToolList != null &&
                            consolidatedTool.MappedToolList.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

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
                    var matchedToolNames = new HashSet<string>(matchingCommands.Keys, StringComparer.OrdinalIgnoreCase);
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

            // Validate metadata for each command
            foreach (var (commandName, command) in matchingCommands)
            {
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

                unmatchedCommands.Remove(commandName);
            }

            // Create an area setup for this consolidated tool
            var area = new SingleConsolidatedToolAreaSetup(
                consolidatedTool,
                matchingCommands
            );
            
            consolidatedAreas.Add(area);
        }

#if DEBUG
        // Check for unmatched commands
        if (unmatchedCommands.Count > 0)
        {
            var unmatchedList = string.Join(", ", unmatchedCommands.OrderBy(c => c));
            var errorMessage = $"Found {unmatchedCommands.Count} unmatched commands: {unmatchedList}";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
#else
        if (unmatchedCommands.Count > 0)
        {
            var unmatchedList = string.Join(", ", unmatchedCommands.OrderBy(c => c));
            _logger.LogWarning("Found {Count} unmatched commands: {Commands}", unmatchedCommands.Count, unmatchedList);
        }
#endif
        
        // Create a new CommandFactory with all consolidated areas
        var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
        var factoryLogger = _serviceProvider.GetRequiredService<ILogger<CommandFactory>>();
        
        _consolidatedCommandFactory = new CommandFactory(
            _serviceProvider,
            consolidatedAreas,
            telemetryService,
            factoryLogger
        );
        
        return _consolidatedCommandFactory;
    }

    private List<ConsolidatedToolDefinition> LoadConsolidatedToolDefinitions()
    {
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

            return JsonSerializer.Deserialize(toolsArray.GetRawText(), ServerJsonContext.Default.ListConsolidatedToolDefinition) ?? new List<ConsolidatedToolDefinition>();
        }
        catch (Exception ex)
        {
            var errorMessage = "Failed to load consolidated tools from JSON file";
            _logger.LogError(ex, errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
    }

    private Dictionary<string, IBaseCommand> FilterCommands(IReadOnlyDictionary<string, IBaseCommand> allCommands)
    {
        return allCommands
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
    }

    /// <summary>
    /// Discovers available command groups and converts them to MCP server providers.
    /// </summary>
    /// <returns>A collection of command group server providers.</returns>
    public override Task<IEnumerable<IMcpServerProvider>> DiscoverServersAsync()
    {
        return Task.FromResult<IEnumerable<IMcpServerProvider>>(new List<IMcpServerProvider>());
    }

    /// <summary>
    /// Compares two ToolMetadata objects for equality.
    /// </summary>
    /// <param name="metadata1">The first ToolMetadata to compare.</param>
    /// <param name="metadata2">The second ToolMetadata to compare.</param>
    /// <returns>True if the metadata objects are equal, false otherwise.</returns>
    internal static bool AreMetadataEqual(ToolMetadata? metadata1, ToolMetadata? metadata2)
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

/// <summary>
/// Represents a single consolidated tool as an IAreaSetup.
/// Each instance creates a top-level namespace for one consolidated tool in the CommandFactory.
/// This allows NamespaceToolLoader to see each consolidated tool as a separate top-level namespace.
/// </summary>
internal sealed class SingleConsolidatedToolAreaSetup : IAreaSetup
{
    private readonly ConsolidatedToolDefinition _consolidatedTool;
    private readonly Dictionary<string, IBaseCommand> _matchingCommands;

    public SingleConsolidatedToolAreaSetup(
        ConsolidatedToolDefinition consolidatedTool,
        Dictionary<string, IBaseCommand> matchingCommands)
    {
        _consolidatedTool = consolidatedTool;
        _matchingCommands = matchingCommands;
    }

    public string Name => _consolidatedTool.Name ?? string.Empty;
    public string Title => Name;

    public void ConfigureServices(IServiceCollection services)
    {
        // No additional services needed
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create command group for this consolidated tool
        var commandGroup = new CommandGroup(Name, Title);

        // Add all matching commands to this group
        foreach (var cmd in _matchingCommands)
        {
            commandGroup.AddCommand(cmd.Key, cmd.Value);
        }

        // Set tool metadata from the consolidated tool definition
        commandGroup.ToolMetadata = _consolidatedTool.ToolMetadata;

        return commandGroup;
    }
}
