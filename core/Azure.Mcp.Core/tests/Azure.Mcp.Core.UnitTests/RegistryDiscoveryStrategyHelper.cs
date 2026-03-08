// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Options;

namespace Azure.Mcp.Core.UnitTests;

public class RegistryDiscoveryStrategyHelper
{
    public static RegistryDiscoveryStrategy CreateStrategy(ServiceStartOptions? options = null, ILogger<RegistryDiscoveryStrategy>? logger = null)
    {
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ServiceStartOptions());
        logger = logger ?? NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<RegistryDiscoveryStrategy>>();
        var httpClientFactory = NSubstitute.Substitute.For<IHttpClientFactory>();
        var registryRoot = RegistryServerHelper.GetRegistryRoot(typeof(Azure.Mcp.Server.Program).Assembly, "Azure.Mcp.Server.Resources.registry.json");
        return new RegistryDiscoveryStrategy(serviceOptions, logger, httpClientFactory, registryRoot!);
    }
}
