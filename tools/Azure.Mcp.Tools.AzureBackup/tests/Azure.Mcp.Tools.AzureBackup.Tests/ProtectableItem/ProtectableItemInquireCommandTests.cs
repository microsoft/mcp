// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectableItem;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.ProtectableItem;

public class ProtectableItemInquireCommandTests : SubscriptionCommandUnitTestsBase<ProtectableItemInquireCommand, IAzureBackupService>
{
    private static InquireResult SampleResult() => new(
        Status: "Accepted",
        Container: "StorageContainer;Storage;rg;storage",
        Message: "Inquiry accepted.");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("inquire", CommandDefinition.Name);
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
        Assert.Contains(options, o => o.Name == "--container");
        Assert.Contains(options, o => o.Name == "--storage-account");
    }

    [Fact]
    public async Task ExecuteAsync_Inquires_ByStorageAccount()
    {
        Service.InquireContainerAsync(
            "v", "rg", "sub", null, "storage", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(SampleResult());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.Accepted, response.Status);
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectableItemInquireCommandResult, HttpStatusCode.Accepted);
        Assert.Equal("Accepted", result.Inquiry.Status);

        await Service.Received(1).InquireContainerAsync(
            "v", "rg", "sub", null, "storage", Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Inquires_ByContainer()
    {
        Service.InquireContainerAsync(
            "v", "rg", "sub", "StorageContainer;Storage;rg;storage", null, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(SampleResult());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--container", "StorageContainer;Storage;rg;storage");

        Assert.Equal(HttpStatusCode.Accepted, response.Status);

        await Service.Received(1).InquireContainerAsync(
            "v", "rg", "sub", "StorageContainer;Storage;rg;storage", null, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Rejects_WhenBothContainerAndStorageAccountProvided()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--container", "StorageContainer;Storage;rg;storage",
            "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("mutually exclusive", response.Message);

        await Service.DidNotReceiveWithAnyArgs().InquireContainerAsync(
            default!, default!, default!, default, default, default, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Rejects_WhenNeitherContainerNorStorageAccountProvided()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("either --container or --storage-account", response.Message);

        await Service.DidNotReceiveWithAnyArgs().InquireContainerAsync(
            default!, default!, default!, default, default, default, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Maps_NotFound_To_404()
    {
        Service.InquireContainerAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException("The specified container was not found."));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Maps_Forbidden_To_403()
    {
        Service.InquireContainerAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status: 403, message: "AuthorizationFailed"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Backup Contributor", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Maps_GenericException_To_500()
    {
        Service.InquireContainerAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("boom"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--storage-account", "storage");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("boom", response.Message);
    }
}
