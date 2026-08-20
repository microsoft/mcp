// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using Microsoft.Mcp.Core.Services.ProcessExecution;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Core.Services.Time;
using NSubstitute;

namespace Azure.Mcp.Core.Tests.Areas.Server;

internal class CommandFactoryHelpers
{
    public static ICommandFactory CreateCommandFactory(IServiceProvider? serviceProvider = default)
    {
        IAreaSetup[] areaSetups = GetAreaSetups();

        var services = serviceProvider ?? CreateDefaultServiceProvider();
        var logger = services.GetRequiredService<ILogger<CommandFactory>>();
        var configurationOptions = Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            Name = "Test Server",
            ShortName = "test",
            Version = "Test Version",
            DisplayName = "Test Display",
            Description = "Test Description",
            RootCommandGroupName = "azmcp"
        });
        var telemetryService = services.GetService<ITelemetryService>() ?? new NoopTelemetryService();
        var commandFactory = new CommandFactory(services, areaSetups, telemetryService, configurationOptions, logger);

        return commandFactory;
    }

    public static IServiceProvider CreateDefaultServiceProvider() => SetupCommonServices().BuildServiceProvider();

    public static IServiceCollection SetupCommonServices()
    {
        IAreaSetup[] areaSetups = GetAreaSetups();

        var builder = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ITelemetryService, NoopTelemetryService>()
            .AddSingleton(Substitute.For<IAzureService>())
            .AddSingleton(Substitute.For<IHttpClientFactory>())
            .AddSingleton(Substitute.For<ICacheService>())
            .AddSingleton(Substitute.For<IDateTimeProvider>())
            .AddSingleton(Substitute.For<IExternalProcessService>())
            .AddSingleton(Substitute.For<IAzureTokenCredentialProvider>())
            .AddSingleton(Substitute.For<IAzureCloudConfiguration>())
            .AddSingleton(Substitute.For<ISubscriptionResolver>())
            .AddSingleton(Substitute.For<IPluginFileReferenceAllowlistProvider>())
            .AddSingleton(Substitute.For<IPluginSkillNameAllowlistProvider>());

        foreach (var area in areaSetups)
        {
            area.ConfigureServices(builder);
        }

        return builder;
    }

    private static IAreaSetup[] GetAreaSetups()
    {
        var areas = typeof(Program).GetField("Areas", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as IAreaSetup[];
        if (areas == null)
        {
            throw new InvalidOperationException("Failed to retrieve area setups from Program class.");
        }

        return areas;
    }
}
