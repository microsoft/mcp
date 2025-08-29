// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

internal class Program
{
    private static IAreaSetup[] Areas = RegisterAreas();

    private static async Task<int> Main(string[] args)
    {
        try
        {
            //Azure.Mcp.Core.Areas.Server.Commands.ServiceStartCommand.ConfigureServices = ConfigureServices;

            ServiceCollection services = new();
            //ConfigureServices(services);

            services.AddLogging(builder =>
            {
                //builder.ConfigureOpenTelemetryLogger();
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
            // Register template areas
        ];
    }

    private static Parser BuildCommandLineParser(IServiceProvider serviceProvider)
    {
        var commandFactory = serviceProvider.GetRequiredService<ICommandFactory>();
        var rootCommand = commandFactory.RootGroup;

        throw new InvalidOperationException("Implement");

        //var builder = new CommandLineBuilder("rootCommand);
        //var builder = new CommandLineBuilder();
        //builder.AddMiddleware(async (context, next) =>
        //{
        //    var commandContext = new CommandContext(serviceProvider, Activity.Current);
        //    var command = context.ParseResult.CommandResult.Command;
        //    if (command.Handler is IBaseCommand baseCommand)
        //    {
        //        var validationResult = baseCommand.Validate(context.ParseResult.CommandResult, commandContext.Response);
        //        if (!validationResult.IsValid)
        //        {
        //            WriteResponse(commandContext.Response);
        //            context.ExitCode = commandContext.Response.Status;
        //            return;
        //        }
        //    }
        //    await next(context);
        //});

        //builder.UseDefaults();
        //return builder.Build();
    }

    private static void WriteResponse(CommandResponse response)
    {
        Console.WriteLine(JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse));
    }

    internal static void ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICommandFactory>();

        foreach (var area in Areas)
        {
            services.AddSingleton(area);
            area.ConfigureServices(services);
        }
    }
}
