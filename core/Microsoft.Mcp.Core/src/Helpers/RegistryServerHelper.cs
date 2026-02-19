// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Core.Areas.Server;
using Azure.Mcp.Core.Areas.Server.Models;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;

namespace Azure.Mcp.Core.Helpers;

public sealed class RegistryServerHelper
{
    public static string GetRegistryServerHttpClientName(string serverName)
    {
        return $"{typeof(RegistryServerProvider).FullName}.{serverName}";
    }

    public static IRegistryRoot? GetRegistryRoot()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("registry.json", StringComparison.OrdinalIgnoreCase));
        if (resourceName is null)
        {
            return null;
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }
        var registry = JsonSerializer.Deserialize(stream, ServerJsonContext.Default.RegistryRoot);
        if (registry?.Servers is null)
        {
            return null;
        }

        return registry;
    }
}
