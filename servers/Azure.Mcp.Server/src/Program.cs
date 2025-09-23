// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Core.Services.ProcessExecution;
using Azure.Mcp.Core.Services.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal class Program
{
    private static IAreaSetup[] Areas = RegisterAreas();

    private static async Task<int> Main(string[] args)
    {
        try
        {
            Azure.Mcp.Core.Areas.Server.Commands.ServiceStartCommand.ConfigureServices = ConfigureServices;

            ServiceCollection services = new();
            ConfigureServices(services);

            services.AddLogging(builder =>
            {
                builder.ConfigureOpenTelemetryLogger();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var serviceProvider = services.BuildServiceProvider();

            var commandFactory = serviceProvider.GetRequiredService<CommandFactory>();
            var rootCommand = commandFactory.RootCommand;
            var parseResult = rootCommand.Parse(args);
            var status = await parseResult.InvokeAsync();

            return (status >= (int)HttpStatusCode.OK && status < (int)HttpStatusCode.MultipleChoices) ? 0 : 1;
        }
        catch (Exception ex)
        {
            WriteResponse(new CommandResponse
            {
                Status = HttpStatusCode.InternalServerError,
                Message = ex.Message,
                Duration = 0
            });
            return 1;
        }
    }
    private static IAreaSetup[] RegisterAreas()
    {

        return [
            // Register core areas
            new Azure.Mcp.Tools.AzureBestPractices.AzureBestPracticesSetup(),
            new Azure.Mcp.Tools.Extension.ExtensionSetup(),
            new Azure.Mcp.Core.Areas.Group.GroupSetup(),
            new Azure.Mcp.Core.Areas.Server.ServerSetup(),
            new Azure.Mcp.Core.Areas.Subscription.SubscriptionSetup(),
            new Azure.Mcp.Core.Areas.Tools.ToolsSetup(),
            // Register Azure service areas
            new Azure.Mcp.Tools.Aks.AksSetup(),
            new Azure.Mcp.Tools.AppConfig.AppConfigSetup(),
            new Azure.Mcp.Tools.AppLens.AppLensSetup(),
            new Azure.Mcp.Tools.AppService.AppServiceSetup(),
            new Azure.Mcp.Tools.Authorization.AuthorizationSetup(),
            new Azure.Mcp.Tools.AzureIsv.AzureIsvSetup(),
            new Azure.Mcp.Tools.AzureManagedLustre.AzureManagedLustreSetup(),
            new Azure.Mcp.Tools.AzureTerraformBestPractices.AzureTerraformBestPracticesSetup(),
            new Azure.Mcp.Tools.Deploy.DeploySetup(),
            new Azure.Mcp.Tools.EventGrid.EventGridSetup(),
            new Azure.Mcp.Tools.Acr.AcrSetup(),
            new Azure.Mcp.Tools.BicepSchema.BicepSchemaSetup(),
            new Azure.Mcp.Tools.Cosmos.CosmosSetup(),
            new Azure.Mcp.Tools.CloudArchitect.CloudArchitectSetup(),
            new Azure.Mcp.Tools.Foundry.FoundrySetup(),
            new Azure.Mcp.Tools.FunctionApp.FunctionAppSetup(),
            new Azure.Mcp.Tools.Grafana.GrafanaSetup(),
            new Azure.Mcp.Tools.KeyVault.KeyVaultSetup(),
            new Azure.Mcp.Tools.Kusto.KustoSetup(),
            new Azure.Mcp.Tools.LoadTesting.LoadTestingSetup(),
            new Azure.Mcp.Tools.Marketplace.MarketplaceSetup(),
            new Azure.Mcp.Tools.Quota.QuotaSetup(),
            new Azure.Mcp.Tools.Monitor.MonitorSetup(),
            new Azure.Mcp.Tools.ApplicationInsights.ApplicationInsightsSetup(),
            new Azure.Mcp.Tools.MySql.MySqlSetup(),
            new Azure.Mcp.Tools.Postgres.PostgresSetup(),
            new Azure.Mcp.Tools.Redis.RedisSetup(),
            new Azure.Mcp.Tools.ResourceHealth.ResourceHealthSetup(),
            new Azure.Mcp.Tools.Search.SearchSetup(),
            new Azure.Mcp.Tools.ServiceBus.ServiceBusSetup(),
            new Azure.Mcp.Tools.Sql.SqlSetup(),
            new Azure.Mcp.Tools.Storage.StorageSetup(),
            new Azure.Mcp.Tools.VirtualDesktop.VirtualDesktopSetup(),
            new Azure.Mcp.Tools.Workbooks.WorkbooksSetup(),
#if !BUILD_NATIVE
            // IMPORTANT: DO NOT MODIFY OR ADD EXCLUSIONS IN THIS SECTION
            // This block must remain as-is.
            // If the "(Native AOT) Build module" stage fails in CI,
            // follow the AOT compatibility guide instead of changing this list:
            // https://github.com/Azure/azure-mcp/blob/main/docs/aot-compatibility.md

#endif
        ];
    }

    private static void WriteResponse(CommandResponse response)
    {
        Console.WriteLine(JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse));
    }

    internal static void ConfigureServices(IServiceCollection services)
    {
        services.ConfigureOpenTelemetry();

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IExternalProcessService, ExternalProcessService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<ITenantService, TenantService>();
        services.AddSingleton<IResourceGroupService, ResourceGroupService>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();
        services.AddSingleton<CommandFactory>();

        foreach (var area in Areas)
        {
            services.AddSingleton(area);
            area.ConfigureServices(services);
        }
    }
}
