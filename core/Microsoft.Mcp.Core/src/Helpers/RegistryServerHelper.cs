// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Models;

namespace Azure.Mcp.Core.Helpers;

public sealed class RegistryServerHelper
{
    public static string GetRegistryServerHttpClientName(string serverName)
    {
        return $"{typeof(RegistryServerProvider).FullName}.{serverName}";
    }

    public static IRegistryRoot? GetRegistryRoot(Assembly assembly, string resourcePattern)
    {
        var resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, resourcePattern);
        var json = EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
        var registry = JsonSerializer.Deserialize(json, ServerJsonContext.Default.RegistryRoot);
        if (registry?.Servers is null)
        {
            return null;
        }

        return registry;
    }
}
