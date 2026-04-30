// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Helpers;
using NSubstitute;

namespace Azure.Mcp.Core.UnitTests;

public class RegistryDiscoveryStrategyHelper
{
    public static RegistryDiscoveryStrategy CreateStrategy(ServiceStartOptions? options = null, ILogger<RegistryDiscoveryStrategy>? logger = null)
    {
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ServiceStartOptions());
        logger ??= Substitute.For<ILogger<RegistryDiscoveryStrategy>>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var registryRoot = RegistryServerHelper.GetRegistryRoot(typeof(Server.Program).Assembly, "Azure.Mcp.Server.Resources.registry.json");
        return new(serviceOptions, logger, httpClientFactory, registryRoot!);
    }
}
