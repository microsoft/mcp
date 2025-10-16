// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Areas.Group;
using Azure.Mcp.Core.Areas.Server;
using Azure.Mcp.Core.Areas.Subscription;
using Azure.Mcp.Core.Areas.Tools;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.Acr;
using Azure.Mcp.Tools.Aks;
using Azure.Mcp.Tools.AppConfig;
using Azure.Mcp.Tools.AppLens;
using Azure.Mcp.Tools.Authorization;
using Azure.Mcp.Tools.AzureBestPractices;
using Azure.Mcp.Tools.AzureIsv;
using Azure.Mcp.Tools.AzureTerraformBestPractices;
using Azure.Mcp.Tools.BicepSchema;
using Azure.Mcp.Tools.CloudArchitect;
using Azure.Mcp.Tools.Cosmos;
using Azure.Mcp.Tools.Deploy;
using Azure.Mcp.Tools.EventGrid;
using Azure.Mcp.Tools.Extension;
using Azure.Mcp.Tools.Foundry;
using Azure.Mcp.Tools.FunctionApp;
using Azure.Mcp.Tools.Grafana;
using Azure.Mcp.Tools.KeyVault;
using Azure.Mcp.Tools.Kusto;
using Azure.Mcp.Tools.LoadTesting;
using Azure.Mcp.Tools.ManagedLustre;
using Azure.Mcp.Tools.Marketplace;
using Azure.Mcp.Tools.Monitor;
using Azure.Mcp.Tools.MySql;
using Azure.Mcp.Tools.Postgres;
using Azure.Mcp.Tools.Quota;
using Azure.Mcp.Tools.Redis;
using Azure.Mcp.Tools.ResourceHealth;
using Azure.Mcp.Tools.Search;
using Azure.Mcp.Tools.ServiceBus;
using Azure.Mcp.Tools.Sql;
using Azure.Mcp.Tools.Storage;
using Azure.Mcp.Tools.VirtualDesktop;
using Azure.Mcp.Tools.Workbooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.UnitTests.Areas.Server;

internal class CommandFactoryHelpers
{
    public static CommandFactory CreateCommandFactory(IServiceProvider? serviceProvider = default)
    {
        IAreaSetup[] areaSetups = [
            // Core areas
            new SubscriptionSetup(),
            new GroupSetup(),
            
            // Tool areas
            new AcrSetup(),
            new AksSetup(),
            new AppConfigSetup(),
            new AppLensSetup(),
            new AuthorizationSetup(),
            new AzureBestPracticesSetup(),
            new AzureIsvSetup(),
            new ManagedLustreSetup(),
            new AzureTerraformBestPracticesSetup(),
            new BicepSchemaSetup(),
            new CloudArchitectSetup(),
            new CosmosSetup(),
            new DeploySetup(),
            new EventGridSetup(),
            new ExtensionSetup(),
            new FoundrySetup(),
            new FunctionAppSetup(),
            new GrafanaSetup(),
            new KeyVaultSetup(),
            new KustoSetup(),
            new LoadTestingSetup(),
            new MarketplaceSetup(),
            new MonitorSetup(),
            new MySqlSetup(),
            new PostgresSetup(),
            new QuotaSetup(),
            new RedisSetup(),
            new ResourceHealthSetup(),
            new SearchSetup(),
            new ServiceBusSetup(),
            new SqlSetup(),
            new StorageSetup(),
            new VirtualDesktopSetup(),
            new WorkbooksSetup(),
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
            // Core areas
            new SubscriptionSetup(),
            new GroupSetup(),
            
            // Tool areas
            new AcrSetup(),
            new AksSetup(),
            new AppConfigSetup(),
            new AppLensSetup(),
            new AuthorizationSetup(),
            new AzureBestPracticesSetup(),
            new AzureIsvSetup(),
            new ManagedLustreSetup(),
            new AzureTerraformBestPracticesSetup(),
            new BicepSchemaSetup(),
            new CloudArchitectSetup(),
            new CosmosSetup(),
            new DeploySetup(),
            new EventGridSetup(),
            new ExtensionSetup(),
            new FoundrySetup(),
            new FunctionAppSetup(),
            new GrafanaSetup(),
            new KeyVaultSetup(),
            new KustoSetup(),
            new LoadTestingSetup(),
            new MarketplaceSetup(),
            new MonitorSetup(),
            new MySqlSetup(),
            new PostgresSetup(),
            new QuotaSetup(),
            new RedisSetup(),
            new ResourceHealthSetup(),
            new SearchSetup(),
            new ServiceBusSetup(),
            new SqlSetup(),
            new StorageSetup(),
            new VirtualDesktopSetup(),
            new WorkbooksSetup(),
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
        public Activity? StartActivity(string activityName) => StartActivity(activityName, null);

        public Activity? StartActivity(string activityName, Implementation? clientInfo) => null;

        public void Dispose()
        {
        }

        public Task InitializeAsync() => Task.CompletedTask;
    }
}
