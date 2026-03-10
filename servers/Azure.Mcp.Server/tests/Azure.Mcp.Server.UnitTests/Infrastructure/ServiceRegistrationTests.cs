// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Commands;
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
}
