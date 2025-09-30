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
public sealed class CommandGroupDiscoveryStrategy(CommandFactory commandFactory, IOptions<ServiceStartOptions> options, ILogger<CommandGroupDiscoveryStrategy> logger) : BaseDiscoveryStrategy(logger)
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
        var providers = _commandFactory.RootGroup.SubGroup
            .Where(group => !DiscoveryConstants.IgnoredCommandGroups.Contains(group.Name, StringComparer.OrdinalIgnoreCase))
            .Where(group => _options.Value.Namespace == null ||
                           _options.Value.Namespace.Length == 0 ||
                           _options.Value.Namespace.Contains(group.Name, StringComparer.OrdinalIgnoreCase))
            .Select(group => new CommandGroupServerProvider(group)
            {
                ReadOnly = _options.Value.ReadOnly ?? false,
                EntryPoint = EntryPoint,
            })
            .Cast<IMcpServerProvider>();

        return Task.FromResult(providers);
    }
}
