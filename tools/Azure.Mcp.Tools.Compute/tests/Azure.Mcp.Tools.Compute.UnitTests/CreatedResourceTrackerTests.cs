// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Mcp.Tools.Compute.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests;

public class CreatedResourceTrackerTests
{
    private readonly ILogger _logger;

    public CreatedResourceTrackerTests()
    {
        _logger = Substitute.For<ILogger>();
    }

    [Fact]
    public void Track_AddsResourceToTracker()
    {
        var tracker = new CreatedResourceTracker(_logger);
        var resourceId = new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/networkSecurityGroups/nsg1");

        tracker.Track(resourceId);

        Assert.True(tracker.HasResources);
    }

    [Fact]
    public void HasResources_ReturnsFalse_WhenNoResourcesTracked()
    {
        var tracker = new CreatedResourceTracker(_logger);

        Assert.False(tracker.HasResources);
    }

    [Fact]
    public void HasResources_ReturnsTrue_AfterTracking()
    {
        var tracker = new CreatedResourceTracker(_logger);
        tracker.Track(new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/networkSecurityGroups/nsg1"));

        Assert.True(tracker.HasResources);
    }

    [Fact]
    public void Track_TracksMultipleResources()
    {
        var tracker = new CreatedResourceTracker(_logger);
        tracker.Track(new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/networkSecurityGroups/nsg1"));
        tracker.Track(new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet1"));
        tracker.Track(new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/publicIPAddresses/pip1"));
        tracker.Track(new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/networkInterfaces/nic1"));

        Assert.True(tracker.HasResources);
    }

    [Fact]
    public async Task RollbackAsync_WithNoResources_CompletesWithoutError()
    {
        var tracker = new CreatedResourceTracker(_logger);
        var armClient = Substitute.For<ArmClient>();

        // Should complete without error even with no resources
        await tracker.RollbackAsync(armClient, CancellationToken.None);

        Assert.False(tracker.HasResources);
    }

    [Fact]
    public async Task RollbackAsync_DeletesTrackedResourcesInReverseOrder()
    {
        var tracker = new CreatedResourceTracker(_logger);
        var nsgId = new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/networkSecurityGroups/nsg1");
        var vnetId = new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet1");
        var pipId = new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/publicIPAddresses/pip1");
        var nicId = new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/networkInterfaces/nic1");

        tracker.Track(nsgId);
        tracker.Track(vnetId);
        tracker.Track(pipId);
        tracker.Track(nicId);

        var deletedIds = new List<ResourceIdentifier>();
        var armClient = Substitute.For<ArmClient>();
        var mockResource = Substitute.For<GenericResource>();
        mockResource.DeleteAsync(WaitUntil.Completed, Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                // We can't easily capture which ID was used per call with this setup,
                // but we verify the method is called the right number of times
                return Substitute.For<ArmOperation>();
            });
        armClient.GetGenericResource(Arg.Any<ResourceIdentifier>()).Returns(mockResource);

        await tracker.RollbackAsync(armClient, CancellationToken.None);

        // Verify all 4 resources had delete called
        armClient.Received(4).GetGenericResource(Arg.Any<ResourceIdentifier>());
        // Verify reverse order: nic, pip, vnet, nsg
        Received.InOrder(() =>
        {
            armClient.GetGenericResource(nicId);
            armClient.GetGenericResource(pipId);
            armClient.GetGenericResource(vnetId);
            armClient.GetGenericResource(nsgId);
        });
    }

    [Fact]
    public async Task RollbackAsync_ContinuesOnDeleteFailure()
    {
        var tracker = new CreatedResourceTracker(_logger);
        var id1 = new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/networkSecurityGroups/nsg1");
        var id2 = new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet1");

        tracker.Track(id1);
        tracker.Track(id2);

        var armClient = Substitute.For<ArmClient>();
        var failingResource = Substitute.For<GenericResource>();
        failingResource.DeleteAsync(WaitUntil.Completed, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("delete failed"));
        var succeedingResource = Substitute.For<GenericResource>();
        succeedingResource.DeleteAsync(WaitUntil.Completed, Arg.Any<CancellationToken>())
            .Returns(Substitute.For<ArmOperation>());

        // id2 is deleted first (reverse order) and fails, id1 should still be attempted
        armClient.GetGenericResource(id2).Returns(failingResource);
        armClient.GetGenericResource(id1).Returns(succeedingResource);

        // Should not throw despite delete failure
        await tracker.RollbackAsync(armClient, CancellationToken.None);

        // Both resources were attempted
        armClient.Received(1).GetGenericResource(id2);
        armClient.Received(1).GetGenericResource(id1);
    }

    [Fact]
    public async Task RollbackAsync_Handles404AsAlreadyDeleted()
    {
        var tracker = new CreatedResourceTracker(_logger);
        var id = new ResourceIdentifier("/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/networkSecurityGroups/nsg1");
        tracker.Track(id);

        var armClient = Substitute.For<ArmClient>();
        var mockResource = Substitute.For<GenericResource>();
        mockResource.DeleteAsync(WaitUntil.Completed, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));
        armClient.GetGenericResource(id).Returns(mockResource);

        // Should complete without error - 404 is treated as already deleted
        await tracker.RollbackAsync(armClient, CancellationToken.None);

        armClient.Received(1).GetGenericResource(id);
    }
}
