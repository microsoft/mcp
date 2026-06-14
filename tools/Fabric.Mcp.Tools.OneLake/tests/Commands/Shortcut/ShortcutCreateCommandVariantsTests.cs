// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.OneLake.Commands.Shortcut;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands.Shortcut;

public class ShortcutCreateCommandVariantsTests
{
    [Fact]
    public async Task ExecuteAsync_OneLakeCommand_MapsTargetValues()
    {
        var service = CreateShortcutService();
        var command = new ShortcutCreateOneLakeCommand(Substitute.For<ILogger<ShortcutCreateOneLakeCommand>>(), service);

        var response = await ExecuteAsync(command,
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcut-path", "Files/landing",
            "--shortcut-name", "shortcut1",
            "--target-workspace-id", "target-ws",
            "--target-item-id", "target-item",
            "--target-path", "Files/data");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).CreateShortcutAsync(
            "ws1",
            "item1",
            Arg.Is<OneLakeShortcut>(shortcut =>
                shortcut.Path == "Files/landing" &&
                shortcut.Name == "shortcut1" &&
                shortcut.Target.OneLake!.WorkspaceId == "target-ws" &&
                shortcut.Target.OneLake.ItemId == "target-item" &&
                shortcut.Target.OneLake.Path == "Files/data"),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AdlsGen2Command_MapsTargetValues()
    {
        var service = CreateShortcutService();
        var command = new ShortcutCreateAdlsGen2Command(Substitute.For<ILogger<ShortcutCreateAdlsGen2Command>>(), service);

        var response = await ExecuteAsync(command,
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcut-path", "Files/landing",
            "--shortcut-name", "shortcut1",
            "--target-location", "https://account.dfs.core.windows.net/container",
            "--target-subpath", "/folder",
            "--target-connection-id", "connection-1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).CreateShortcutAsync(
            "ws1",
            "item1",
            Arg.Is<OneLakeShortcut>(shortcut =>
                shortcut.Target.AdlsGen2!.Location == "https://account.dfs.core.windows.net/container" &&
                shortcut.Target.AdlsGen2.Subpath == "/folder" &&
                shortcut.Target.AdlsGen2.ConnectionId == "connection-1"),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AmazonS3Command_MapsTargetValues()
    {
        var service = CreateShortcutService();
        var command = new ShortcutCreateAmazonS3Command(Substitute.For<ILogger<ShortcutCreateAmazonS3Command>>(), service);

        var response = await ExecuteAsync(command,
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcut-path", "Files/landing",
            "--shortcut-name", "shortcut1",
            "--target-location", "https://bucket.s3.us-west-2.amazonaws.com",
            "--target-subpath", "/folder",
            "--target-connection-id", "connection-1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).CreateShortcutAsync(
            "ws1",
            "item1",
            Arg.Is<OneLakeShortcut>(shortcut =>
                shortcut.Target.AmazonS3!.Location == "https://bucket.s3.us-west-2.amazonaws.com" &&
                shortcut.Target.AmazonS3.Subpath == "/folder" &&
                shortcut.Target.AmazonS3.ConnectionId == "connection-1"),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AzureBlobCommand_MapsTargetValues()
    {
        var service = CreateShortcutService();
        var command = new ShortcutCreateAzureBlobCommand(Substitute.For<ILogger<ShortcutCreateAzureBlobCommand>>(), service);

        var response = await ExecuteAsync(command,
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcut-path", "Files/landing",
            "--shortcut-name", "shortcut1",
            "--target-location", "https://account.blob.core.windows.net/container",
            "--target-subpath", "/folder",
            "--target-connection-id", "connection-1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).CreateShortcutAsync(
            "ws1",
            "item1",
            Arg.Is<OneLakeShortcut>(shortcut =>
                shortcut.Target.AzureBlobStorage!.Location == "https://account.blob.core.windows.net/container" &&
                shortcut.Target.AzureBlobStorage.Subpath == "/folder" &&
                shortcut.Target.AzureBlobStorage.ConnectionId == "connection-1"),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_GcsCommand_MapsTargetValues()
    {
        var service = CreateShortcutService();
        var command = new ShortcutCreateGcsCommand(Substitute.For<ILogger<ShortcutCreateGcsCommand>>(), service);

        var response = await ExecuteAsync(command,
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcut-path", "Files/landing",
            "--shortcut-name", "shortcut1",
            "--target-location", "https://bucket.storage.googleapis.com",
            "--target-subpath", "/folder",
            "--target-connection-id", "connection-1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).CreateShortcutAsync(
            "ws1",
            "item1",
            Arg.Is<OneLakeShortcut>(shortcut =>
                shortcut.Target.GoogleCloudStorage!.Location == "https://bucket.storage.googleapis.com" &&
                shortcut.Target.GoogleCloudStorage.Subpath == "/folder" &&
                shortcut.Target.GoogleCloudStorage.ConnectionId == "connection-1"),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_S3CompatibleCommand_MapsBucketValue()
    {
        var service = CreateShortcutService();
        var command = new ShortcutCreateS3CompatibleCommand(Substitute.For<ILogger<ShortcutCreateS3CompatibleCommand>>(), service);

        var response = await ExecuteAsync(command,
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcut-path", "Files/landing",
            "--shortcut-name", "shortcut1",
            "--target-location", "https://s3endpoint.contoso.com",
            "--target-subpath", "/folder",
            "--target-connection-id", "connection-1",
            "--target-bucket", "bucket-1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).CreateShortcutAsync(
            "ws1",
            "item1",
            Arg.Is<OneLakeShortcut>(shortcut =>
                shortcut.Target.S3Compatible!.Location == "https://s3endpoint.contoso.com" &&
                shortcut.Target.S3Compatible.Subpath == "/folder" &&
                shortcut.Target.S3Compatible.ConnectionId == "connection-1" &&
                shortcut.Target.S3Compatible.Bucket == "bucket-1"),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DataverseCommand_MapsTargetValues()
    {
        var service = CreateShortcutService();
        var command = new ShortcutCreateDataverseCommand(Substitute.For<ILogger<ShortcutCreateDataverseCommand>>(), service);

        var response = await ExecuteAsync(command,
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcut-path", "Files/landing",
            "--shortcut-name", "shortcut1",
            "--target-environment-domain", "https://org.crm.dynamics.com",
            "--target-connection-id", "connection-1",
            "--target-deltalake-folder", "Tables/account",
            "--target-table-name", "account");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).CreateShortcutAsync(
            "ws1",
            "item1",
            Arg.Is<OneLakeShortcut>(shortcut =>
                shortcut.Target.Dataverse!.EnvironmentDomain == "https://org.crm.dynamics.com" &&
                shortcut.Target.Dataverse.ConnectionId == "connection-1" &&
                shortcut.Target.Dataverse.DeltaLakeFolder == "Tables/account"),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_OneDriveSharePointCommand_MapsSensitivityFlag()
    {
        var service = CreateShortcutService();
        var command = new ShortcutCreateOneDriveSharePointCommand(Substitute.For<ILogger<ShortcutCreateOneDriveSharePointCommand>>(), service);

        var response = await ExecuteAsync(command,
            "--workspace-id", "ws1",
            "--item-id", "item1",
            "--shortcut-path", "Files/landing",
            "--shortcut-name", "shortcut1",
            "--target-location", "https://contoso.sharepoint.com/sites/site",
            "--target-subpath", "/Documents",
            "--target-connection-id", "connection-1",
            "--target-update-fabric-item-sensitivity", "true");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).CreateShortcutAsync(
            "ws1",
            "item1",
            Arg.Is<OneLakeShortcut>(shortcut =>
                shortcut.Target.OneDriveSharePoint!.Location == "https://contoso.sharepoint.com/sites/site" &&
                shortcut.Target.OneDriveSharePoint.Subpath == "/Documents" &&
                shortcut.Target.OneDriveSharePoint.ConnectionId == "connection-1" &&
                shortcut.Target.OneDriveSharePoint.UpdateFabricItemSensitivity == true),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MissingRequiredArguments_ReturnsBadRequest()
    {
        var command = new ShortcutCreateOneLakeCommand(
            Substitute.For<ILogger<ShortcutCreateOneLakeCommand>>(),
            CreateShortcutService());

        var response = await ExecuteAsync(command,
            "--item-id", "item1",
            "--shortcut-path", "Files/landing",
            "--shortcut-name", "shortcut1",
            "--target-workspace-id", "target-ws",
            "--target-item-id", "target-item");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    private static IOneLakeService CreateShortcutService()
    {
        var service = Substitute.For<IOneLakeService>();
        service.CreateShortcutAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<OneLakeShortcut>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<OneLakeShortcut>(2));
        return service;
    }

    private static async Task<CommandResponse> ExecuteAsync(IBaseCommand command, params string[] args)
    {
        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new CommandContext(serviceProvider);
        return await command.ExecuteAsync(context, command.GetCommand().Parse(args), CancellationToken.None);
    }
}
