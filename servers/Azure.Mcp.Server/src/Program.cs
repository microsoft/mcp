// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.ServerInstructions;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Models;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Caching;
using Microsoft.Mcp.Core.Services.ProcessExecution;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Core.Services.Time;

namespace Azure.Mcp.Server;

internal class Program
{
    private static readonly IAreaSetup[] Areas = RegisterAreas();

    // Derived from the registered ServerSetup instance so the name stays in sync
    // with the actual area registration — no magic string duplication.
    private static readonly string ServerAreaName =
        Array.Find(Areas, static a => a is Microsoft.Mcp.Core.Areas.Server.ServerSetup)?.Name ?? "server";

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

            // The server start and plugin-telemetry containers always need full area registration.
            ServiceStartCommand.ConfigureServices = services => ConfigureServices(services);
            ServiceStartCommand.InitializeServicesAsync = InitializeServicesAsync;

            PluginTelemetryCommand.ConfigureServices = services => ConfigureServices(services);
            PluginTelemetryCommand.InitializeServicesAsync = InitializeServicesAsync;

            // Optimization: detect the target service area early so we can skip registering the
            // other 60+ areas in the DI container.
            var targetAreaName = GetTargetAreaName(args);

            ServiceCollection services = new();

            ConfigureServices(services, targetAreaName);

