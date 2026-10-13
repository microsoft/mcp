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

public class GovernanceSoftDeleteCommandTests : SubscriptionCommandUnitTestsBase<GovernanceSoftDeleteCommand, IAzureBackupService>
{
    private static void StubDefault(IAzureBackupService service, AzureBackupSoftDeleteState state = AzureBackupSoftDeleteState.On, int retention = 14)
    {
        service.ConfigureSoftDeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Is(state), Arg.Is(retention),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, "Soft delete configured"));
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("soft-delete", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ConfiguresSoftDelete_Successfully()
    {
        // Arrange
        Service.ConfigureSoftDeleteAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is(AzureBackupSoftDeleteState.AlwaysOn), Arg.Is(90),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, "Soft delete configured"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--soft-delete", "AlwaysOn",
            "--soft-delete-retention-days", "90");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.GovernanceSoftDeleteCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.ConfigureSoftDeleteAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is(AzureBackupSoftDeleteState.On), Arg.Is(14),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--soft-delete", "On",
            "--soft-delete-retention-days", "14");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --soft-delete On --soft-delete-retention-days 14", true)]
    [InlineData("--subscription sub --vault v --resource-group rg --soft-delete On", false)] // retention-days required
    [InlineData("--subscription sub --vault v --resource-group rg --soft-delete-retention-days 30", false)] // soft-delete required
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // both required missing
    [InlineData("--subscription sub", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            StubDefault(Service);
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
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        Service.ConfigureSoftDeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Is(AzureBackupSoftDeleteState.On), Arg.Any<int>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--soft-delete", "On",
            "--soft-delete-retention-days", "14");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        // Arrange
        Service.ConfigureSoftDeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Is(AzureBackupSoftDeleteState.On), Arg.Any<int>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "Cannot change"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--soft-delete", "On",
            "--soft-delete-retention-days", "14");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        Service.ConfigureSoftDeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Is(AzureBackupSoftDeleteState.On), Arg.Any<int>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--soft-delete", "On",
            "--soft-delete-retention-days", "14");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Forbidden", response.Message);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Enable")]
    [InlineData("Disable")]
    [InlineData("always")]
    public async Task ExecuteAsync_RejectsInvalidSoftDeleteState(string softDeleteState)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--soft-delete", softDeleteState,
            "--soft-delete-retention-days", "14");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData("0")]   // below 14
    [InlineData("13")]  // below 14
    [InlineData("181")] // above 180
    [InlineData("abc")] // non-numeric
    public async Task ExecuteAsync_RejectsInvalidRetentionDays(string retentionDays)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--soft-delete", "On",
            "--soft-delete-retention-days", retentionDays);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData(14)]
    [InlineData(90)]
    [InlineData(180)]
    public async Task ExecuteAsync_AcceptsValidRetentionDays(int retentionDays)
    {
        // Arrange
        StubDefault(Service, AzureBackupSoftDeleteState.On, retentionDays);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--soft-delete", "On",
            "--soft-delete-retention-days", retentionDays.ToString());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        StubDefault(Service, AzureBackupSoftDeleteState.On, 30);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--soft-delete", "On",
            "--soft-delete-retention-days", "30");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.GovernanceSoftDeleteCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
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
        Assert.Contains(options, o => o.Name == "--soft-delete");
        Assert.Contains(options, o => o.Name == "--soft-delete-retention-days");
    }
}
