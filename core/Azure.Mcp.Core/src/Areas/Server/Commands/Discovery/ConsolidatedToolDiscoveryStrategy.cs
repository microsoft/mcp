// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
public sealed class ConsolidatedToolDiscoveryStrategy(CommandFactory commandFactory, IOptions<ServiceStartOptions> options, ILogger<CommandGroupDiscoveryStrategy> logger) : BaseDiscoveryStrategy(logger)
{
    private readonly CommandFactory _commandFactory = commandFactory;
    private readonly IOptions<ServiceStartOptions> _options = options;

    /// <summary>
    /// Gets or sets the entry point to use for the command group servers.
    /// This can be used to specify a custom entry point for the commands.
    /// </summary>
    public string? EntryPoint { get; set; } = null;

    /// <summary>
    /// Discovers available command groups and converts them to MCP server providers.
    /// </summary>
    /// <returns>A collection of command group server providers.</returns>
    public override Task<IEnumerable<IMcpServerProvider>> DiscoverServersAsync()
    {
        // Find all commands with the same CompositeToolMapped value
        var allCommands = _commandFactory.AllCommands;
        var matchingCommands = allCommands
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value.CompositeToolMapped) && 
                        string.Equals(kvp.Value.CompositeToolMapped, "get_azure_best_practices", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        // Create a new CommandGroup and add the matching commands
        var commandGroup = new CommandGroup("get_azure_best_practices", "Retrieve Azure best practices and infrastructure schema for code generation, deployment, and operations. Covers general Azure practices, Azure Functions best practices, Terraform configurations, Bicep template schemas, and deployment best practices.");
        _commandFactory.RootGroup.AddSubGroup(commandGroup);
        foreach (var (commandName, command) in matchingCommands)
        {
            commandGroup.AddCommand(commandName, command);
            // Extract just the command name (remove any prefix)
            // TODO: It is going to be all get commands???
            // var simpleName = commandName.Replace("azmcp_", "").Replace("_", ".");
            // commandGroup.AddCommand(simpleName, command);
        }
        commandGroup.ToolMetadata = new ToolMetadata
        {
            Destructive = false,
            Idempotent = true,
            OpenWorld = false,
            ReadOnly = true,
            LocalRequired = false,
            Secret = false
        };

        CommandGroupServerProvider serverProvider = new CommandGroupServerProvider(commandGroup)
        {
            ReadOnly = _options.Value.ReadOnly ?? false,
            EntryPoint = EntryPoint,
        };

        var providers = new List<IMcpServerProvider> { serverProvider };
        // providers.AddRange(_commandFactory.RootGroup.SubGroup
        //     .Where(group => _options.Value.Namespace == null ||
        //                    _options.Value.Namespace.Length == 0 ||
        //                    _options.Value.Namespace.Contains(group.Name, StringComparer.OrdinalIgnoreCase))
        //     .Select(group => new CommandGroupServerProvider(group)
        //     {
        //         ReadOnly = _options.Value.ReadOnly ?? false,
        //         EntryPoint = EntryPoint,
        //     })
        //     .Cast<IMcpServerProvider>();

        return Task.FromResult<IEnumerable<IMcpServerProvider>>(providers);
    }
}
