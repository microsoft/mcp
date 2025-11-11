// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Core.Services.ProcessExecution;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Core.Services.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ServiceStartCommand = Azure.Mcp.Core.Areas.Server.Commands.ServiceStartCommand;

internal class Program
{
    private static IAreaSetup[] Areas = RegisterAreas();

    private static async Task<int> Main(string[] args)
    {
        try
        {
            ServiceStartCommand.ConfigureServices = ConfigureServices;
            ServiceStartCommand.InitializeServicesAsync = InitializeServicesAsync;

            ServiceCollection services = new();
            ConfigureServices(services);

            services.AddLogging(builder =>
            {
                builder.ConfigureOpenTelemetryLogger();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var serviceProvider = services.BuildServiceProvider();
            await InitializeServicesAsync(serviceProvider);

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
            new Azure.Mcp.Tools.AzureAIBestPractices.AzureAIBestPracticesSetup(),
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
            new Azure.Mcp.Tools.ManagedLustre.ManagedLustreSetup(),
            new Azure.Mcp.Tools.AzureTerraformBestPractices.AzureTerraformBestPracticesSetup(),
            new Azure.Mcp.Tools.Deploy.DeploySetup(),
            new Azure.Mcp.Tools.EventGrid.EventGridSetup(),
            new Azure.Mcp.Tools.Acr.AcrSetup(),
            new Azure.Mcp.Tools.BicepSchema.BicepSchemaSetup(),
            new Azure.Mcp.Tools.Cosmos.CosmosSetup(),
            new Azure.Mcp.Tools.CloudArchitect.CloudArchitectSetup(),
            new Azure.Mcp.Tools.ConfidentialLedger.ConfidentialLedgerSetup(),
            new Azure.Mcp.Tools.EventHubs.EventHubsSetup(),
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
            new Azure.Mcp.Tools.Communication.CommunicationSetup(),
            new Azure.Mcp.Tools.ResourceHealth.ResourceHealthSetup(),
            new Azure.Mcp.Tools.Search.SearchSetup(),
            new Azure.Mcp.Tools.Speech.SpeechSetup(),
            new Azure.Mcp.Tools.ServiceBus.ServiceBusSetup(),
            new Azure.Mcp.Tools.SignalR.SignalRSetup(),
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

    /// <summary>
    /// <para>
    /// Configures services for dependency injection.
    /// </para>
    /// <para>
    /// WARNING: This method is being used for TWO DEPENDENCY INJECTION CONTAINERS:
    /// </para>
    /// <list type="number">
    /// <item>
    /// <see cref="Main"/>'s command picking: The container used to populate instances of
    /// <see cref="IBaseCommand"/> and selected by <see cref="CommandFactory"/>
    /// baesd on the command line input. This container is a local variable in
    /// <see cref="Main"/>, and it is not tied to
    /// <c>Microsoft.Extensions.Hosting.IHostBuilder</c> (stdio) nor any
    /// <c>Microsoft.AspNetCore.Hosting.IWebHostBuilder</c> (http).
    /// </item>
    /// <item>
    /// <see cref="ServiceStartCommand"/>'s execution: The container is created by some
    /// dynamically created <c>Microsoft.Extensions.Hosting.IHostBuilder</c> (stdio) or
    /// <c>Microsoft.AspNetCore.Hosting.IWebHostBuilder</c> (http). While the
    /// <see cref="IBaseCommand.ExecuteAsync"/>instance of <see cref="ServiceStartCommand"/>
    /// is created by the first container, this second container it creates and runs is
    /// built separately during <see cref="ServiceStartCommand.ExecuteAsync"/>. Thus, this
    /// container is built and this <see cref="ConfigureServices"/> method is called sometime
    /// during that method execution.
    /// </item>
    /// </list>
    /// <para>
    /// DUE TO THIS DUAL USAGE, PLEASE BE VERY CAREFUL WHEN MODIFYING THIS METHOD. This
    /// method may have some expectations, but it and all methods it calls must be safe for
    /// both the stdio and http transport modes.
    /// </para>
    /// <para>
    /// For example, most <see cref="IBaseCommand"/> instances take an indirect dependency
    /// on <see cref="ITenantService"/> or <see cref="ICacheService"/>, both of which have
    /// transport-specific implementations. This method can add the stdio-specific
    /// implementation to allow the first container (used for command picking) to work,
    /// but such transport-specific registrations must be overriden within
    /// <see cref="ServiceStartCommand.ExecuteAsync"/> with the appropriate
    /// transport-specific implementation based on command line arguments.
    /// </para>
    /// <para>
    /// This large doc comment is copy/pasta in each Program.cs file of this repo, so if
    /// you're reading this, please keep them in sync and/or add specific warnings per
    /// project if needed. Below is the list of known differences:
    /// </para>
    /// <list type="bullet">
    /// <item>No differences. This is also copy/pasta as a placeholder for this project.</item>
    /// </list>
    /// </summary>
    /// <param name="services">A service collection.</param>
    internal static void ConfigureServices(IServiceCollection services)
    {
        services.ConfigureOpenTelemetry();

        services.AddMemoryCache();
        services.AddSingleton<IExternalProcessService, ExternalProcessService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IResourceGroupService, ResourceGroupService>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();
        services.AddSingleton<CommandFactory>();

        // !!! WARNING !!!
        // stdio-transport-specific implementations of ITenantService and ICacheService.
        // The http-traport-specific implementations and configurations must be registered
        // within ServiceStartCommand.ExecuteAsync().
        services.AddAzureTenantService();
        services.AddSingleUserCliCacheService();

        foreach (var area in Areas)
        {
            services.AddSingleton(area);
            area.ConfigureServices(services);
        }
    }

    internal static async Task InitializeServicesAsync(IServiceProvider serviceProvider)
    {
        // Perform any initialization before starting the service.
        // If the initialization operation fails, do not continue because we do not want
        // invalid telemetry published.
        var telemetryService = serviceProvider.GetRequiredService<ITelemetryService>();
        await telemetryService.InitializeAsync();
    }
}
