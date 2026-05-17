// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Commands.ServerInstructions;
using Xunit;

namespace Azure.Mcp.Server.UnitTests.Infrastructure;

public class ServiceRegistrationTests
{
    [Fact]
    public void ConfigureServices_ConfiguresServerInstructions()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        Program.ConfigureServices(services);

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IServerInstructionsProvider? instructionsProvider = provider.GetService<IServerInstructionsProvider>();

        // Verify server instructions are configured
        Assert.NotNull(instructionsProvider);

        // Output the actual content for debugging
        string? instructions = instructionsProvider?.GetServerInstructions();

        // Verify the instructions contain expected sections
        Assert.Contains("Azure MCP server usage rules:", instructions);
        Assert.Contains("Use Azure Code Gen Best Practices:", instructions);
        Assert.Contains("Use Azure AI App Code Generation Best Practices", instructions);
    }

    [Fact]
    public void ConfigureServices_WithNonServerAreaFilter_RegistersNullServerInstructionsProvider()
    {
        // Arrange
        ServiceCollection services = new();

        // Act – a non-server area filter (CLI targeted path)
        Program.ConfigureServices(services, "storage");

        // Assert – server-mode providers should be lightweight null stubs to avoid
        // loading embedded resources on every CLI invocation
        ServiceProvider provider = services.BuildServiceProvider();
        IServerInstructionsProvider? instructionsProvider = provider.GetService<IServerInstructionsProvider>();

        Assert.NotNull(instructionsProvider);
        Assert.IsType<NullServerInstructionsProvider>(instructionsProvider);
        Assert.Null(instructionsProvider.GetServerInstructions());
    }

    [Fact]
    public void ConfigureServices_WithNullAreaFilter_RegistersRealServerInstructionsProvider()
    {
        // Arrange
        ServiceCollection services = new();

        // Act – null filter = full init; real server-mode providers must be registered
        Program.ConfigureServices(services);

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IServerInstructionsProvider? instructionsProvider = provider.GetService<IServerInstructionsProvider>();

        Assert.NotNull(instructionsProvider);
        Assert.IsNotType<NullServerInstructionsProvider>(instructionsProvider);
        Assert.NotNull(instructionsProvider?.GetServerInstructions());
    }
}
