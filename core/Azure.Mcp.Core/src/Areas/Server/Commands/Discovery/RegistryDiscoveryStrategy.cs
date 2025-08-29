// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
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
public sealed class RegistryDiscoveryStrategy(IOptions<ServiceStartOptions> options, ILogger<RegistryDiscoveryStrategy> logger) : BaseDiscoveryStrategy(logger)
{
    private readonly IOptions<ServiceStartOptions> _options = options;
    /// <summary>
    /// Discovers available MCP servers from the embedded registry.
    /// </summary>
    /// <returns>A collection of server providers defined in the registry.</returns>
    public override async Task<IEnumerable<IMcpServerProvider>> DiscoverServersAsync()
    {
        var registryRoot = await LoadRegistryAsync();
        if (registryRoot == null)
        {
            return [];
        }

        return registryRoot
            .Servers!
            .Where(s => _options.Value.Namespace == null ||
                       _options.Value.Namespace.Length == 0 ||
                       _options.Value.Namespace.Contains(s.Key, StringComparer.OrdinalIgnoreCase))
            .Select(s => new RegistryServerProvider(s.Key, s.Value))
            .Cast<IMcpServerProvider>();
    }

    /// <summary>
    /// Loads the registry configuration from the embedded resource file.
    /// </summary>
    /// <returns>The deserialized registry root containing server configurations, or null if not found.</returns>
    private async Task<RegistryRoot?> LoadRegistryAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("registry.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            return null;
        }

        await using var stream = assembly.GetManifestResourceStream(resourceName)!;
        var registry = await JsonSerializer.DeserializeAsync(stream, ServerJsonContext.Default.RegistryRoot);

        if (registry?.Servers != null)
        {
            foreach (var kvp in registry.Servers)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.Name = kvp.Key;
                }
            }
        }

        return registry;
    }
}
