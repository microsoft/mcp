// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Areas.Group.Commands;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Tests.Helpers;
using Microsoft.Mcp.Core.Models.ResourceGroup;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Group.UnitTests;

public class ResourceGroupListCommandTests : CommandUnitTestsBase<GroupListCommand, IResourceGroupService>
{
    [Fact]
    public async Task ExecuteAsync_WithValidSubscription_ReturnsResourceGroups()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var expectedGroups = new List<ResourceGroupInfo>
        {
            ResourceGroupTestHelpers.CreateResourceGroupInfo("rg1", subscriptionId, "East US"),
            ResourceGroupTestHelpers.CreateResourceGroupInfo("rg2", subscriptionId, "West US")
        };

        Service.GetResourceGroups(
            Arg.Is(subscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedGroups);

        // Act
        var result = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        var resultGroups = ConvertResponse(result, GroupJsonContext.Default.Result);
        Assert.NotNull(resultGroups);
        Assert.Equal(2, resultGroups.Groups.Count);

        var first = resultGroups.Groups[0];
        var second = resultGroups.Groups[1];

        Assert.Equal("rg1", first.Name);
        Assert.Equal("/subscriptions/test-subs-id/resourceGroups/rg1", first.Id);
        Assert.Equal("East US", first.Location);

        Assert.Equal("rg2", second.Name);
        Assert.Equal("/subscriptions/test-subs-id/resourceGroups/rg2", second.Id);
        Assert.Equal("West US", second.Location);

        await Service.Received(1).GetResourceGroups(
            Arg.Is(subscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTenant_PassesTenantToService()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var tenantId = "test-tenant-id";
        var expectedGroups = new List<ResourceGroupInfo>
        {
            ResourceGroupTestHelpers.CreateResourceGroupInfo("rg1", subscriptionId, "East US")
        };

        Service.GetResourceGroups(
            Arg.Is(subscriptionId),
            Arg.Is(tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedGroups);

        // Act
        var result = await ExecuteCommandAsync("--subscription", subscriptionId, "--tenant", tenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        await Service.Received(1).GetResourceGroups(
            Arg.Is(subscriptionId),
            Arg.Is(tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResourceGroupList_ReturnsEmptyResults()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        Service.GetResourceGroups(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        var resultGroups = ConvertResponse(result, GroupJsonContext.Default.Result);
        Assert.NotNull(resultGroups);
        Assert.Empty(resultGroups.Groups);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorInResponse()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var expectedError = "Test error message";
        Service.GetResourceGroups(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var result = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Contains(expectedError, result.Message);
    }
}
