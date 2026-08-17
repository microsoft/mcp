// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Helpers;
using NSubstitute;
using ExtensionsOptions = Microsoft.Extensions.Options;

namespace Azure.Mcp.Core.Tests;

public class RegistryDiscoveryStrategyHelper
{
    public static RegistryDiscoveryStrategy CreateStrategy(ServerStartOptions? options = null, ILogger<RegistryDiscoveryStrategy>? logger = null)
    {
        var serviceOptions = ExtensionsOptions.Options.Create(options ?? new ServerStartOptions());
        logger ??= Substitute.For<ILogger<RegistryDiscoveryStrategy>>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var registryRoot = RegistryServerHelper.GetRegistryRoot(typeof(Server.Program).Assembly, "Azure.Mcp.Server.Resources.registry.json");
        return new(serviceOptions, logger, httpClientFactory, registryRoot!);
    }
}
