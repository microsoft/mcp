// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Governance;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Governance;

public class GovernanceImmutabilityCommandTests : SubscriptionCommandUnitTestsBase<GovernanceImmutabilityCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("immutability", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ConfiguresImmutability_Successfully()
    {
        // Arrange
        Service.ConfigureImmutabilityAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is(AzureBackupImmutabilityState.Unlocked),
            Arg.Is(AzureBackupImmutabilityType.AsPerPolicy),
            Arg.Any<int?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, "Immutability configured"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--immutability-state", "Unlocked",
            "--immutability-type", "AsPerPolicy");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.GovernanceImmutabilityCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.ConfigureImmutabilityAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is(AzureBackupImmutabilityState.Unlocked),
            Arg.Is(AzureBackupImmutabilityType.AsPerPolicy),
            Arg.Any<int?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--immutability-state", "Unlocked",
            "--immutability-type", "AsPerPolicy");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --immutability-state Unlocked --immutability-type AsPerPolicy", true)]
    [InlineData("--subscription sub --vault v --resource-group rg --immutability-state Unlocked", false)] // missing --immutability-type
    [InlineData("--subscription sub --vault v --resource-group rg --immutability-type AsPerPolicy", false)] // missing --immutability-state
    [InlineData("--subscription sub --vault v --resource-group rg --immutability-state Unlocked --immutability-type TimeBased", false)] // TimeBased requires duration
    [InlineData("--subscription sub --vault v --resource-group rg --immutability-state Unlocked --immutability-type TimeBased --immutability-duration-days 5", false)] // duration below 30
    [InlineData("--subscription sub --vault v --resource-group rg --immutability-state Unlocked --immutability-type TimeBased --immutability-duration-days 90", true)]
    [InlineData("--subscription sub --vault v --resource-group rg --immutability-state Disabled --immutability-type TimeBased", true)] // Disabled ignores duration
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // Missing both required
    [InlineData("--subscription sub", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ConfigureImmutabilityAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<AzureBackupImmutabilityState>(),
                Arg.Any<AzureBackupImmutabilityType>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(new OperationResult("Succeeded", null, null));
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
        Assert.Contains(options, o => o.Name == "--immutability-state");
        Assert.Contains(options, o => o.Name == "--immutability-type");
        Assert.Contains(options, o => o.Name == "--immutability-duration-days");
    }
}
