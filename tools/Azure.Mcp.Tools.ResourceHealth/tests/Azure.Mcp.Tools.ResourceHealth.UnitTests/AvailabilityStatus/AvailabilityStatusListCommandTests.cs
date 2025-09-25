// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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

public class AvailabilityStatusListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IResourceHealthService _resourceHealthService;
    private readonly ILogger<AvailabilityStatusListCommand> _logger;

    public AvailabilityStatusListCommandTests()
    {
        _resourceHealthService = Substitute.For<IResourceHealthService>();
        _logger = Substitute.For<ILogger<AvailabilityStatusListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_resourceHealthService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAvailabilityStatuses_WhenResourcesExist()
    {
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var expectedStatuses = new List<AvailabilityStatusModel>
        {
            new()
            {
                ResourceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1",
                AvailabilityState = "Available",
                Summary = "Resource is healthy",
                DetailedStatus = "Virtual machine is running normally"
            },
            new()
            {
                ResourceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg2/providers/Microsoft.Storage/storageAccounts/storage1",
                AvailabilityState = "Available",
                Summary = "Resource is healthy",
                DetailedStatus = "Storage account is accessible"
            }
        };

        _resourceHealthService.ListAvailabilityStatusesAsync(subscriptionId, null, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedStatuses);

        var command = new AvailabilityStatusListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.AvailabilityStatusListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.Statuses.Count);
        Assert.Equal("Available", result.Statuses[0].AvailabilityState);
        Assert.Equal("Available", result.Statuses[1].AvailabilityState);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFilteredAvailabilityStatuses_WhenResourceGroupProvided()
    {
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var resourceGroup = "test-rg";
        var expectedStatuses = new List<AvailabilityStatusModel>
        {
            new()
            {
                ResourceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/vm1",
                AvailabilityState = "Available",
                Summary = "Resource is healthy",
                DetailedStatus = "Virtual machine is running normally"
            }
        };

        _resourceHealthService.ListAvailabilityStatusesAsync(subscriptionId, resourceGroup, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedStatuses);

        var command = new AvailabilityStatusListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.AvailabilityStatusListCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Statuses);
        Assert.Contains("test-rg", result.Statuses[0].ResourceId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        _resourceHealthService.ListAvailabilityStatusesAsync(subscriptionId, null, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new AvailabilityStatusListCommand(_logger);

        var args = command.GetCommand().Parse(["--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription")]
    public async Task ExecuteAsync_ReturnsError_WhenRequiredParameterIsMissing(string missingParameter)
    {
        var command = new AvailabilityStatusListCommand(_logger);
        var argsList = new List<string>();
        if (missingParameter != "--subscription")
        {
            argsList.Add("--subscription");
            argsList.Add("12345678-1234-1234-1234-123456789012");
        }

        var args = command.GetCommand().Parse([.. argsList]);

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }
}
