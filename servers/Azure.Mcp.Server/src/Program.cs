// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Core.Services.ProcessExecution;
using Azure.Mcp.Core.Services.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Telemetry;

namespace Azure.Mcp.Server;

internal class Program
{
    private static IAreaSetup[] Areas = RegisterAreas();
    private static AreaRegistrationInfo[] DescriptorAreas = RegisterDescriptorAreas();

    private static async Task<int> Main(string[] args)
    {
        try
        {
            // Fast path: Handle simple metadata requests without initializing service infrastructure
            // This optimization reduces startup time from ~10s to <3s for these queries
            var fastPathResult = TryHandleFastPathRequest(args);
            if (fastPathResult.HasValue)
            {
                return fastPathResult.Value;
            }

            ServiceStartCommand.ConfigureServices = ConfigureServices;
            ServiceStartCommand.InitializeServicesAsync = InitializeServicesAsync;

            PluginTelemetryCommand.ConfigureServices = ConfigureServices;
            PluginTelemetryCommand.InitializeServicesAsync = InitializeServicesAsync;

            ServiceCollection services = new();

            ConfigureServices(services);

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var serviceProvider = services.BuildServiceProvider();
            await InitializeServicesAsync(serviceProvider);

            var commandFactory = serviceProvider.GetRequiredService<ICommandFactory>();
            var rootCommand = commandFactory.RootCommand;
            var parseResult = rootCommand.Parse(args);
            var status = await parseResult.InvokeAsync();

            if (status == 0)
            {
                status = (int)HttpStatusCode.OK;
            }

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
        return [];
    }

    private static AreaRegistrationInfo[] RegisterDescriptorAreas()
    {
        return [
            // Core areas
            AreaRegistrationInfo.Create<Microsoft.Mcp.Core.Areas.Server.ServerRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Core.Areas.Tools.ToolsRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Core.Areas.Group.GroupRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Core.Areas.Subscription.SubscriptionRegistration>(),
            // Recommended tools
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.AzureBestPractices.AzureBestPracticesRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Extension.ExtensionRegistration>(),
            // Azure service areas
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Acr.AcrRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Advisor.AdvisorRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Aks.AksRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.AppConfig.AppConfigRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.AppLens.AppLensRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.ApplicationInsights.ApplicationInsightsRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.AppService.AppServiceRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Authorization.AuthorizationRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.AzureIsv.AzureIsvRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.AzureMigrate.AzureMigrateRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.AzureTerraformBestPractices.AzureTerraformBestPracticesRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.BicepSchema.BicepSchemaRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.CloudArchitect.CloudArchitectRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Communication.CommunicationRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Compute.ComputeRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.ConfidentialLedger.ConfidentialLedgerRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.ContainerApps.ContainerAppsRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Cosmos.CosmosRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Deploy.DeployRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.DeviceRegistry.DeviceRegistryRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.EventGrid.EventGridRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.EventHubs.EventHubsRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.FileShares.FileSharesRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.FoundryExtensions.FoundryExtensionsRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.FunctionApp.FunctionAppRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Functions.FunctionsRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Grafana.GrafanaRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.KeyVault.KeyVaultRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Kusto.KustoRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.LoadTesting.LoadTestingRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.ManagedLustre.ManagedLustreRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Marketplace.MarketplaceRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Monitor.MonitorRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.MySql.MySqlRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Policy.PolicyRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Postgres.PostgresRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Pricing.PricingRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Quota.QuotaRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Redis.RedisRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.ResourceHealth.ResourceHealthRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Search.SearchRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.ServiceBus.ServiceBusRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.ServiceFabric.ServiceFabricRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.SignalR.SignalRRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Speech.SpeechRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Sql.SqlRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Storage.StorageRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.StorageSync.StorageSyncRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.VirtualDesktop.VirtualDesktopRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.WellArchitectedFramework.WellArchitectedFrameworkRegistration>(),
            AreaRegistrationInfo.Create<Azure.Mcp.Tools.Workbooks.WorkbooksRegistration>(),
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
    /// based on the command line input. This container is a local variable in
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
    /// but such transport-specific registrations must be overridden within
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
        var thisAssembly = typeof(Program).Assembly;

        services.InitializeConfigurationAndOptions();
        services.ConfigureOpenTelemetry();

        services.AddMemoryCache();
        services.AddSingleton<IExternalProcessService, ExternalProcessService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IResourceGroupService, ResourceGroupService>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();
        services.AddSingleton<ICommandFactory, CommandFactory>();

        // !!! WARNING !!!
        // stdio-transport-specific implementations of ITenantService and ICacheService.
        // The http-transport-specific implementations and configurations must be registered
        // within ServiceStartCommand.ExecuteAsync().
        services.AddHttpClientServices(configureDefaults: true);
        services.AddAzureTenantService();
        services.AddSingleUserCliCacheService();

        foreach (var area in Areas)
        {
            services.AddSingleton(area);
            area.ConfigureServices(services);
        }

        // Register descriptor-based areas
        foreach (var areaInfo in DescriptorAreas)
        {
            services.AddSingleton(areaInfo);
            areaInfo.RegisterServices(services);
        }

        services.AddRegistryRoot(thisAssembly, $"registry.json");

        services.AddSingleton<IServerInstructionsProvider>(
            new ResourceServerInstructionsProvider(thisAssembly, $"azure-rules.txt"));

        services.AddSingleton<IConsolidatedToolDefinitionProvider>(sp =>
            ActivatorUtilities.CreateInstance<ResourceConsolidatedToolDefinitionProvider>(sp, thisAssembly, $"consolidated-tools.json"));

        services.AddSingleton<IPluginFileReferenceAllowlistProvider>(sp =>
            ActivatorUtilities.CreateInstance<ResourcePluginFileReferenceAllowlistProvider>(sp, thisAssembly, $"allowed-plugin-file-references.json"));

        services.AddSingleton<IPluginSkillNameAllowlistProvider>(sp =>
            ActivatorUtilities.CreateInstance<ResourcePluginSkillNameAllowlistProvider>(sp, thisAssembly, $"allowed-skill-names.json"));
    }

    internal static async Task InitializeServicesAsync(IServiceProvider serviceProvider)
    {
        ServiceStartOptions? options = serviceProvider.GetService<IOptions<ServiceStartOptions>>()?.Value;

        if (options != null)
        {
            // Update the UserAgentPolicy for all Azure service calls to include the transport type.
            var transport = string.IsNullOrEmpty(options.Transport) ? TransportTypes.StdIo : options.Transport;
            BaseAzureService.InitializeUserAgentPolicy(transport);

            if (options.DangerouslyDisableRetryLimits)
            {
                BaseAzureService.DisableRetryLimits();
            }
        }

        // Perform any initialization before starting the service.
        // If the initialization operation fails, do not continue because we do not want
        // invalid telemetry published.
        var telemetryService = serviceProvider.GetRequiredService<ITelemetryService>();
        await telemetryService.InitializeAsync();
    }

    /// <summary>
    /// Attempts to handle the --version flag without requiring full service initialization.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code if request was handled, null otherwise.</returns>
    private static int? TryHandleFastPathRequest(string[] args)
    {
        // Handle --version flag
        if (args.Length == 1 && (args[0] == "--version" || args[0] == "-v"))
        {
            var version = AssemblyHelper.GetFullAssemblyVersion(typeof(Program).Assembly);
            Console.WriteLine(version);
            return 0;
        }

        return null;
    }
}
