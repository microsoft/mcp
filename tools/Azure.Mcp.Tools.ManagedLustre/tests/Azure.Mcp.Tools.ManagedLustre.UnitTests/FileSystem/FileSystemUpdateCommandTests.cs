// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ManagedLustre.Commands;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Models;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem;

public class FileSystemUpdateCommandTests : CommandUnitTestsBase<FileSystemUpdateCommand, IManagedLustreService>
{
    private const string Sub = "sub123";
    private const string Rg = "rg1";
    private const string Name = "amlfs-01";
    private const string Location = "eastus";
    private const string Sku = "AMLFS-Durable-Premium-125";
    private const int Size = 4;
    private const string SubnetId = "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/sub1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01", false)] // Missing update params
    [InlineData("--resource-group rg1 --name amlfs-01 --maintenance-day Monday", false)] // Missing subscription
    [InlineData("--subscription sub123 --name amlfs-01 --maintenance-day Monday", false)] // Missing resource group
    [InlineData("--subscription sub123 --resource-group rg1 --maintenance-day Monday", false)] // Missing name
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --maintenance-day Monday", false)]
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --maintenance-time 00:00", false)]
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --no-squash-nid-list nid1,nid2 --squash-uid 1000", false)] // missing gid
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.UpdateFileSystemAsync(
                Arg.Is(Sub),
                Arg.Is(Rg),
                Arg.Is(Name),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<long?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(CreateLustre());
        }

        // Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.False(string.IsNullOrWhiteSpace(response.Message));
            Assert.True(
                response.Message.Contains("required", StringComparison.OrdinalIgnoreCase)
                || response.Message.Contains("provide", StringComparison.OrdinalIgnoreCase)
                || response.Message.Contains("must be", StringComparison.OrdinalIgnoreCase)
            );
        }
        else
        {
            Assert.NotNull(response.Results);
        }
    }

    [Fact]
    public async Task ExecuteAsync_MaintenanceUpdate_CallsServiceAndReturnsResult()
    {
        var expected = CreateLustre();
        Service.UpdateFileSystemAsync(
            Sub,
            Rg,
            Name,
            "Monday",
            "01:00",
            null,
            null,
            null,
            null,
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--maintenance-day", "Monday",
            "--maintenance-time", "01:00");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateFileSystemAsync(
            Sub,
            Rg,
            Name,
            "Monday",
            "01:00",
            null,
            null,
            null,
            null,
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.FileSystemUpdateResult);
        Assert.Equal(Name, result.FileSystem.Name);
    }

    [Fact]
    public async Task ExecuteAsync_RootSquashUpdate_CallsService()
    {
        var expected = CreateLustre();
        Service.UpdateFileSystemAsync(
            Sub,
            Rg,
            Name,
            null,
            null,
            "All",
            "nid1,nid2",
            1000,
            1000,
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--root-squash-mode", "All",
            "--no-squash-nid-list", "nid1,nid2",
            "--squash-uid", "1000",
            "--squash-gid", "1000");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateFileSystemAsync(
            Sub,
            Rg,
            Name,
            null,
            null,
            "All",
            "nid1,nid2",
            1000,
            1000,
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsRequestFailed_ReturnsStatus()
    {
        Service.UpdateFileSystemAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "not found"));

        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--maintenance-day", "Monday",
            "--maintenance-time", "00:00");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --root-squash-mode All --squash-uid 1000", false)] // missing gid
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --root-squash-mode All --squash-gid 1000", false)] // missing uid
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --root-squash-mode None", true)] // None doesn't require uid/gid
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --root-squash-mode All  --squash-uid 1000 --squash-gid 1000 --no-squash-nid-list 10.0.0.10", true)] // Should succeed
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --root-squash-mode None --maintenance-day Monday --maintenance-time 00:00", true)] // None doesn't require uid/gid
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --root-squash-mode None --maintenance-time 00:00", false)] // Missing time
    [InlineData("--subscription sub123 --resource-group rg1 --name amlfs-01 --root-squash-mode None --maintenance-day Monday", false)] // Missing day
    public async Task ExecuteAsync_RootSquashMode_Validation_Works(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.UpdateFileSystemAsync(
                Arg.Is(Sub),
                Arg.Is(Rg),
                Arg.Is(Name),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<long?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(CreateLustre());
        }

        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.False(string.IsNullOrWhiteSpace(response.Message));
            Assert.True(
                response.Message.Contains("squash", StringComparison.OrdinalIgnoreCase) ||
                response.Message.Contains("maintenance", StringComparison.OrdinalIgnoreCase),
                $"Expected error message to mention 'squash' or 'maintenance' but was: {response.Message}");
        }
    }

    [Fact]
    public async Task ExecuteAsync_RootSquashMode_WithUidGid_SucceedsAndCallsService()
    {
        var expected = CreateLustre();
        Service.UpdateFileSystemAsync(
            Sub,
            Rg,
            Name,
            null,
            null,
            "All",
            "nid1,nid2",
            2000,
            3000,
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--root-squash-mode", "All",
            "--no-squash-nid-list", "nid1,nid2",
            "--squash-uid", "2000",
            "--squash-gid", "3000");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateFileSystemAsync(
            Sub,
            Rg,
            Name,
            null,
            null,
            "All",
            "nid1,nid2",
            2000,
            3000,
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RootSquashNotNone_MissingNoSquashNidList_Returns400()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--resource-group", Rg,
            "--name", Name,
            "--root-squash-mode", "All",
            "--squash-uid", "1000",
            "--squash-gid", "1000");

        Assert.True(response.Status >= HttpStatusCode.BadRequest);
        Assert.Contains("no-squash", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static LustreFileSystem CreateLustre() => new(
        Name,
        $"/subs/{Sub}/rg/{Rg}/providers/Microsoft.StorageCache/amlfs/{Name}",
        Rg,
        Sub,
        Location,
        "Succeeded",
        "Healthy",
        "10.0.0.4",
        Sku,
        Size,
        SubnetId,          // Added: subnet ID
        "Monday",
        "00:00",
        "None",            // rootSquashMode
        null,              // noSquashNidList
        null,              // squashUid
        null,              // squashGid
        null,              // HSM data container
        null);             // HSM Log container
}
