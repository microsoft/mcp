// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
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
            new Azure.Mcp.Core.Areas.Group.GroupSetup(),
            new Azure.Mcp.Core.Areas.Server.ServerSetup(),
            new Azure.Mcp.Core.Areas.Subscription.SubscriptionSetup(),
            new Azure.Mcp.Core.Areas.Tools.ToolsSetup(),
            // Register Fabric areas
            new Fabric.Mcp.Tools.PublicApi.FabricPublicApiSetup(),
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
