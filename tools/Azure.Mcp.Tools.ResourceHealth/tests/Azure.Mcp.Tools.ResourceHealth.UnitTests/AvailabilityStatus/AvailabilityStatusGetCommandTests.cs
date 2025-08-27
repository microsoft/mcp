// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using AvailabilityStatusModel = Azure.Mcp.Tools.ResourceHealth.Models.AvailabilityStatus;

namespace Azure.Mcp.Tools.ResourceHealth.UnitTests.AvailabilityStatus;

public class AvailabilityStatusGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IResourceHealthService _resourceHealthService;
    private readonly ILogger<AvailabilityStatusGetCommand> _logger;

    public AvailabilityStatusGetCommandTests()
    {
        _resourceHealthService = Substitute.For<IResourceHealthService>();
        _logger = Substitute.For<ILogger<AvailabilityStatusGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_resourceHealthService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAvailabilityStatus_WhenResourceExists()
    {
        var resourceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm";
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var expectedStatus = new AvailabilityStatusModel
        {
            ResourceId = resourceId,
            AvailabilityState = "Available",
            Summary = "Resource is healthy",
            DetailedStatus = "Virtual machine is running normally"
        };

        _resourceHealthService.GetAvailabilityStatusAsync(resourceId, Arg.Any<RetryPolicyOptions>())
            .Returns(expectedStatus);

        var command = new AvailabilityStatusGetCommand(_logger);
        var args = command.GetCommand().Parse(["--resourceId", resourceId, "--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<AvailabilityStatusGetResult>(json, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Equal(resourceId, result.Status.ResourceId);
        Assert.Equal("Available", result.Status.AvailabilityState);
        Assert.Equal("Resource is healthy", result.Status.Summary);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var resourceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm";
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        _resourceHealthService.GetAvailabilityStatusAsync(resourceId, Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new AvailabilityStatusGetCommand(_logger);

        var args = command.GetCommand().Parse(["--resourceId", resourceId, "--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--resourceId")]
    [InlineData("--subscription")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var command = new AvailabilityStatusGetCommand(_logger);
        var argsList = new List<string>();
        if (missingParameter != "--subscription")
        {
            argsList.Add("--subscription");
            argsList.Add("12345678-1234-1234-1234-123456789012");
        }

        if (missingParameter != "--resourceId")
        {
            argsList.Add("--resourceId");
            argsList.Add("/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm");
        }

        var args = command.GetCommand().Parse([.. argsList]);

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }

    private record AvailabilityStatusGetResult(AvailabilityStatusModel Status);
}