            services.AddLogging(builder =>
            {
                builder.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var serviceProvider = services.BuildServiceProvider();

            // Optimization: run telemetry initialization concurrently with CommandFactory resolution.
            // Telemetry init reads the MAC address (NetworkInterface) and device ID (registry on Windows)
            // from the thread pool. CommandFactory resolves command singletons for the target area.
            // Both complete in parallel so neither adds to the other's latency on the hot path.
            //
            // If GetRequiredService throws before we reach the await, telemetryInitTask would become
            // an unobserved faulted task. Observe it (suppressing its exception) in that path so the
            // DI failure is what surfaces to the user — not a TaskScheduler.UnobservedTaskException.
            var telemetryInitTask = InitializeServicesAsync(serviceProvider);
            ICommandFactory commandFactory;
            try
            {
                commandFactory = serviceProvider.GetRequiredService<ICommandFactory>();
            }
            catch
            {
                await telemetryInitTask.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                throw;
            }
            await telemetryInitTask;

            // Short-circuit for --learn: return command metadata without executing.
            // This MUST happen before Parse/InvokeAsync so that System.CommandLine's
            // required-option validation cannot block the discovery response, and to
            // avoid wastefully parsing 250+ commands and options only to discard the result.
            if (Array.Exists(args, a => string.Equals(a, ICommandFactory.LearnOptionName, StringComparison.OrdinalIgnoreCase)))
            {
                var learnJson = commandFactory.GetLearnResponse(args);
                Console.WriteLine(learnJson);
                using var learnDoc = JsonDocument.Parse(learnJson);
                var learnStatus = learnDoc.RootElement.TryGetProperty("status", out var statusEl)
                    ? statusEl.GetInt32()
                    : (int)HttpStatusCode.InternalServerError;
                return (learnStatus >= (int)HttpStatusCode.OK && learnStatus < (int)HttpStatusCode.MultipleChoices) ? 0 : 1;
            }

            var rootCommand = commandFactory.RootCommand;
            var parseResult = rootCommand.Parse(args);
            var command = parseResult.CommandResult.Command;
            int status = 0;

            if (command is ExtendedCommand extendedCommand &&
                (extendedCommand.BaseCommand is ServiceStartCommand || extendedCommand.BaseCommand is PluginTelemetryCommand))
            {
                // One of the special commands that need to be handled differently.
                status = await parseResult.InvokeAsync();
            }
            else
            {
                // Command wasn't one of the registered ServerSetup commands, so bind up a Host of all the services
                // to run the command.
                var builder = Host.CreateApplicationBuilder();
                builder.Logging.ClearProviders();
                builder.Logging.AddEventSourceLogger();
                ConfigureServices(builder.Services);
                builder.Services.AddAzureMcpServer(new()
                {
                    Transport = TransportTypes.StdIo
                });

                using var host = builder.Build();

                await InitializeServicesAsync(host.Services);
                await host.StartAsync();

                commandFactory = host.Services.GetRequiredService<ICommandFactory>();
                rootCommand = commandFactory.RootCommand;
                parseResult = rootCommand.Parse(args);

                status = await parseResult.InvokeAsync();

                await host.StopAsync();
                await host.WaitForShutdownAsync();
            }

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

        return [
            // Register core areas
            new Azure.Mcp.Tools.AzureBestPractices.AzureBestPracticesSetup(),
            new Azure.Mcp.Tools.Extension.ExtensionSetup(),
            new Azure.Mcp.Core.Areas.Group.GroupSetup(),
            new Microsoft.Mcp.Core.Areas.Server.ServerSetup(),
            new Azure.Mcp.Core.Areas.Subscription.SubscriptionSetup(),
            new Microsoft.Mcp.Core.Areas.Tools.ToolsSetup(),
            // Register Azure service areas
            new Azure.Mcp.Tools.Aks.AksSetup(),
            new Azure.Mcp.Tools.AppConfig.AppConfigSetup(),
            new Azure.Mcp.Tools.AppLens.AppLensSetup(),
            new Azure.Mcp.Tools.AppService.AppServiceSetup(),
            new Azure.Mcp.Tools.Authorization.AuthorizationSetup(),
            new Azure.Mcp.Tools.AzureBackup.AzureBackupSetup(),
            new Azure.Mcp.Tools.AzureIsv.AzureIsvSetup(),
            new Azure.Mcp.Tools.ManagedLustre.ManagedLustreSetup(),
            new Azure.Mcp.Tools.AzureMigrate.AzureMigrateSetup(),
            new Azure.Mcp.Tools.AzureTerraform.AzureTerraformSetup(),
            new Azure.Mcp.Tools.AzureTerraformBestPractices.AzureTerraformBestPracticesSetup(),
            new Azure.Mcp.Tools.Deploy.DeploySetup(),
            new Azure.Mcp.Tools.DeviceRegistry.DeviceRegistrySetup(),
            new Azure.Mcp.Tools.EventGrid.EventGridSetup(),
            new Azure.Mcp.Tools.Acr.AcrSetup(),
            new Azure.Mcp.Tools.Advisor.AdvisorSetup(),
            new Azure.Mcp.Tools.BicepSchema.BicepSchemaSetup(),
            new Azure.Mcp.Tools.Cosmos.CosmosSetup(),
            new Azure.Mcp.Tools.CloudArchitect.CloudArchitectSetup(),
            new Azure.Mcp.Tools.Communication.CommunicationSetup(),
            new Azure.Mcp.Tools.Compute.ComputeSetup(),
            new Azure.Mcp.Tools.ConfidentialLedger.ConfidentialLedgerSetup(),
            new Azure.Mcp.Tools.ContainerApps.ContainerAppsSetup(),
            new Azure.Mcp.Tools.EventHubs.EventHubsSetup(),
            new Azure.Mcp.Tools.FileShares.FileSharesSetup(),
            new Azure.Mcp.Tools.FoundryExtensions.FoundryExtensionsSetup(),
            new Azure.Mcp.Tools.FunctionApp.FunctionAppSetup(),
            new Azure.Mcp.Tools.Functions.FunctionsSetup(),
            new Azure.Mcp.Tools.Grafana.GrafanaSetup(),
            new Azure.Mcp.Tools.KeyVault.KeyVaultSetup(),
            new Azure.Mcp.Tools.Kusto.KustoSetup(),
            new Azure.Mcp.Tools.LoadTesting.LoadTestingSetup(),
            new Azure.Mcp.Tools.Marketplace.MarketplaceSetup(),
            new Azure.Mcp.Tools.Quota.QuotaSetup(),
            new Azure.Mcp.Tools.Monitor.MonitorSetup(),
            new Azure.Mcp.Tools.ApplicationInsights.ApplicationInsightsSetup(),
            new Azure.Mcp.Tools.MySql.MySqlSetup(),
            new Azure.Mcp.Tools.Policy.PolicySetup(),
            new Azure.Mcp.Tools.Postgres.PostgresSetup(),
            new Azure.Mcp.Tools.Pricing.PricingSetup(),
            new Azure.Mcp.Tools.Redis.RedisSetup(),
            new Azure.Mcp.Tools.ResourceHealth.ResourceHealthSetup(),
            new Azure.Mcp.Tools.Search.SearchSetup(),
            new Azure.Mcp.Tools.Speech.SpeechSetup(),
            new Azure.Mcp.Tools.ServiceBus.ServiceBusSetup(),
            new Azure.Mcp.Tools.ServiceFabric.ServiceFabricSetup(),
            new Azure.Mcp.Tools.SignalR.SignalRSetup(),
            new Azure.Mcp.Tools.Sql.SqlSetup(),
            new Azure.Mcp.Tools.Storage.StorageSetup(),
            new Azure.Mcp.Tools.StorageSync.StorageSyncSetup(),
            new Azure.Mcp.Tools.VirtualDesktop.VirtualDesktopSetup(),
            new Azure.Mcp.Tools.WellArchitectedFramework.WellArchitectedFrameworkSetup(),
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
        => Console.WriteLine(JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse));

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
    /// <item>
    /// This project's <see cref="ConfigureServices"/> accepts an optional <paramref name="areaFilter"/>.
    /// When set (single-command CLI invocations), only infrastructure areas and the named
    /// Azure service area register their services. Server-mode resource providers
    /// (registry, instructions, allowlists) are replaced with null-stubs when an area filter
    /// is active, since those providers are only needed by the MCP server transport.
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="services">A service collection.</param>
    /// <param name="areaFilter">
    /// When non-<see langword="null"/>, only the named Azure service area and infrastructure areas
    /// (those with <see cref="CommandCategory"/> other than <see cref="CommandCategory.AzureServices"/>)
    /// have their services registered. Pass <see langword="null"/> for full initialization.
    /// </param>
    internal static void ConfigureServices(IServiceCollection services, string? areaFilter = null)
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
        services.AddSingleUserCliCacheService(disabled: true);

        foreach (var area in Areas)
        {
            // When areaFilter is set (CLI path), skip Azure service areas that don't match the target.
            // Non-Azure-service areas (Category != AzureServices) provide shared infrastructure
            // (command routing, server start, subscription listing, etc.) and must always be registered.
            // Any area whose Category is AzureServices and whose name doesn't match the filter is skipped;
            // this avoids registering services (HTTP clients, SDKs, etc.) for 60+ irrelevant services.
            if (areaFilter != null &&
                area.Category == CommandCategory.AzureServices &&
                !string.Equals(area.Name, areaFilter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            services.AddSingleton(area);
            area.ConfigureServices(services);
        }

        // Optimization: server-mode providers (registry, instructions, plugin allowlists) are only
        // used when running as an MCP server. For CLI area invocations they are never resolved, so
        // register lightweight stubs to avoid reading embedded resources on every CLI call.
        if (areaFilter == null || string.Equals(areaFilter, ServerAreaName, StringComparison.OrdinalIgnoreCase))
        {
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
        else
        {
            services.AddSingleton<IRegistryRoot>(new RegistryRoot());
            services.AddSingleton<IServerInstructionsProvider>(new NullServerInstructionsProvider());
            services.AddSingleton<IConsolidatedToolDefinitionProvider>(new NullConsolidatedToolDefinitionProvider());
            services.AddSingleton<IPluginFileReferenceAllowlistProvider>(new NullPluginFileReferenceAllowlistProvider());
            services.AddSingleton<IPluginSkillNameAllowlistProvider>(new NullPluginSkillNameAllowlistProvider());
        }
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
    /// Extracts the target service area name from the first non-option CLI token.
    /// Returns <see langword="null"/> when no area can be determined (e.g. bare <c>--help</c>,
    /// <c>--learn</c>, <c>--version</c>, or no arguments), which causes <see cref="CommandFactory"/>
    /// to fall back to full initialization of all areas.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>The area name (e.g. "storage"), or <see langword="null"/>.</returns>
    internal static string? GetTargetAreaName(string[] args)
    {
        // Scan for the first token that is not an option flag (does not start with '-').
        // '--' is the POSIX end-of-options marker: everything after it is a positional argument,
        // but for our purposes we stop scanning at '--' and treat it as "no area found".
        string? firstToken = null;
        foreach (var arg in args)
        {
            if (arg == "--")
                break;
            if (arg.Length > 0 && !arg.StartsWith('-'))
            {
                firstToken = arg;
                break;
            }
        }

        if (firstToken is null)
        {
            return null;
        }

        // The "tools" area introspects factory.AllCommands to enumerate every registered command.
        // Filtering to only the tools area would return an empty result set, so skip optimization.
        if (string.Equals(firstToken, "tools", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Only apply the optimization when the first token is a known registered area.
        // If the token doesn't match any area (e.g. a typo), fall through to full initialization
        // so System.CommandLine can produce helpful "Did you mean..." suggestions.
        if (!Array.Exists(Areas, a => string.Equals(a.Name, firstToken, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        return firstToken;
    }

    /// <summary>
    /// Attempts to handle the --version flag without requiring full service initialization.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code if request was handled, null otherwise.</returns>
    private static int? TryHandleFastPathRequest(string[] args)
    {
        // Handle --version / -v flags before DI initialization
        if (args.Length == 1 && (args[0] == "--version" || args[0] == "-v"))
        {
            var version = AssemblyHelper.GetFullAssemblyVersion(typeof(Program).Assembly);
            Console.WriteLine(version);
            return 0;
        }

        return null;
    }
}
