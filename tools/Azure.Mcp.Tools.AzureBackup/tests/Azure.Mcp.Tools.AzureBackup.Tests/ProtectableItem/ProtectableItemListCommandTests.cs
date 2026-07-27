// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectableItem;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.ProtectableItem;

public class ProtectableItemListCommandTests : SubscriptionCommandUnitTestsBase<ProtectableItemListCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ListsProtectableItems_Successfully()
    {
        // Arrange
        var expectedItems = new List<ProtectableItemInfo>
        {
            new("id1", "db1", "SQLDataBase", "SQL", "MyDatabase", "server1", "instance1", "NotProtected", "container1"),
            new("id2", "db2", "SAPHanaDatabase", "SAPHana", "HanaDb", "server2", "instance2", "NotProtected", "container2")
        };

        Service.ListProtectableItemsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedItems);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectableItemListCommandResult);

        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoItemsExist()
    {
        // Arrange
        Service.ListProtectableItemsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectableItemListCommandResult);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.ListProtectableItemsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg", true)]
    [InlineData("--subscription sub", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListProtectableItemsAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var options = CommandDefinition.Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--workload-type");
        Assert.Contains(options, o => o.Name == "--container");
    }

    // NEW-4: --workload-type must be rejected at the command boundary with a 400-class
    // validation error rather than leaking the inner ArgumentException from the service
    // layer as a 500. The validator's accepted set must mirror the service-layer guard.

    [Theory]
    [InlineData("SQL")]
    [InlineData("sql")]                 // case-insensitive
    [InlineData("SQLDatabase")]         // alias
    [InlineData("SQLInstance")]
    [InlineData("SAPHana")]
    [InlineData("SAPHanaDatabase")]
    [InlineData("SAPHanaSystem")]
    [InlineData("SAPHanaDBInstance")]
    [InlineData("SAPHanaDBI")]          // alias
    [InlineData("VM")]
    [InlineData("IaaSVM")]              // alias
    [InlineData("VirtualMachine")]      // alias
    [InlineData("FileShare")]
    [InlineData("AzureFileShare")]      // alias
    [InlineData("AFS")]                 // alias
    [InlineData("SAPAse")]
    [InlineData("SAPAseDatabase")]      // alias
    [InlineData("ASE")]                 // alias
    [InlineData("Sybase")]              // alias
    public async Task ExecuteAsync_AcceptsKnownWorkloadType(string workloadType)
    {
        // Arrange
        Service.ListProtectableItemsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is<string?>(workloadType), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--workload-type", workloadType);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("Cosmos")]              // not an RSV workload
    [InlineData("AzureDisk")]           // DPP-only, not an RSV protectable item
    [InlineData("garbage")]
    [InlineData("'); DROP TABLE--")]    // OData-injection style input
    [InlineData(" ")]                   // whitespace must not bypass validation
    [InlineData("\t  ")]                // tabs + spaces -- still whitespace
    public async Task ExecuteAsync_RejectsUnknownWorkloadType_AsValidationError(string workloadType)
    {
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--workload-type", workloadType);

        // Assert: validation error (400), not service-layer 500
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Unknown workload type", response.Message);
        Assert.Contains(workloadType, response.Message);

        // And the service is never invoked once validation has rejected the input.
        await Service.DidNotReceive().ListProtectableItemsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
