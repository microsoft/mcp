// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.ResourceManager.Resources;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Metrics;

public class ResourceResolverServiceTests
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly ResourceResolverService _service;

    private readonly SubscriptionResource _subscriptionResource = Substitute.For<SubscriptionResource>();

    public ResourceResolverServiceTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _tenantService = Substitute.For<ITenantService>();
        _service = new ResourceResolverService(_subscriptionService, _tenantService);

        _subscriptionService.GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(_subscriptionResource);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_Succeeds()
    {
        // Act & Assert - Constructor should not throw
        var service = new ResourceResolverService(_subscriptionService, _tenantService);
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullSubscriptionService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ResourceResolverService(null!, _tenantService));
    }

    #endregion

    #region ResolveResourceIdAsync Tests

    [Fact]
    public async Task ResolveResourceIdAsync_WithFullResourceId_ReturnsDirectly()
    {
        // Arrange
        var fullResourceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.Storage/storageAccounts/test";
        var subscription = "87654321-4321-4321-4321-210987654321"; // Different subscription to ensure it's not used

        // Act
        var result = await _service.ResolveResourceIdAsync(subscription, null, null, fullResourceId);

        // Assert
        Assert.Equal(fullResourceId, result.ToString());
        // Verify that subscription service was not called since we're passing a full resource ID
        await _subscriptionService.DidNotReceive().GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ResolveResourceIdAsync_WithResourceGroupAndType_BuildsDirectPath()
    {
        // Arrange
        var subscription = "12345678-1234-1234-1234-123456789012";
        var resourceGroup = "test-rg";
        var resourceType = "Microsoft.Storage/storageAccounts";
        var resourceName = "test";

        var expectedResourceId = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/{resourceType}/{resourceName}";

        // Act
        var result = await _service.ResolveResourceIdAsync(subscription, resourceGroup, resourceType, resourceName);

        // Assert
        Assert.Equal(expectedResourceId, result);
        await _subscriptionService.DidNotReceive().GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>());
    }

    [Theory]
    [InlineData("", "")]
    public async Task ResolveResourceIdAsync_WithNullOrEmptySubscription_ThrowsArgumentException(string? subscription, string? resourceName)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ResolveResourceIdAsync(subscription!, null, null, resourceName!));
    }

    [Theory]
    [InlineData(null, null)]
    public async Task ResolveResourceIdAsync_WithNullOrEmptySubscription_ThrowsArgumentNullException(string? subscription, string? resourceName)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.ResolveResourceIdAsync(subscription!, null, null, resourceName!));
    }

    [Fact]
    public async Task ResolveResourceIdAsync_ResourceDiscovery_NoResourcesFound_ThrowsException()
    {
        // Arrange
        var subscription = "sub1";
        var resourceName = "nonexistent-resource";

        var emptyAsyncPageable = CreateEmptyAsyncPageable();

        _subscriptionResource.GetGenericResourcesAsync(cancellationToken: Arg.Any<CancellationToken>()).Returns(emptyAsyncPageable);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.ResolveResourceIdAsync(subscription, null, null, resourceName));

        Assert.Contains($"Resource '{resourceName}' not found in subscription '{subscription}'", exception.Message);
    }

    [Fact]
    public async Task ResolveResourceIdAsync_ResourceDiscovery_MultipleResourcesFound_ThrowsException()
    {
        // Arrange
        var subscription = Guid.NewGuid().ToString();
        var resourceName = "duplicate-resource";

        var resource1 = CreateMockGenericResource($"/subscriptions/{subscription}/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/{resourceName}", "rg1", "Microsoft.Storage/storageAccounts", resourceName);
        var resource2 = CreateMockGenericResource($"/subscriptions/{subscription}/resourceGroups/rg2/providers/Microsoft.Compute/virtualMachines/{resourceName}", "rg2", "Microsoft.Compute/virtualMachines", resourceName);

        var resourcesAsyncPageable = CreateAsyncPageableWithItems(resource1, resource2);

        _subscriptionResource.GetGenericResourcesAsync(cancellationToken: Arg.Any<CancellationToken>()).Returns(resourcesAsyncPageable);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.ResolveResourceIdAsync(subscription, null, null, resourceName));

        Assert.Contains($"Multiple resources named '{resourceName}' found", exception.Message);
        Assert.Contains("Please specify both resourceGroup and resourceType parameters", exception.Message);
    }

    [Fact]
    public async Task ResolveResourceIdAsync_ResourceDiscovery_SingleResourceFound_ReturnsResourceId()
    {
        // Arrange
        var subscription = Guid.NewGuid().ToString();
        var resourceName = "unique-resource";
        var expectedResourceId = $"/subscriptions/{subscription}/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/{resourceName}";

        var resource = CreateMockGenericResource(expectedResourceId, "rg1", "Microsoft.Storage/storageAccounts", resourceName);

        var subscriptionResource = Substitute.For<SubscriptionResource>();
        var resourcesAsyncPageable = CreateAsyncPageableWithItems(resource);

        subscriptionResource.GetGenericResourcesAsync(cancellationToken: Arg.Any<CancellationToken>()).Returns(resourcesAsyncPageable);
        _subscriptionService.GetSubscription(subscription, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(subscriptionResource);

        // Act
        var result = await _service.ResolveResourceIdAsync(subscription, null, null, resourceName);

        // Assert
        Assert.Equal(expectedResourceId, result.ToString());
    }

    [Fact]
    public async Task ResolveResourceIdAsync_WithResourceGroupFilter_FiltersCorrectly()
    {
        // Arrange
        var subscription = Guid.NewGuid().ToString();
        var resourceGroup = "rg1";
        var resourceName = "filtered-resource";
        var expectedResourceId = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{resourceName}";

        // Create resources in different resource groups with same name
        var resource1 = CreateMockGenericResource(expectedResourceId, "rg1", "Microsoft.Storage/storageAccounts", resourceName);
        var resource2 = CreateMockGenericResource($"/subscriptions/{subscription}/resourceGroups/rg2/providers/Microsoft.Storage/storageAccounts/{resourceName}", "rg2", "Microsoft.Storage/storageAccounts", resourceName);

        var resourcesAsyncPageable = CreateAsyncPageableWithItems(resource1, resource2);

        _subscriptionResource.GetGenericResourcesAsync(cancellationToken: Arg.Any<CancellationToken>()).Returns(resourcesAsyncPageable);

        // Act
        var result = await _service.ResolveResourceIdAsync(subscription, resourceGroup, null, resourceName);

        // Assert
        Assert.Equal(expectedResourceId, result.ToString());
    }

    [Fact]
    public async Task ResolveResourceIdAsync_WithResourceTypeFilter_FiltersCorrectly()
    {
        // Arrange
        var subscription = Guid.NewGuid().ToString();
        var resourceType = "Microsoft.Storage/storageAccounts";
        var resourceName = "filtered-resource";
        var expectedResourceId = $"/subscriptions/{subscription}/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/{resourceName}";

        // Create resources of different types with same name
        var resource1 = CreateMockGenericResource(expectedResourceId, "rg1", "Microsoft.Storage/storageAccounts", resourceName);
        var resource2 = CreateMockGenericResource($"/subscriptions/{subscription}/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/{resourceName}", "rg1", "Microsoft.Compute/virtualMachines", resourceName);

        var resourcesAsyncPageable = CreateAsyncPageableWithItems(resource1, resource2);

        _subscriptionResource.GetGenericResourcesAsync(cancellationToken: Arg.Any<CancellationToken>()).Returns(resourcesAsyncPageable);

        // Act
        var result = await _service.ResolveResourceIdAsync(subscription, null, resourceType, resourceName);

        // Assert
        Assert.Equal(expectedResourceId, result.ToString());
    }

    #endregion

    #region Helper Methods

    private static GenericResource CreateMockGenericResource(string resourceId, string resourceGroupName, string resourceType, string resourceName)
    {
        var result = Substitute.For<GenericResource>();

        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes($@"{{
                ""name"": ""{resourceName}"",
                ""id"": ""{resourceId}"",
                ""type"": ""{resourceType}""
            }}"));

        IJsonModel<GenericResourceData> data = new GenericResourceData(AzureLocation.AustraliaCentral);
        var d = data.Create(ref reader, new ModelReaderWriterOptions("W"));

        result.Data.Returns(d);
        result.Id.Returns(new ResourceIdentifier(resourceId));
        return result;
    }

    private static AsyncPageable<GenericResource> CreateEmptyAsyncPageable()
    {
        return CreateAsyncPageableWithItems();
    }

    private static AsyncPageable<GenericResource> CreateAsyncPageableWithItems(params GenericResource[] items)
    {
        var page = Page<GenericResource>.FromValues(items, continuationToken: null, Substitute.For<Response>());

        return AsyncPageable<GenericResource>.FromPages(new[]
        {
            page
        });
    }

    #endregion
}

// Helper class to create async enumerables for testing
public static class AsyncPageableHelper
{
    public static async IAsyncEnumerable<T> CreateAsyncEnumerable<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }
}
