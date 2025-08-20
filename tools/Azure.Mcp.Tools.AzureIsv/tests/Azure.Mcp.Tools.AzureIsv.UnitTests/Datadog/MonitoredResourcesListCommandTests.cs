// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.AzureIsv.Commands.Datadog;
using Azure.Mcp.Tools.AzureIsv.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureIsv.UnitTests.Datadog;

public class MonitoredResourcesListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDatadogService _datadogService;
    private readonly ILogger<MonitoredResourcesListCommand> _logger;

    public MonitoredResourcesListCommandTests()
    {
        _datadogService = Substitute.For<IDatadogService>();
        _logger = Substitute.For<ILogger<MonitoredResourcesListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_datadogService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResources_WhenResourcesExist()
    {
        var expectedResources = new List<string>
        {
            "/subscriptions/1234/resourceGroups/rg-demo/providers/Microsoft.Datadog/monitors/app-demo-1",
            "/subscriptions/1234/resourceGroups/rg-demo/providers/Microsoft.Datadog/monitors/vm-demo-2"
        };
        _datadogService.ListMonitoredResources(Arg.Is("rg1"), Arg.Is("sub123"), Arg.Is("datadog1"))
            .Returns(expectedResources);

        var command = new MonitoredResourcesListCommand(_logger);
        var args = command.GetCommand().Parse($"--subscription sub123 --resource-group rg1 --datadog-resource datadog1");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoResources()
    {
        // Arrange
        _datadogService.ListMonitoredResources("rg1", "sub123", "datadog1")
            .Returns(new List<string>());

        var command = new MonitoredResourcesListCommand(_logger);
        var args = command.GetCommand().Parse($"--subscription sub123 --resource-group rg1 --datadog-resource datadog1");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Missing required arguments: datadog-resource";
        _datadogService.ListMonitoredResources("rg1", "sub123", "datadog1")
            .ThrowsAsync(new Exception(expectedError));

        var command = new MonitoredResourcesListCommand(_logger);
        var args = command.GetCommand().Parse($"--subscription sub123 --resource-group rg1 --datadog-resource datadog1");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class MonitoredResourcesListResult
    {
        [JsonPropertyName("resources")]
        public List<string> Resources { get; set; } = new();
    }
}
