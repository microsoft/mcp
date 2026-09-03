// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Container;
using Azure.Mcp.Tools.AzureBackup.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Container;

public class ContainerRefreshCommandTests : SubscriptionCommandUnitTestsBase<ContainerRefreshCommand, IAzureBackupService>
{
    private const string DefaultFilter = "backupManagementType eq 'AzureStorage'";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("refresh", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var options = CommandDefinition.Options;

        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--filter");
    }

    [Fact]
    public async Task ExecuteAsync_TriggersRefresh_WithDefaultFilter_WhenFilterOmitted()
    {
        // Arrange
        Service.RefreshContainersAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is<string?>(DefaultFilter),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.Status);

        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ContainerRefreshCommandResult, HttpStatusCode.Accepted);
        Assert.Equal("Accepted", result.Status);
        Assert.Equal("v", result.Vault);
        Assert.Equal("Azure", result.Fabric);
        Assert.Equal(DefaultFilter, result.Filter);

        await Service.Received(1).RefreshContainersAsync(
            "v", "rg", "sub",
            DefaultFilter,
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_UsesProvidedFilter_WhenFilterSpecified()
    {
        // Arrange
        const string customFilter = "backupManagementType eq 'AzureIaasVM'";
        Service.RefreshContainersAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is<string?>(customFilter),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--filter", customFilter);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.Status);

        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ContainerRefreshCommandResult, HttpStatusCode.Accepted);
        Assert.Equal(customFilter, result.Filter);
    }

    [Theory]
    [InlineData("dpp")]
    [InlineData("DPP")]
    public async Task ExecuteAsync_Rejects_DppVaultType_AtValidation(string vaultType)
    {
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--vault-type", vaultType);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Recovery Services", response.Message);

        await Service.DidNotReceive().RefreshContainersAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg", true)]
    [InlineData("--subscription sub --vault v", false)]     // missing resource-group
    [InlineData("--subscription sub --resource-group rg", false)] // missing vault
    [InlineData("--vault v --resource-group rg", false)]    // missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.RefreshContainersAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
        }

        var response = await ExecuteCommandAsync(args);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.Accepted, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_Maps_Forbidden_To_403()
    {
        Service.RefreshContainersAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status: 403, message: "AuthorizationFailed"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg");

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Backup Contributor", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Maps_NotFound_To_404()
    {
        Service.RefreshContainersAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status: 404, message: "VaultNotFound"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("vault was not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Maps_ArgumentException_To_400()
    {
        Service.RefreshContainersAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Container refresh is only supported for Recovery Services (RSV) vaults."));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Recovery Services", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Maps_GenericException_To_500()
    {
        Service.RefreshContainersAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("boom"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("boom", response.Message);
    }
}
