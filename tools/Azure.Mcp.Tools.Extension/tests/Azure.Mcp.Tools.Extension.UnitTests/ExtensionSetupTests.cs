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
    public void RegisterCommands_HttpOboMode_ExcludesAzqrCommand()
    {
        // Arrange
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
        Assert.DoesNotContain("azqr", commandGroup.Commands.Keys);
        // cli subgroup and its commands should still be present
        Assert.Contains(commandGroup.SubGroup, g => g.Name == "cli");
    }

    [Fact]
    public void RegisterCommands_StdioMode_IncludesAzqrCommand()
    {
        // Arrange – stdio transport (default), no OBO
        var options = new ServiceStartOptions
        {
            Transport = TransportTypes.StdIo,
        };
        var provider = BuildServiceProvider(options);
        var setup = new ExtensionSetup();

        // Act
        var commandGroup = setup.RegisterCommands(provider);

        // Assert
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
