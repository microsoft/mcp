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

public class VolumeGroupUpdateCommandTests : SubscriptionCommandUnitTestsBase<VolumeGroupUpdateCommand, INetAppFilesVolumeGroupService>
{
    private const string ValidVolumes = """
        [{
          "name": "data-volume",
          "creationToken": "data-volume",
          "quotaGib": 100,
          "subnetId": "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf",
          "capacityPoolId": "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1",
          "volumeSpecName": "data",
          "serviceLevel": "Premium",
          "protocols": ["NFSv4.1"],
          "allowedClients": "10.0.0.0/24"
        }]
        """;

    private static readonly NetAppFilesVolumeGroup UpdatedVolumeGroup = new(
        "group1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/volumeGroups/group1",
        "eastus",
        "Succeeded",
        "Oracle",
        "OR2",
        ["data-volume"]);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("update", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesMetadata()
    {
        Service.UpdateVolumeGroupAsync(
            "account1",
            "group1",
            "Oracle",
            "OR2",
            "Oracle volume group",
            null,
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(UpdatedVolumeGroup);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--volume-group", "group1",
            "--application-type", "Oracle",
            "--application-identifier", "OR2",
            "--group-description", "Oracle volume group",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.VolumeGroupUpdateResult);
        Assert.Equal(UpdatedVolumeGroup.Name, result.VolumeGroup.Name);
        Assert.Equal(UpdatedVolumeGroup.Id, result.VolumeGroup.Id);
        Assert.Equal(UpdatedVolumeGroup.Location, result.VolumeGroup.Location);
        Assert.Equal(UpdatedVolumeGroup.ProvisioningState, result.VolumeGroup.ProvisioningState);
        Assert.Equal(UpdatedVolumeGroup.ApplicationType, result.VolumeGroup.ApplicationType);
        Assert.Equal(UpdatedVolumeGroup.ApplicationIdentifier, result.VolumeGroup.ApplicationIdentifier);
        Assert.Equal(UpdatedVolumeGroup.Volumes, result.VolumeGroup.Volumes);
    }

    [Fact]
    public async Task ExecuteAsync_ReplacesVolumes()
    {
        Service.UpdateVolumeGroupAsync(
            "account1",
            "group1",
            null,
            null,
            null,
            Arg.Is<IReadOnlyList<NetAppFilesVolumeGroupVolumeSpecification>>(volumes =>
                volumes.Count == 1 &&
                volumes[0].Name == "data-volume" &&
                volumes[0].VolumeSpecName == "data"),
            "rg",
            "sub",
            null,
            Arg.Any<CancellationToken>())
            .Returns(UpdatedVolumeGroup);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--volume-group", "group1",
            "--volumes", ValidVolumes,
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_NoUpdateProperty_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--volume-group", "group1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("At least one update property", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--application-type", "sap", "SapHana or Oracle")]
    [InlineData("--volumes", "not-json", "valid JSON array")]
    [InlineData("--volumes", "[]", "at least one")]
    public async Task ExecuteAsync_InvalidUpdateProperty_ReturnsBadRequest(
        string option,
        string value,
        string expectedMessage)
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--volume-group", "group1",
            option, value,
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.UpdateVolumeGroupAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<NetAppFilesVolumeGroupVolumeSpecification>?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Azure.RequestFailedException(
                (int)HttpStatusCode.Conflict,
                "backend payload containing internal volume metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--volume-group", "group1",
            "--application-identifier", "OR2",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
