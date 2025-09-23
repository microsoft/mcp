// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Areas.Subscription;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.AppConfig;
using Azure.Mcp.Tools.Deploy;
using Azure.Mcp.Tools.KeyVault;
using Azure.Mcp.Tools.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.UnitTests.Areas.Server;

internal class CommandFactoryHelpers
{
    public static CommandFactory CreateCommandFactory(IServiceProvider? serviceProvider = default)
    {
        IAreaSetup[] areaSetups = [
            new SubscriptionSetup(),
            new KeyVaultSetup(),
            new StorageSetup(),
            new DeploySetup(),
            new AppConfigSetup()
        ];

        var services = serviceProvider ?? CreateDefaultServiceProvider();
        var logger = services.GetRequiredService<ILogger<CommandFactory>>();
        var telemetryService = services.GetService<ITelemetryService>() ?? new NoOpTelemetryService();
        var commandFactory = new CommandFactory(services, areaSetups, telemetryService, logger);

        return commandFactory;
    }

    public static IServiceProvider CreateDefaultServiceProvider()
    {
        return SetupCommonServices().BuildServiceProvider();
    }

    public static IServiceCollection SetupCommonServices()
    {
        IAreaSetup[] areaSetups = [
            new SubscriptionSetup(),
            new KeyVaultSetup(),
            new StorageSetup(),
            new DeploySetup(),
            new AppConfigSetup()
        ];

        var builder = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ITelemetryService, NoOpTelemetryService>();

        foreach (var area in areaSetups)
        {
            area.ConfigureServices(builder);
        }

        return builder;
    }

    public class NoOpTelemetryService : ITelemetryService
    {
        public ValueTask<Activity?> StartActivity(string activityName)
        {
            return ValueTask.FromResult<Activity?>(null);
        }

        public ValueTask<Activity?> StartActivity(string activityName, Implementation? clientInfo)
        {
            return ValueTask.FromResult<Activity?>(null);
        }

        public void Dispose()
        {
        }
    }
}
