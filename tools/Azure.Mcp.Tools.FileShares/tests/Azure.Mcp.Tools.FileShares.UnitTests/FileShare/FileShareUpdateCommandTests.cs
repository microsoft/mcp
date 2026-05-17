// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.FileShare;

/// <summary>
/// Unit tests for FileShareUpdateCommand.
/// </summary>
public class FileShareUpdateCommandTests
{
    private readonly IFileSharesService _service;
    private readonly ILogger<FileShareUpdateCommand> _logger;
    private readonly FileShareUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    public FileShareUpdateCommandTests()
    {
        _service = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<FileShareUpdateCommand>>();
        _command = new(_logger, _service);

        var collection = new ServiceCollection().AddSingleton(_service);
        _context = new(collection.BuildServiceProvider());
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("update", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Update File Share", _command.Title);
    }

    [Fact]
    public void BindOptions_BindsNfsEncryptionInTransitCorrectly()
    {
        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--name", "test-share",
            "--nfs-encryption-in-transit", "Disabled"
        ]);

        var options = _command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, [parseResult]) as FileShareCreateOrUpdateOptions;

        Assert.NotNull(options);
        Assert.Equal("Disabled", options!.NfsEncryptionInTransit);
        Assert.Equal("test-sub", options.Subscription);
        Assert.Equal("test-rg", options.ResourceGroup);
        Assert.Equal("test-share", options.FileShareName);
    }

    [Fact]
    public void BindOptions_NfsEncryptionInTransitIsNullWhenNotProvided()
    {
        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--name", "test-share"
        ]);

        var options = _command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, [parseResult]) as FileShareCreateOrUpdateOptions;

        Assert.NotNull(options);
        Assert.Null(options!.NfsEncryptionInTransit);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --name share1", true)]
    [InlineData("--subscription sub --resource-group rg --name share1 --nfs-encryption-in-transit Enabled", true)]
    [InlineData("--subscription sub --resource-group rg --name share1 --nfs-root-squash RootSquash --nfs-encryption-in-transit Disabled", true)]
    [InlineData("--subscription sub --resource-group rg", false)] // Missing name
    [InlineData("--subscription sub --name share1", false)] // Missing resource group
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
                HostName: null,
                MediaTier: null,
                Redundancy: null,
                Protocol: "NFS",
                ProvisionedStorageInGiB: 100,
                ProvisionedIOPerSec: null,
                ProvisionedThroughputMiBPerSec: null,
                PublicNetworkAccess: null);

            _service.PatchFileShareAsync(
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

        var parseResult = _commandDefinition.Parse(args);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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

        _service.PatchFileShareAsync(
            "sub",
            "rg",
            "share1",
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

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--name", "share1",
            "--nfs-root-squash", "RootSquash",
            "--nfs-encryption-in-transit", "Enabled"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
        await _service.Received(1).PatchFileShareAsync(
            "sub",
            "rg",
            "share1",
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

        _service.PatchFileShareAsync(
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

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--name", "share1"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = System.Text.Json.JsonSerializer.Serialize(response.Results);
        var result = System.Text.Json.JsonSerializer.Deserialize(json, FileSharesJsonContext.Default.FileShareUpdateCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result!.FileShare);
        Assert.Equal("share1", result.FileShare.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _service.PatchFileShareAsync(
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

        var parseResult = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--name", "share1"
        ]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
