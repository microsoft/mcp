// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Container;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Container;

public class ContainerRegisterCommandTests : SubscriptionCommandUnitTestsBase<ContainerRegisterCommand, IAzureBackupService>
{
    private static ContainerRegisterResult SampleResult(bool alreadyRegistered = false) => new(
        Status: "Registered",
        Container: new RegisteredContainerInfo(
            Name: "StorageContainer;Storage;rg;storage",
            FriendlyName: "storage",
            BackupManagementType: "AzureStorage",
            RegistrationStatus: "Registered",
            HealthStatus: "Healthy",
            SourceResourceId: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/storage"),
        AlreadyRegistered: alreadyRegistered,
        Message: "done");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("register", CommandDefinition.Name);
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
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
        Assert.Contains(options, o => o.Name == "--storage-account");
        Assert.Contains(options, o => o.Name == "--acquire-lock");
    }

    [Fact]
    public async Task ExecuteAsync_Registers_WithLockAcquiredByDefault()
    {
        Service.RegisterContainerAsync(
            "v", "rg", "sub", "storage", true, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(SampleResult());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ContainerRegisterCommandResult);
        Assert.Equal("Registered", result.Registration.Status);
        Assert.False(result.Registration.AlreadyRegistered);
        Assert.Equal("storage", result.Registration.Container.FriendlyName);

        await Service.Received(1).RegisterContainerAsync(
            "v", "rg", "sub", "storage", true, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesAcquireLockFalse_WhenSpecified()
    {
        Service.RegisterContainerAsync(
            "v", "rg", "sub", "storage", false, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(SampleResult());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--storage-account", "storage",
            "--acquire-lock", "false");

        Assert.Equal(HttpStatusCode.OK, response.Status);

        await Service.Received(1).RegisterContainerAsync(
            "v", "rg", "sub", "storage", false, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAlreadyRegistered_WhenIdempotent()
    {
        Service.RegisterContainerAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(SampleResult(alreadyRegistered: true));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ContainerRegisterCommandResult);
        Assert.True(result.Registration.AlreadyRegistered);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --storage-account storage", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)]     // missing storage-account
    [InlineData("--subscription sub --vault v --storage-account storage", false)] // missing resource-group
    [InlineData("--subscription sub --resource-group rg --storage-account storage", false)] // missing vault
    [InlineData("--vault v --resource-group rg --storage-account storage", false)] // missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.RegisterContainerAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(SampleResult());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_Maps_Forbidden_To_403()
    {
        Service.RegisterContainerAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status: 403, message: "AuthorizationFailed"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Backup Contributor", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Maps_Conflict_To_409()
    {
        Service.RegisterContainerAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status: 409, message: "Conflict"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already registered", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Maps_NotFound_To_404()
    {
        Service.RegisterContainerAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status: 404, message: "NotFound"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Maps_ArgumentException_To_400()
    {
        Service.RegisterContainerAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Container registration is only supported for Recovery Services (RSV) vaults."));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Recovery Services", response.Message);
    }
}
