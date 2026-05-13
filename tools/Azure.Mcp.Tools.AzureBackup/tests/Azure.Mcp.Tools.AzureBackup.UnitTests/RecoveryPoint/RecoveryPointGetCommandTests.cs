// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.RecoveryPoint;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.RecoveryPoint;

public class RecoveryPointGetCommandTests : CommandUnitTestsBase<RecoveryPointGetCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ListsRecoveryPoints_WhenNoRpSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var protectedItem = "vm1";
        var expectedPoints = new List<RecoveryPointInfo>
        {
            new("rp1", "rp1", "rsv", DateTimeOffset.UtcNow.AddDays(-1), "Full"),
            new("rp2", "rp2", "rsv", DateTimeOffset.UtcNow.AddDays(-2), "Incremental")
        };

        Service.ListRecoveryPointsAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Is(protectedItem),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedPoints);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--vault", vault,
            "--resource-group", resourceGroup,
            "--protected-item", protectedItem);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.RecoveryPointGetCommandResult);

        Assert.Equal(2, result.RecoveryPoints.Count);
        Assert.Equal("rp1", result.RecoveryPoints[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_GetsSingleRecoveryPoint_WhenRpSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var protectedItem = "vm1";
        var rpId = "rp1";

        Service.GetRecoveryPointAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Is(protectedItem),
            Arg.Is(rpId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new RecoveryPointInfo("rp1", rpId, "rsv", DateTimeOffset.UtcNow.AddDays(-1), "Full"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--vault", vault,
            "--resource-group", resourceGroup,
            "--protected-item", protectedItem,
            "--recovery-point", rpId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.RecoveryPointGetCommandResult);

        Assert.Single(result.RecoveryPoints);
        Assert.Equal(rpId, result.RecoveryPoints[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoRecoveryPointsExist()
    {
        // Arrange
        Service.ListRecoveryPointsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("item"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--protected-item", "item");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.RecoveryPointGetCommandResult);

        Assert.Empty(result.RecoveryPoints);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.ListRecoveryPointsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("item"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--protected-item", "item");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        Service.GetRecoveryPointAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("item"), Arg.Is("nonexistent"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Recovery point not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--protected-item", "item",
            "--recovery-point", "nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --protected-item item", true)]
    [InlineData("--subscription sub --vault v --resource-group rg --protected-item item --recovery-point rp1", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // Missing protected-item
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListRecoveryPointsAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("item"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns([]);

            Service.GetRecoveryPointAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("item"), Arg.Is("rp1"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new RecoveryPointInfo("rp1", "rp1", "rsv", DateTimeOffset.UtcNow, "Full"));
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
        Assert.Contains(options, o => o.Name == "--protected-item");
        Assert.Contains(options, o => o.Name == "--container");
        Assert.Contains(options, o => o.Name == "--recovery-point");
    }
}
