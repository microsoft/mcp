// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
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

    #region Get (Single Resource) Tests

    [Fact]
    public async Task ExecuteAsync_ReturnsAvailabilityStatus_WhenResourceIdProvided()
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

        _resourceHealthService.GetAvailabilityStatusAsync(resourceId, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedStatus);

        var command = new AvailabilityStatusGetCommand(_logger);
        var args = command.GetCommand().Parse(["--resource-id", resourceId, "--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.AvailabilityStatusGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Statuses);
        Assert.Single(result.Statuses);
        Assert.Equal(resourceId, result.Statuses[0].ResourceId);
        Assert.Equal("Available", result.Statuses[0].AvailabilityState);
        Assert.Equal("Resource is healthy", result.Statuses[0].Summary);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_WhenGettingSingleResource()
    {
        var resourceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm";
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        _resourceHealthService.GetAvailabilityStatusAsync(resourceId, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new AvailabilityStatusGetCommand(_logger);

        var args = command.GetCommand().Parse(["--resource-id", resourceId, "--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    #endregion

    #region List (Multiple Resources) Tests

    [Fact]
    public async Task ExecuteAsync_ReturnsAvailabilityStatuses_WhenResourceIdNotProvided()
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

        _resourceHealthService.ListAvailabilityStatusesAsync(subscriptionId, null, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedStatuses);

        var command = new AvailabilityStatusGetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.AvailabilityStatusGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Statuses);
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

        _resourceHealthService.ListAvailabilityStatusesAsync(subscriptionId, resourceGroup, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedStatuses);

        var command = new AvailabilityStatusGetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ResourceHealthJsonContext.Default.AvailabilityStatusGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Statuses);
        Assert.Single(result.Statuses);
        Assert.Contains("test-rg", result.Statuses[0].ResourceId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_WhenListingResources()
    {
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        _resourceHealthService.ListAvailabilityStatusesAsync(subscriptionId, null, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new AvailabilityStatusGetCommand(_logger);

        var args = command.GetCommand().Parse(["--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("--subscription")]
    public async Task ExecuteAsync_ReturnsError_WhenRequiredParameterIsMissing(string missingParameter)
    {
        var command = new AvailabilityStatusGetCommand(_logger);
        var argsList = new List<string>();
        if (missingParameter != "--subscription")
        {
            argsList.Add("--subscription");
            argsList.Add("12345678-1234-1234-1234-123456789012");
        }

        var args = command.GetCommand().Parse([.. argsList]);

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }

    #endregion
}
