// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Areas.Group.Commands;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Microsoft.Mcp.Core.Models.Resource;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Group.UnitTests;

public class ResourceListCommandTests : CommandUnitTestsBase<ResourceListCommand, IResourceGroupService>
{
    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsResources()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var resourceGroup = "test-rg";
        var expectedResources = new List<GenericResourceInfo>
        {
            new("storageAccount1", "/subscriptions/test-subs-id/resourceGroups/test-rg/providers/Microsoft.Storage/storageAccounts/storageAccount1", "Microsoft.Storage/storageAccounts", "East US"),
            new("vm1", "/subscriptions/test-subs-id/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/vm1", "Microsoft.Compute/virtualMachines", "West US")
        };

        Service.GetGenericResources(
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResources.ToAsyncEnumerable());

        // Act
        var result = await ExecuteCommandAsync("--subscription", subscriptionId, "--resource-group", resourceGroup);

        // Assert
        var listResult = ValidateAndDeserializeResponse(result, GroupJsonContext.Default.ResourceListCommandResult);
        Assert.Equal(2, listResult.Resources.Count);

        Assert.Equal("storageAccount1", listResult.Resources[0].Name);
        Assert.Equal("Microsoft.Storage/storageAccounts", listResult.Resources[0].Type);
        Assert.Equal("East US", listResult.Resources[0].Location);

        Assert.Equal("vm1", listResult.Resources[1].Name);
        Assert.Equal("Microsoft.Compute/virtualMachines", listResult.Resources[1].Type);
        Assert.Equal("West US", listResult.Resources[1].Location);

        Service.Received(1).GetGenericResources(
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTenant_PassesTenantToService()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var resourceGroup = "test-rg";
        var tenantId = "test-tenant-id";
        var expectedResources = new List<GenericResourceInfo>
        {
            new("resource1", "/subscriptions/test-subs-id/resourceGroups/test-rg/providers/Microsoft.Storage/storageAccounts/resource1", "Microsoft.Storage/storageAccounts", "East US")
        };

        Service.GetGenericResources(
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Is(tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResources.ToAsyncEnumerable());

        // Act
        var result = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--tenant", tenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Service.Received(1).GetGenericResources(
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Is(tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResourceList_ReturnsEmptyResults()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var resourceGroup = "test-rg";
        Service.GetGenericResources(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<GenericResourceInfo>());

        // Act
        var result = await ExecuteCommandAsync("--subscription", subscriptionId, "--resource-group", resourceGroup);

        // Assert
        var listResult = ValidateAndDeserializeResponse(result, GroupJsonContext.Default.ResourceListCommandResult);
        Assert.Empty(listResult.Resources);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorInResponse()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var resourceGroup = "test-rg";
        var expectedError = "Test error message";
        Service.GetGenericResources(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Throws(new Exception(expectedError));

        // Act
        var result = await ExecuteCommandAsync("--subscription", subscriptionId, "--resource-group", resourceGroup);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Contains(expectedError, result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceGroup_ReturnsValidationError()
    {
        // Arrange
        var subscriptionId = "test-subs-id";

        // Act
        var result = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(HttpStatusCode.OK, result.Status);
    }
}
