// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Core.Services.ProcessExecution;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Core.Services.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            var builder = Host.CreateApplicationBuilder();
            ConfigureServices(builder.Services, builder.Environment);

            builder.Services.AddLogging(builder =>
            {
                builder.ConfigureOpenTelemetryLogger();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var serviceProvider = builder.Services.BuildServiceProvider();
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
            new Azure.Mcp.Core.Areas.Server.ServerSetup(),
            new Azure.Mcp.Core.Areas.Tools.ToolsSetup()
            // Register template areas
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
    /// <item>Template's Program.cs doesn't add ITenantService.</item>
    /// </list>
    /// </summary>
    /// <param name="services">A service collection.</param>
    internal static void ConfigureServices(IServiceCollection services, IHostEnvironment hostEnvironment)
    {
        services.ConfigureOpenTelemetry(hostEnvironment);

        services.AddMemoryCache();
        services.AddSingleton<IExternalProcessService, ExternalProcessService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<CommandFactory>();

        // !!! WARNING !!!
        // stdio-transport-specific implementations of ICacheService.
        // The http-traport-specific implementations and configurations must be registered
        // within ServiceStartCommand.ExecuteAsync().
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
