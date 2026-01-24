// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Models;
using Azure.Mcp.Core.Areas.Server.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.Mcp.Core.Areas.Server.Commands.Discovery;

/// <summary>
/// Discovers MCP servers from an embedded registry.json resource file.
/// This strategy loads server configurations from a JSON resource bundled with the assembly.
/// </summary>
/// <param name="options">Options for configuring the service behavior.</param>
/// <param name="logger">Logger instance for this discovery strategy.</param>
/// <param name="httpClientFactory">Factory that can create HttpClient objects.</param>
/// <param name="registryRoot">Manifest of all the MCP server registries.</param>
public sealed class RegistryDiscoveryStrategy(IOptions<ServiceStartOptions> options, ILogger<RegistryDiscoveryStrategy> logger, IHttpClientFactory httpClientFactory, IRegistryRoot registryRoot) : BaseDiscoveryStrategy(logger)
{
    private readonly IOptions<ServiceStartOptions> _options = options;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    /// <inheritdoc/>
    public override async Task<IEnumerable<IMcpServerProvider>> DiscoverServersAsync(CancellationToken cancellationToken)
    {
        if (registryRoot == null)
        {
            return [];
        }

        return registryRoot
            .Servers!
            .Where(s => _options.Value.Namespace == null ||
                       _options.Value.Namespace.Length == 0 ||
                       _options.Value.Namespace.Contains(s.Key, StringComparer.OrdinalIgnoreCase))
            .Select(s => new RegistryServerProvider(s.Key, s.Value, _httpClientFactory))
            .Cast<IMcpServerProvider>();
    }
}
