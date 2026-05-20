// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

public class ServiceInfoCommandTests : CommandUnitTestsBase<ServiceInfoCommand, object>
{
    private readonly McpServerConfiguration _mcpServerConfiguration;

    public ServiceInfoCommandTests()
    {
        _mcpServerConfiguration = new McpServerConfiguration
        {
            Name = "Test-Name?",
            Version = "Test-Version?",
            DisplayName = "Test Display",
            RootCommandGroupName = "azmcp"
        };
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(_mcpServerConfiguration));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCorrectProperties()
    {
        var response = await ExecuteCommandAsync([]);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ServiceInfoJsonContext.Default.ServiceInfoCommandResult);

        Assert.Equal(_mcpServerConfiguration.Name, result.Name);
        Assert.Equal(_mcpServerConfiguration.Version, result.Version);
    }
}
