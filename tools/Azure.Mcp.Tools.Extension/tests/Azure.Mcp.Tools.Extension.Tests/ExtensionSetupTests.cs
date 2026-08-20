// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.ProcessExecution;
using Microsoft.Mcp.Core.Services.Time;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Extension.Tests;

public sealed class ExtensionSetupTests
{
    private static IServiceProvider BuildServiceProvider(ServerRuntimeConfiguration? startOptions)
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());

        var setup = new ExtensionSetup();
        setup.ConfigureServices(services);

        services.AddSingleton(Substitute.For<IExternalProcessService>());
        services.AddSingleton(Substitute.For<IAzureService>());
        services.AddSingleton(Substitute.For<IDateTimeProvider>());
        services.AddSingleton(Substitute.For<IAzureTokenCredentialProvider>());
        services.AddSingleton(Substitute.For<IAzureCloudConfiguration>());
        services.AddSingleton(Substitute.For<ISubscriptionResolver>());

        if (startOptions is not null)
        {
            services.AddSingleton(startOptions);
        }

        return services.BuildServiceProvider();
    }

    [Fact]
    public void RegisterCommands_HttpMode_ExcludesAzqrCommand()
    {
        // Arrange: HTTP (remote)
        var provider = BuildServiceProvider(new() { Transport = TransportTypes.Http });
        var setup = new ExtensionSetup();

        // Act
        var commandGroup = setup.RegisterCommands(provider);

        // Assert
        // In remote mode the azqr command that shells out to an external process is excluded.
        Assert.DoesNotContain("azqr", commandGroup.Commands.Keys);
        // cli subgroup and its commands should still be present
        Assert.Contains(commandGroup.SubGroup, g => g.Name == "cli");
    }

    [Fact]
    public void RegisterCommands_LocalStdioMode_IncludesAzqrCommand()
    {
        // Arrange: stdio transport
        var provider = BuildServiceProvider(new() { Transport = TransportTypes.StdIo });
        var setup = new ExtensionSetup();

        // Act
        var commandGroup = setup.RegisterCommands(provider);

        // Assert
        // In local mode the azqr command that shells out to an external process is allowed.
        Assert.Contains("azqr", commandGroup.Commands.Keys);
        Assert.Contains(commandGroup.SubGroup, g => g.Name == "cli");
    }

    [Fact]
    public void RegisterCommands_NoServerRuntimeConfiguration_IncludesAzqrCommand()
    {
        // Arrange – ServerRuntimeConfiguration not registered (first DI container (CLI routing) scenario) where all commands
        // are exposed. See: ConfigureServices method in https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/src/Program.cs
        var provider = BuildServiceProvider(startOptions: null);
        var setup = new ExtensionSetup();

        // Act
        var commandGroup = setup.RegisterCommands(provider);

        // Assert
        Assert.Contains("azqr", commandGroup.Commands.Keys);
        Assert.Contains(commandGroup.SubGroup, g => g.Name == "cli");
    }
}
