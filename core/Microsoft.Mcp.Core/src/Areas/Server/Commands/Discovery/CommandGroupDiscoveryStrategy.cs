// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;

/// <summary>
/// Discovery strategy that exposes command groups as MCP servers.
/// This strategy converts Azure CLI command groups into MCP servers, allowing them to be accessed via the MCP protocol.
/// </summary>
/// <param name="commandFactory">The command factory used to access available command groups.</param>
/// <param name="configuration">Runtime configuration for the server.</param>
/// <param name="logger">Logger instance for this discovery strategy.</param>
public sealed class CommandGroupDiscoveryStrategy(
    ICommandFactory commandFactory,
    IOptions<ServerRuntimeConfiguration> configuration,
    ILogger<CommandGroupDiscoveryStrategy> logger) : BaseDiscoveryStrategy(logger)
{
    private readonly ICommandFactory _commandFactory = commandFactory;
    private readonly IOptions<ServerRuntimeConfiguration> _configuration = configuration;

    /// <summary>
    /// Gets or sets the entry point to use for the command group servers.
    /// This can be used to specify a custom entry point for the commands.
    /// </summary>
    public string? EntryPoint { get; set; } = null;

    /// <inheritdoc/>
    public override Task<IEnumerable<IMcpServerProvider>> DiscoverServersAsync(CancellationToken cancellationToken)
    {
        var providers = _commandFactory.RootGroup.SubGroup
            .Where(group => !DiscoveryConstants.IgnoredCommandGroups.Contains(group.Name, StringComparer.OrdinalIgnoreCase))
            .Where(group => _configuration.Value.Namespace == null ||
                           _configuration.Value.Namespace.Length == 0 ||
                           _configuration.Value.Namespace.Contains(group.Name, StringComparer.OrdinalIgnoreCase))
            .Select(group => new CommandGroupServerProvider(group)
            {
                ReadOnly = _configuration.Value.ReadOnly,
                Transport = _configuration.Value.Transport,
                StructuredOutputMode = _configuration.Value.StructuredOutputMode,
                EntryPoint = EntryPoint,
            })
            .Cast<IMcpServerProvider>();

        return Task.FromResult(providers);
    }
}
