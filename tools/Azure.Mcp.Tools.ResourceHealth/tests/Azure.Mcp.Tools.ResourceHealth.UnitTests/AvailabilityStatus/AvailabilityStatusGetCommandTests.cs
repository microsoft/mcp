// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using AvailabilityStatusModel = Azure.Mcp.Tools.ResourceHealth.Models.AvailabilityStatus;

namespace Azure.Mcp.Tools.ResourceHealth.UnitTests.AvailabilityStatus;

public class AvailabilityStatusGetCommandTests : CommandUnitTestsBase<AvailabilityStatusGetCommand, IResourceHealthService>
{
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

        Service.GetAvailabilityStatusAsync(resourceId, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedStatus);

        var response = await ExecuteCommandAsync("--resourceId", resourceId, "--subscription", subscriptionId);

        var result = ValidateAndDeserializeResponse(response, ResourceHealthJsonContext.Default.AvailabilityStatusGetCommandResult);

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

        Service.GetAvailabilityStatusAsync(resourceId, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--resourceId", resourceId, "--subscription", subscriptionId);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenResourceIdIsInvalid()
    {
        var resourceId = "not-a-resource-id";
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var expectedError = "Invalid Azure resource ID. Provide a resource ID in the format /subscriptions/<subscription>/resourceGroups/<resource-group>/providers/<provider>/<type>/<name>. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        Service.GetAvailabilityStatusAsync(resourceId, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new FormatException("Invalid resource ID format."));

        var response = await ExecuteCommandAsync("--resourceId", resourceId, "--subscription", subscriptionId);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsHelpfulError_WhenResourceHealthReturnsUnprocessableEntity()
    {
        var resourceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet";
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var resourceType = "Microsoft.Network/virtualNetworks";
        var errorCode = "UnsupportedResourceType";
        var errorMessage = "Resource type is not supported.";
        var expectedError = $"Azure Resource Health could not process availability status for resource type '{resourceType}'. Error code: {errorCode}. Details: {errorMessage}. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        Service.GetAvailabilityStatusAsync(resourceId, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ResourceHealthUnprocessableEntityException(resourceId, resourceType, errorCode, errorMessage));

        var response = await ExecuteCommandAsync("--resourceId", resourceId, "--subscription", subscriptionId);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
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

        Service.ListAvailabilityStatusesAsync(subscriptionId, null, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedStatuses);

        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        var result = ValidateAndDeserializeResponse(response, ResourceHealthJsonContext.Default.AvailabilityStatusGetCommandResult);

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

        Service.ListAvailabilityStatusesAsync(subscriptionId, resourceGroup, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedStatuses);

        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--resource-group", resourceGroup);

        var result = ValidateAndDeserializeResponse(response, ResourceHealthJsonContext.Default.AvailabilityStatusGetCommandResult);

        Assert.NotNull(result.Statuses);
        Assert.Single(result.Statuses);
        Assert.Contains("test-rg", result.Statuses[0].ResourceId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_WhenListingResources()
    {
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        Service.ListAvailabilityStatusesAsync(subscriptionId, null, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsConflict_WhenResourceHealthListRequestConflicts()
    {
        var subscriptionId = "12345678-1234-1234-1234-123456789012";
        var errorCode = "MissingSubscriptionRegistration";
        var errorMessage = "The subscription is not registered to use namespace 'Microsoft.ResourceHealth'.";
        var expectedError = $"Azure Resource Health returned Conflict. The subscription may need the Microsoft.ResourceHealth provider registered, or the provider may still be registering. Details: {errorMessage}. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        Service.ListAvailabilityStatusesAsync(subscriptionId, null, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ResourceHealthRequestFailedException(HttpStatusCode.Conflict, errorCode, errorMessage));

        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenSubscriptionLookupFails()
    {
        var subscription = "missing-subscription";
        var expectedError = $"Could not find subscription with name {subscription} (Parameter 'subscriptionName'). To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        Service.ListAvailabilityStatusesAsync(subscription, null, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException($"Could not find subscription with name {subscription}", "subscriptionName"));

        var response = await ExecuteCommandAsync("--subscription", subscription);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("--subscription")]
    public async Task ExecuteAsync_ReturnsError_WhenRequiredParameterIsMissing(string missingParameter)
    {
        // The subscription option falls back to the Azure CLI profile or AZURE_SUBSCRIPTION_ID env var.
        // Skip if a CLI profile default is present so the test only runs when
        // the missing-subscription path is actually exercised.
        TestEnvironment.SkipIfDefaultSubscriptionConfigured();

        var argsList = new List<string>();
        if (missingParameter != "--subscription")
        {
            argsList.Add("--subscription");
            argsList.Add("12345678-1234-1234-1234-123456789012");
        }

        var response = await ExecuteCommandAsync(argsList.ToArray());

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }

    #endregion
}
