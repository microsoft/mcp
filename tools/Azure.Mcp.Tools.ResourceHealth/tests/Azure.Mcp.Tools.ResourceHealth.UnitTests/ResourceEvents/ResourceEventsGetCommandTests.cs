// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResourceHealth.Commands.ResourceEvents;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ResourceHealth.Tests.UnitTests.ResourceEvents;

[TestClass]
public class ResourceEventsGetCommandTests : BaseResourceHealthCommandTestsFixture<ResourceEventsGetCommand>
{
    protected override ResourceEventsGetCommand CreateCommand()
    {
        return new ResourceEventsGetCommand(CreateMockLogger<ResourceEventsGetCommand>());
    }

    [TestMethod]
    public void Constructor_WithValidLogger_SetsProperties()
    {
        // Arrange
        var logger = CreateMockLogger<ResourceEventsGetCommand>();

        // Act
        var command = new ResourceEventsGetCommand(logger);

        // Assert
        Assert.IsNotNull(command);
        Assert.AreEqual("get", command.Name);
        Assert.AreEqual("Get Resource Events", command.Title);
        Assert.IsTrue(command.Description.Contains("historical availability events"));
        Assert.IsFalse(command.Metadata.Destructive);
        Assert.IsTrue(command.Metadata.ReadOnly);
    }

    [TestMethod]
    public async Task ExecuteAsync_WithValidResourceId_ReturnsResourceEvents()
    {
        // Arrange
        var command = CreateCommand();
        var args = "--resource-id /subscriptions/12345/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1";

        // Act
        var result = await CallCommand(command, args);

        // Assert
        Assert.AreEqual(200, result.Status);
        Assert.IsNotNull(result.Results);
    }

    [TestMethod]
    public async Task ExecuteAsync_WithTimeRange_ReturnsFilteredEvents()
    {
        // Arrange
        var command = CreateCommand();
        var args = "--resource-id /subscriptions/12345/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1 " +
                  "--query-start-time 2024-01-01T00:00:00Z --query-end-time 2024-01-31T23:59:59Z";

        // Act
        var result = await CallCommand(command, args);

        // Assert
        Assert.AreEqual(200, result.Status);
        Assert.IsNotNull(result.Results);
    }

    [TestMethod]
    public async Task ExecuteAsync_WithMissingResourceId_ReturnsBadRequest()
    {
        // Arrange
        var command = CreateCommand();
        var args = "--query-start-time 2024-01-01T00:00:00Z";

        // Act
        var result = await CallCommand(command, args);

        // Assert
        Assert.AreEqual(400, result.Status);
        Assert.IsTrue(result.Message.Contains("Resource ID is required"));
    }

    [TestMethod]
    public async Task ExecuteAsync_WithInvalidDateFormat_ReturnsBadRequest()
    {
        // Arrange
        var command = CreateCommand();
        var args = "--resource-id /subscriptions/12345/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1 " +
                  "--query-start-time invalid-date";

        // Act
        var result = await CallCommand(command, args);

        // Assert
        Assert.AreEqual(400, result.Status);
        Assert.IsTrue(result.Message.Contains("Invalid query start time format"));
    }

    [TestMethod]
    public async Task ExecuteAsync_WithStartTimeAfterEndTime_ReturnsBadRequest()
    {
        // Arrange
        var command = CreateCommand();
        var args = "--resource-id /subscriptions/12345/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1 " +
                  "--query-start-time 2024-01-31T23:59:59Z --query-end-time 2024-01-01T00:00:00Z";

        // Act
        var result = await CallCommand(command, args);

        // Assert
        Assert.AreEqual(400, result.Status);
        Assert.IsTrue(result.Message.Contains("Query start time must be before query end time"));
    }

    [TestMethod]
    public async Task ExecuteAsync_WithCustomFilter_ReturnsFilteredEvents()
    {
        // Arrange
        var command = CreateCommand();
        var args = "--resource-id /subscriptions/12345/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1 " +
                  "--filter \"availabilityState eq 'Unavailable'\"";

        // Act
        var result = await CallCommand(command, args);

        // Assert
        Assert.AreEqual(200, result.Status);
        Assert.IsNotNull(result.Results);
    }
}
