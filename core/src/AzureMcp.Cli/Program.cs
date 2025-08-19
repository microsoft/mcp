// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Builder;
using System.Diagnostics;
using AzureMcp.Core.Areas;
using AzureMcp.Core.Commands;
using AzureMcp.Core.Services.Azure.ResourceGroup;
using AzureMcp.Core.Services.Azure.Subscription;
using AzureMcp.Core.Services.Azure.Tenant;
using AzureMcp.Core.Services.Caching;
using AzureMcp.Core.Services.ProcessExecution;
using AzureMcp.Core.Services.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal class Program
{
    private static IAreaSetup[] Areas = RegisterAreas();

    private static async Task<int> Main(string[] args)
    {
        try
        {
            AzureMcp.Core.Areas.Server.Commands.ServiceStartCommand.ConfigureServices = ConfigureServices;

            ServiceCollection services = new();
            ConfigureServices(services);

            services.AddLogging(builder =>
            {
                builder.ConfigureOpenTelemetryLogger();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var serviceProvider = services.BuildServiceProvider();

            var parser = BuildCommandLineParser(serviceProvider);
            return await parser.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            WriteResponse(new CommandResponse
            {
                Status = 500,
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
            new AzureMcp.AzureBestPractices.AzureBestPracticesSetup(),
            new AzureMcp.Extension.ExtensionSetup(),
            new AzureMcp.Core.Areas.Group.GroupSetup(),
            new AzureMcp.Core.Areas.Server.ServerSetup(),
            new AzureMcp.Core.Areas.Subscription.SubscriptionSetup(),
            new AzureMcp.Core.Areas.Tools.ToolsSetup(),
            // Register Azure service areas
            new AzureMcp.Aks.AksSetup(),
            new AzureMcp.AppConfig.AppConfigSetup(),
            new AzureMcp.Authorization.AuthorizationSetup(),
            new AzureMcp.AzureIsv.AzureIsvSetup(),
            new AzureMcp.AzureTerraformBestPractices.AzureTerraformBestPracticesSetup(),
            new AzureMcp.Deploy.DeploySetup(),
            new AzureMcp.Acr.AcrSetup(),
            new AzureMcp.Foundry.FoundrySetup(),
            new AzureMcp.FunctionApp.FunctionAppSetup(),
            new AzureMcp.Grafana.GrafanaSetup(),
            new AzureMcp.KeyVault.KeyVaultSetup(),
            new AzureMcp.Kusto.KustoSetup(),
            new AzureMcp.LoadTesting.LoadTestingSetup(),
            new AzureMcp.Marketplace.MarketplaceSetup(),
            new AzureMcp.Quota.QuotaSetup(),
            new AzureMcp.Monitor.MonitorSetup(),
            new AzureMcp.Postgres.PostgresSetup(),
            new AzureMcp.Redis.RedisSetup(),
            new AzureMcp.Search.SearchSetup(),
            new AzureMcp.ServiceBus.ServiceBusSetup(),
            new AzureMcp.Sql.SqlSetup(),
            new AzureMcp.Storage.StorageSetup(),
            new AzureMcp.VirtualDesktop.VirtualDesktopSetup(),
            new AzureMcp.Workbooks.WorkbooksSetup(),
            new AzureMcp.Ai.AiSetup(),
#if !BUILD_NATIVE
            new AzureMcp.BicepSchema.BicepSchemaSetup(),
            new AzureMcp.Cosmos.CosmosSetup(),
            new AzureMcp.ResourceHealth.ResourceHealthSetup(),
#endif
        ];
    }

    private static Parser BuildCommandLineParser(IServiceProvider serviceProvider)
    {
        var commandFactory = serviceProvider.GetRequiredService<CommandFactory>();
        var rootCommand = commandFactory.RootCommand;
        var builder = new CommandLineBuilder(rootCommand);

        builder.AddMiddleware(async (context, next) =>
        {
            var commandContext = new CommandContext(serviceProvider, Activity.Current);
            var command = context.ParseResult.CommandResult.Command;
            if (command.Handler is IBaseCommand baseCommand)
            {
                var validationResult = baseCommand.Validate(context.ParseResult.CommandResult, commandContext.Response);
                if (!validationResult.IsValid)
                {
                    WriteResponse(commandContext.Response);
                    context.ExitCode = commandContext.Response.Status;
                    return;
                }
            }
            await next(context);
        });

        builder.UseDefaults();
        return builder.Build();
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
