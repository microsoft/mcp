// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.VolumeGroup;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests.VolumeGroup;

public class VolumeGroupGetCommandTests : SubscriptionCommandUnitTestsBase<VolumeGroupGetCommand, INetAppFilesVolumeGroupService>
{
    private static readonly NetAppFilesVolumeGroup ExistingVolumeGroup = new(
        "group1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/volumeGroups/group1",
        "eastus",
        "Succeeded",
        "SapHana",
        "SH1",
        ["data-volume", "log-volume"]);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("get", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_GetsVolumeGroup()
    {
        Service.GetVolumeGroupAsync(
            "account1",
            "group1",
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(ExistingVolumeGroup);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--volume-group", "group1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.VolumeGroupGetResult);
        Assert.Equal(ExistingVolumeGroup.Name, result.VolumeGroup.Name);
        Assert.Equal(ExistingVolumeGroup.Id, result.VolumeGroup.Id);
        Assert.Equal(ExistingVolumeGroup.Location, result.VolumeGroup.Location);
        Assert.Equal(ExistingVolumeGroup.ProvisioningState, result.VolumeGroup.ProvisioningState);
        Assert.Equal(ExistingVolumeGroup.ApplicationType, result.VolumeGroup.ApplicationType);
        Assert.Equal(ExistingVolumeGroup.ApplicationIdentifier, result.VolumeGroup.ApplicationIdentifier);
        Assert.Equal(ExistingVolumeGroup.Volumes, result.VolumeGroup.Volumes);
    }

    [Theory]
    [InlineData("--account", "_account")]
    [InlineData("--account", "account/name")]
    [InlineData("--volume-group", "_group")]
    [InlineData("--volume-group", "group/name")]
    public async Task ExecuteAsync_InvalidResourceName_ReturnsBadRequest(string option, string value)
    {
        var arguments = new[]
        {
            "--account", "account1",
            "--volume-group", "group1",
            "--resource-group", "rg",
            "--subscription", "sub"
        };
        arguments[Array.IndexOf(arguments, option) + 1] = value;

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(option, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--volume-group group1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --volume-group group1 --subscription sub")]
    [InlineData("--account account1 --volume-group group1 --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_VolumeGroupNotFound_ReturnsSafeMessage()
    {
        Service.GetVolumeGroupAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Azure.RequestFailedException(
                (int)HttpStatusCode.NotFound,
                "backend payload containing internal volume metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--volume-group", "group1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("volume group not found", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
