// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.ProtectedItem;

public class ProtectedItemProtectCommandTests : CommandUnitTestsBase<ProtectedItemProtectCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("protect", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ProtectsItem_Successfully()
    {
        // Arrange
        Service.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vm1"), Arg.Is("DefaultPolicy"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ProtectResult("Succeeded", "vm1-backup", "job123", "Protection enabled"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vm1",
            "--policy", "DefaultPolicy");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vm1"), Arg.Is("DefaultPolicy"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vm1",
            "--policy", "DefaultPolicy");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --datasource-id ds1 --policy pol1", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // Missing datasource-id and policy
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ProtectItemAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"), Arg.Is("pol1"),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new ProtectResult("Succeeded", "item1", "job1", null));
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
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        Service.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vm1"), Arg.Is("DefaultPolicy"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vm1",
            "--policy", "DefaultPolicy");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("Backup Contributor", response.Message);
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
        Assert.Contains(options, o => o.Name == "--datasource-id");
        Assert.Contains(options, o => o.Name == "--policy");
        Assert.Contains(options, o => o.Name == "--container");
        Assert.Contains(options, o => o.Name == "--datasource-type");
    }
}
