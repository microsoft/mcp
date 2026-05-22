// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.Tests.FileShare;

/// <summary>
/// Unit tests for FileShareCreateCommand.
/// </summary>
public class FileShareCreateCommandTests : CommandUnitTestsBase<FileShareCreateCommand, IFileSharesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", Command.Name);
        Assert.Equal("Create File Share", Command.Title);
        Assert.Equal("create", CommandDefinition.Name);
    }

    [Fact]
    public void BindOptions_BindsNfsEncryptionInTransitCorrectly()
    {
        var parseResult = CommandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--name", "test-share",
            "--location", "eastus",
            "--nfs-encryption-in-transit", "Enabled"
        ]);

        var options = Command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(Command, [parseResult]) as FileShareCreateOrUpdateOptions;

        Assert.NotNull(options);
        Assert.Equal("Enabled", options!.NfsEncryptionInTransit);
        Assert.Equal("test-sub", options.Subscription);
        Assert.Equal("test-rg", options.ResourceGroup);
        Assert.Equal("test-share", options.FileShareName);
        Assert.Equal("eastus", options.Location);
    }

    [Fact]
    public void BindOptions_NfsEncryptionInTransitIsNullWhenNotProvided()
    {
        var parseResult = CommandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--name", "test-share",
            "--location", "eastus"
        ]);

        var options = Command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(Command, [parseResult]) as FileShareCreateOrUpdateOptions;

        Assert.NotNull(options);
        Assert.Null(options!.NfsEncryptionInTransit);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --name share1 --location eastus", true)]
    [InlineData("--subscription sub --resource-group rg --name share1 --location eastus --nfs-encryption-in-transit Enabled", true)]
    [InlineData("--subscription sub --resource-group rg --name share1 --location eastus --nfs-root-squash RootSquash --nfs-encryption-in-transit Disabled", true)]
    [InlineData("--subscription sub --resource-group rg --location eastus", false)] // Missing name
    [InlineData("--subscription sub --name share1 --location eastus", false)] // Missing resource group
    [InlineData("--subscription sub --resource-group rg --name share1", false)] // Missing location
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedShare = new FileShareInfo(
                Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa/fileShares/share1",
                Name: "share1",
                Location: "eastus",
                ResourceGroup: "rg",
                Type: "Microsoft.Storage/storageAccounts/fileShares",
                ProvisioningState: "Succeeded",
                MountName: "share1",
                HostName: "sa.file.core.windows.net",
                MediaTier: "SSD",
                Redundancy: "Local",
                Protocol: "NFS",
                ProvisionedStorageInGiB: 100,
                ProvisionedIOPerSec: 3000,
                ProvisionedThroughputMiBPerSec: 125,
                PublicNetworkAccess: "Enabled");

            Service.CreateOrUpdateFileShareAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<Dictionary<string, string>>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedShare);
        }

        var parseResult = CommandDefinition.Parse(args);
        var response = await Command.ExecuteAsync(Context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(shouldSucceed ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_PassesNfsEncryptionInTransitToService()
    {
        var expectedShare = new FileShareInfo(
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa/fileShares/share1",
            Name: "share1",
            Location: "eastus",
            ResourceGroup: "rg",
            Type: "Microsoft.Storage/storageAccounts/fileShares",
            ProvisioningState: "Succeeded",
            MountName: "share1",
            HostName: null,
            MediaTier: null,
            Redundancy: null,
            Protocol: "NFS",
            ProvisionedStorageInGiB: 100,
            ProvisionedIOPerSec: null,
            ProvisionedThroughputMiBPerSec: null,
            PublicNetworkAccess: null);

        Service.CreateOrUpdateFileShareAsync(
            "sub",
            "rg",
            "share1",
            "eastus",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "RootSquash",
            "Enabled",
            Arg.Any<string[]>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedShare);

        var parseResult = CommandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--name", "share1",
            "--location", "eastus",
            "--nfs-root-squash", "RootSquash",
            "--nfs-encryption-in-transit", "Enabled"
        ]);

        var response = await Command.ExecuteAsync(Context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateOrUpdateFileShareAsync(
            "sub",
            "rg",
            "share1",
            "eastus",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "RootSquash",
            "Enabled",
            Arg.Any<string[]>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var expectedShare = new FileShareInfo(
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa/fileShares/share1",
            Name: "share1",
            Location: "eastus",
            ResourceGroup: "rg",
            Type: "Microsoft.Storage/storageAccounts/fileShares",
            ProvisioningState: "Succeeded",
            MountName: "share1",
            HostName: null,
            MediaTier: null,
            Redundancy: null,
            Protocol: "NFS",
            ProvisionedStorageInGiB: 100,
            ProvisionedIOPerSec: null,
            ProvisionedThroughputMiBPerSec: null,
            PublicNetworkAccess: null);

        Service.CreateOrUpdateFileShareAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedShare);

        var parseResult = CommandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--name", "share1",
            "--location", "eastus"
        ]);

        var response = await Command.ExecuteAsync(Context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = System.Text.Json.JsonSerializer.Serialize(response.Results);
        var result = System.Text.Json.JsonSerializer.Deserialize(json, FileSharesJsonContext.Default.FileShareCreateCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result!.FileShare);
        Assert.Equal("share1", result.FileShare.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.CreateOrUpdateFileShareAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = CommandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--name", "share1",
            "--location", "eastus"
        ]);

        var response = await Command.ExecuteAsync(Context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
