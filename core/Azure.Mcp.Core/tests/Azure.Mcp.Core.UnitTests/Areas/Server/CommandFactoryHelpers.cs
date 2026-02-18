// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Core.Areas.Group;
using Azure.Mcp.Core.Areas.Subscription;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Acr;
using Azure.Mcp.Tools.Advisor;
using Azure.Mcp.Tools.Aks;
using Azure.Mcp.Tools.AppConfig;
using Azure.Mcp.Tools.AppLens;
using Azure.Mcp.Tools.ApplicationInsights;
using Azure.Mcp.Tools.AppService;
using Azure.Mcp.Tools.Authorization;
using Azure.Mcp.Tools.AzureBackup;
using Azure.Mcp.Tools.AzureBestPractices;
using Azure.Mcp.Tools.AzureIsv;
using Azure.Mcp.Tools.AzureMigrate;
using Azure.Mcp.Tools.AzureTerraform;
using Azure.Mcp.Tools.AzureTerraformBestPractices;
using Azure.Mcp.Tools.BicepSchema;
using Azure.Mcp.Tools.CloudArchitect;
using Azure.Mcp.Tools.Communication;
using Azure.Mcp.Tools.Compute;
using Azure.Mcp.Tools.ConfidentialLedger;
using Azure.Mcp.Tools.ContainerApps;
using Azure.Mcp.Tools.Cosmos;
using Azure.Mcp.Tools.Deploy;
using Azure.Mcp.Tools.DeviceRegistry;
using Azure.Mcp.Tools.EventGrid;
using Azure.Mcp.Tools.EventHubs;
using Azure.Mcp.Tools.Extension;
using Azure.Mcp.Tools.FileShares;
using Azure.Mcp.Tools.FoundryExtensions;
using Azure.Mcp.Tools.FunctionApp;
using Azure.Mcp.Tools.Functions;
using Azure.Mcp.Tools.Grafana;
using Azure.Mcp.Tools.KeyVault;
using Azure.Mcp.Tools.Kusto;
using Azure.Mcp.Tools.LoadTesting;
using Azure.Mcp.Tools.ManagedLustre;
using Azure.Mcp.Tools.Marketplace;
using Azure.Mcp.Tools.Monitor;
using Azure.Mcp.Tools.MySql;
using Azure.Mcp.Tools.Policy;
using Azure.Mcp.Tools.Postgres;
using Azure.Mcp.Tools.Pricing;
using Azure.Mcp.Tools.Quota;
using Azure.Mcp.Tools.Redis;
using Azure.Mcp.Tools.ResourceHealth;
using Azure.Mcp.Tools.Search;
using Azure.Mcp.Tools.ServiceBus;
using Azure.Mcp.Tools.ServiceFabric;
using Azure.Mcp.Tools.SignalR;
using Azure.Mcp.Tools.Speech;
using Azure.Mcp.Tools.Sql;
using Azure.Mcp.Tools.Storage;
using Azure.Mcp.Tools.StorageSync;
using Azure.Mcp.Tools.VirtualDesktop;
using Azure.Mcp.Tools.WellArchitectedFramework;
using Azure.Mcp.Tools.Workbooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using Microsoft.Mcp.Core.Services.ProcessExecution;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Core.Services.Time;
using ModelContextProtocol.Protocol;
using NSubstitute;

namespace Azure.Mcp.Core.UnitTests.Areas.Server;

