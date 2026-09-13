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

public class ContainerListAvailableCommandTests : SubscriptionCommandUnitTestsBase<ContainerListAvailableCommand, IAzureBackupService>
{
    private const string DefaultFilter = "backupManagementType eq 'AzureStorage'";

    [Fact]
    public void CommandMetadataAndOptions_AreDefined()
    {
        Assert.Equal("list-available", CommandDefinition.Name);
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
        Assert.Contains(CommandDefinition.Options, option => option.Name == "--filter");
        Assert.Contains(CommandDefinition.Options, option => option.Name == "--storage-account");
    }

    [Fact]
    public async Task ExecuteAsync_UsesDefaultFilterAndReturnsContainers()
    {
        Service.ListAvailableContainersAsync(
            "v", "rg", "sub", DefaultFilter, null, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns([new ProtectableContainerInfo("container", "storage", "StorageContainer", "AzureStorage", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/storage", "Healthy")]);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--vault", "v", "--resource-group", "rg");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ContainerListAvailableCommandResult);
        Assert.Single(result.Containers);
        Assert.Equal("storage", result.Containers[0].FriendlyName);
        await Service.Received(1).ListAvailableContainersAsync("v", "rg", "sub", DefaultFilter, null, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesAzureStorageFilterAndStorageAccount()
    {
        const string filter = "backupManagementType eq 'AzureStorage'";
        Service.ListAvailableContainersAsync("v", "rg", "sub", filter, "storage", Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns([]);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--filter", filter, "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ContainerListAvailableCommandResult);
        Assert.Empty(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsStorageAccountWithNonStorageFilter()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--filter", "backupManagementType eq 'AzureIaasVM'",
            "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("AzureStorage", response.Message);

        await Service.DidNotReceive().ListAvailableContainersAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MapsServiceError()
    {
        Service.ListAvailableContainersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status: 404, message: "VaultNotFound"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--vault", "v", "--resource-group", "rg");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("VaultNotFound", response.Message);
    }
}
