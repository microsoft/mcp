// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Core.Areas.Subscription;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AppConfig;
using Azure.Mcp.Tools.Deploy;
using Azure.Mcp.Tools.KeyVault;
using Azure.Mcp.Tools.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Services.Telemetry;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.UnitTests.Areas.Server;

internal class CommandFactoryHelpers
{
    public static CommandFactory CreateCommandFactory(IServiceProvider? serviceProvider = default)
    {
        IServiceProvider services = serviceProvider ?? new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var logger = services.GetRequiredService<ILogger<CommandFactory>>();
        var telemetryService = services.GetService<ITelemetryService>() ?? new NoOpTelemetryService();

        IAreaSetup[] areaSetups = [
            new SubscriptionSetup(),
            new KeyVaultSetup(),
            new StorageSetup(),
            new DeploySetup(),
            new AppConfigSetup()
        ];

        var commandFactory = new CommandFactory(services, areaSetups, telemetryService, logger);

        return commandFactory;
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
