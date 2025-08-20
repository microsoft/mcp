// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;
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
            new Azure.Mcp.Core.Areas.Group.GroupSetup(),
            new Azure.Mcp.Core.Areas.Server.ServerSetup(),
            new Azure.Mcp.Core.Areas.Subscription.SubscriptionSetup(),
            new Azure.Mcp.Core.Areas.Tools.ToolsSetup(),
            // Register Fabric areas
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