internal class CommandFactoryHelpers
{
    public static ICommandFactory CreateCommandFactory(IServiceProvider? serviceProvider = default)
    {
        IAreaSetup[] areaSetups = [
            // Core areas
            new SubscriptionSetup(),
            new GroupSetup(),

            // Tool areas
            new AcrSetup(),
            new AdvisorSetup(),
            new AksSetup(),
            new AppConfigSetup(),
            new AppLensSetup(),
            new ApplicationInsightsSetup(),
            new AppServiceSetup(),
            new AuthorizationSetup(),
            new AzureBackupSetup(),
            new AzureBestPracticesSetup(),
            new AzureIsvSetup(),
            new ManagedLustreSetup(),
            new AzureMigrateSetup(),
            new AzureTerraformSetup(),
            new AzureTerraformBestPracticesSetup(),
            new BicepSchemaSetup(),
            new CloudArchitectSetup(),
            new CommunicationSetup(),
            new ComputeSetup(),
            new ConfidentialLedgerSetup(),
            new ContainerAppsSetup(),
            new CosmosSetup(),
            new DeploySetup(),
            new DeviceRegistrySetup(),
            new EventGridSetup(),
            new EventHubsSetup(),
            new ExtensionSetup(),
            new FileSharesSetup(),
            new FoundryExtensionsSetup(),
            new FunctionAppSetup(),
            new FunctionsSetup(),
            new GrafanaSetup(),
            new KeyVaultSetup(),
            new KustoSetup(),
            new LoadTestingSetup(),
            new MarketplaceSetup(),
            new MonitorSetup(),
            new MySqlSetup(),
            new PolicySetup(),
            new PostgresSetup(),
            new PricingSetup(),
            new QuotaSetup(),
            new RedisSetup(),
            new ResourceHealthSetup(),
            new SearchSetup(),
            new ServiceBusSetup(),
            new ServiceFabricSetup(),
            new SignalRSetup(),
            new SpeechSetup(),
            new SqlSetup(),
            new StorageSetup(),
            new StorageSyncSetup(),
            new VirtualDesktopSetup(),
            new WellArchitectedFrameworkSetup(),
            new WorkbooksSetup(),
        ];

        var services = serviceProvider ?? CreateDefaultServiceProvider();
        var logger = services.GetRequiredService<ILogger<CommandFactory>>();
        var configurationOptions = Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            Name = "Test Server",
            Version = "Test Version",
            DisplayName = "Test Display",
            RootCommandGroupName = "azmcp"
        });
        var telemetryService = services.GetService<ITelemetryService>() ?? new NoOpTelemetryService();
        var commandFactory = new CommandFactory(services, areaSetups, telemetryService, configurationOptions, logger);

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
            new AdvisorSetup(),
            new AksSetup(),
            new AppConfigSetup(),
            new AppLensSetup(),
            new ApplicationInsightsSetup(),
            new AppServiceSetup(),
            new AuthorizationSetup(),
            new AzureBackupSetup(),
            new AzureBestPracticesSetup(),
            new AzureIsvSetup(),
            new ManagedLustreSetup(),
            new AzureMigrateSetup(),
            new AzureTerraformSetup(),
            new AzureTerraformBestPracticesSetup(),
            new BicepSchemaSetup(),
            new CloudArchitectSetup(),
            new CommunicationSetup(),
            new ComputeSetup(),
            new ConfidentialLedgerSetup(),
            new ContainerAppsSetup(),
            new CosmosSetup(),
            new DeploySetup(),
            new DeviceRegistrySetup(),
            new EventGridSetup(),
            new EventHubsSetup(),
            new ExtensionSetup(),
            new FileSharesSetup(),
            new FoundryExtensionsSetup(),
            new FunctionAppSetup(),
            new FunctionsSetup(),
            new GrafanaSetup(),
            new KeyVaultSetup(),
            new KustoSetup(),
            new LoadTestingSetup(),
            new MarketplaceSetup(),
            new MonitorSetup(),
            new MySqlSetup(),
            new PolicySetup(),
            new PostgresSetup(),
            new PricingSetup(),
            new QuotaSetup(),
            new RedisSetup(),
            new ResourceHealthSetup(),
            new SearchSetup(),
            new ServiceBusSetup(),
            new ServiceFabricSetup(),
            new SignalRSetup(),
            new SpeechSetup(),
            new SqlSetup(),
            new StorageSetup(),
            new StorageSyncSetup(),
            new VirtualDesktopSetup(),
            new WellArchitectedFrameworkSetup(),
            new WorkbooksSetup(),
        ];

        var builder = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ITelemetryService, NoOpTelemetryService>()
            .AddSingleton(Substitute.For<ISubscriptionService>())
            .AddSingleton(Substitute.For<IResourceGroupService>())
            .AddSingleton(Substitute.For<ITenantService>())
            .AddSingleton(Substitute.For<IHttpClientFactory>())
            .AddSingleton(Substitute.For<ICacheService>())
            .AddSingleton(Substitute.For<IDateTimeProvider>())
            .AddSingleton(Substitute.For<IExternalProcessService>())
            .AddSingleton(Substitute.For<IAzureTokenCredentialProvider>())
            .AddSingleton(Substitute.For<IAzureCloudConfiguration>());

        foreach (var area in areaSetups)
        {
            area.ConfigureServices(builder);
        }

        return builder;
    }

    public class NoOpTelemetryService : ITelemetryService
    {
        public Activity? StartActivity(string activityName) => null;

        public Activity? StartActivity(string activityName, Implementation? clientInfo) => null;

        public void Dispose()
        {
        }

        public Task InitializeAsync() => Task.CompletedTask;
    }
}
