// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Xunit;

namespace Azure.Mcp.Tools.Extension.UnitTests;

public sealed class ExtensionSetupTests
{
    private static IServiceProvider BuildServiceProvider(ServiceStartOptions? startOptions)
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());

        var setup = new ExtensionSetup();
        setup.ConfigureServices(services);

        if (startOptions is not null)
        {
            services.AddSingleton(startOptions);
        }

        return services.BuildServiceProvider();
    }

    [Fact]
    public void RegisterCommands_RemoteHttpOboMode_ExcludesAzqrCommand()
    {
        // Arrange: HTTP (remote) + OBO auth mode
        var options = new ServiceStartOptions
        {
            Transport = TransportTypes.Http,
            OutgoingAuthStrategy = OutgoingAuthStrategy.UseOnBehalfOf,
        };
        var provider = BuildServiceProvider(options);
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
    public void RegisterCommands_RemoteHttpHostIdentityMode_ExcludesAzqrCommand()
    {
        // Arrange: HTTP (remote) + HostIdentity auth mode
        var options = new ServiceStartOptions
        {
            Transport = TransportTypes.Http,
            OutgoingAuthStrategy = OutgoingAuthStrategy.UseHostingEnvironmentIdentity,
        };
        var provider = BuildServiceProvider(options);
        var setup = new ExtensionSetup();

        // Act
        var commandGroup = setup.RegisterCommands(provider);

        // Assert
        // In remote mode the azqr command that shells out to an external process is excluded.
        Assert.DoesNotContain("azqr", commandGroup.Commands.Keys);
        Assert.Contains(commandGroup.SubGroup, g => g.Name == "cli");
    }

    [Fact]
    public void RegisterCommands_LocalStdioMode_IncludesAzqrCommand()
    {
        // Arrange: stdio transport
        var options = new ServiceStartOptions
        {
            Transport = TransportTypes.StdIo,
        };
        var provider = BuildServiceProvider(options);
        var setup = new ExtensionSetup();

        // Act
        var commandGroup = setup.RegisterCommands(provider);

        // Assert
        // In local mode the azqr command that shells out to an external process is allowed.
        Assert.Contains("azqr", commandGroup.Commands.Keys);
        Assert.Contains(commandGroup.SubGroup, g => g.Name == "cli");
    }

    [Fact]
    public void RegisterCommands_NoServiceStartOptions_IncludesAzqrCommand()
    {
        // Arrange – ServiceStartOptions not registered (first DI container (CLI routing) scenario) where all commands
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
