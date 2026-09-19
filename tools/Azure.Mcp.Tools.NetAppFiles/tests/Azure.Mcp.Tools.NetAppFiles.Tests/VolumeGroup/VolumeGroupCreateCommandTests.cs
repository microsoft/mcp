// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.VolumeGroup;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests.VolumeGroup;

public class VolumeGroupCreateCommandTests : SubscriptionCommandUnitTestsBase<VolumeGroupCreateCommand, INetAppFilesVolumeGroupService>
{
    private const string SubnetId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf";
    private const string CapacityPoolId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1";
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

    private static readonly NetAppFilesVolumeGroup CreatedVolumeGroup = new(
        "group1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/volumeGroups/group1",
        "eastus",
        "Succeeded",
        "SapHana",
        "SH1",
        ["data-volume"]);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("create", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_CreatesVolumeGroup()
    {
        Service.CreateVolumeGroupAsync(
            "account1",
            "group1",
            "eastus",
            "SapHana",
            "SH1",
            Arg.Is<IReadOnlyList<NetAppFilesVolumeGroupVolumeSpecification>>(volumes =>
                volumes.Count == 1 &&
                volumes[0].Name == "data-volume" &&
                volumes[0].SubnetId == SubnetId &&
                volumes[0].CapacityPoolId == CapacityPoolId),
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(CreatedVolumeGroup);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--volume-group", "group1",
            "--location", "eastus",
            "--application-type", "SapHana",
            "--application-identifier", "SH1",
            "--volumes", ValidVolumes,
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        Assert.True(response.Status == HttpStatusCode.OK, response.Message);
        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.VolumeGroupCreateResult);
        Assert.Equal(CreatedVolumeGroup.Name, result.VolumeGroup.Name);
        Assert.Equal(CreatedVolumeGroup.Id, result.VolumeGroup.Id);
        Assert.Equal(CreatedVolumeGroup.Location, result.VolumeGroup.Location);
        Assert.Equal(CreatedVolumeGroup.ProvisioningState, result.VolumeGroup.ProvisioningState);
        Assert.Equal(CreatedVolumeGroup.ApplicationType, result.VolumeGroup.ApplicationType);
        Assert.Equal(CreatedVolumeGroup.ApplicationIdentifier, result.VolumeGroup.ApplicationIdentifier);
        Assert.Equal(CreatedVolumeGroup.Volumes, result.VolumeGroup.Volumes);
    }

    [Theory]
    [InlineData("not-json", "valid JSON array")]
    [InlineData("[]", "at least one")]
    public async Task ExecuteAsync_InvalidVolumesJson_ReturnsBadRequest(string volumes, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(CreateArguments(volumes));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidVolumeProperties_ReturnsBadRequest()
    {
        const string volumes = """
            [{
              "name": "data-volume",
              "creationToken": "1invalid",
              "quotaGib": 49,
              "subnetId": "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/storage1",
              "capacityPoolId": "not-an-arm-id",
              "volumeSpecName": "data",
              "serviceLevel": "Invalid",
              "protocols": ["SMB"]
            }]
            """;

        var response = await ExecuteCommandAsync(CreateArguments(volumes));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("creationToken", response.Message);
        Assert.Contains("quotaGib", response.Message);
        Assert.Contains("subnetId", response.Message);
        Assert.Contains("capacityPoolId", response.Message);
        Assert.Contains("serviceLevel", response.Message);
        Assert.Contains("protocols", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_NullRequiredVolumeProperties_ReturnsBadRequest()
    {
        const string volumes = """
            [{
              "name": null,
              "creationToken": null,
              "quotaGib": 100,
              "subnetId": null,
              "capacityPoolId": null,
              "volumeSpecName": null
            }]
            """;

        var response = await ExecuteCommandAsync(CreateArguments(volumes));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("volume name", response.Message);
        Assert.Contains("creationToken", response.Message);
        Assert.Contains("subnetId", response.Message);
        Assert.Contains("capacityPoolId", response.Message);
        Assert.Contains("volumeSpecName", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_NfsVolumeWithoutAllowedClients_ReturnsBadRequest()
    {
        const string volumes = """
            [{
              "name": "data-volume",
              "creationToken": "data-volume",
              "quotaGib": 100,
              "subnetId": "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf",
              "capacityPoolId": "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1",
              "volumeSpecName": "data",
              "protocols": ["NFSv4.1"]
            }]
            """;

        var options = new VolumeGroupCreateOptions
        {
            Account = "account1",
            VolumeGroup = "group1",
            Location = "eastus",
            ApplicationType = "SapHana",
            ApplicationIdentifier = "SH1",
            Volumes = volumes,
            ResourceGroup = "rg",
            Subscription = "sub"
        };
        var validationResult = new ValidationResult();
        Command.ValidateOptions(options, validationResult);
        Assert.Contains(validationResult.Errors, error => error.Contains("allowedClients", StringComparison.Ordinal));

        var response = await ExecuteCommandAsync(CreateArguments(volumes));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("allowedClients", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateVolumeNames_ReturnsBadRequest()
    {
        const string duplicateVolumes = """
                        [
                            {
                                "name": "data-volume",
                                "creationToken": "data-volume",
                                "quotaGib": 100,
                                "subnetId": "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf",
                                "capacityPoolId": "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1",
                                "volumeSpecName": "data"
                            },
                            {
                                "name": "data-volume",
                                "creationToken": "data-volume",
                                "quotaGib": 100,
                                "subnetId": "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/anf",
                                "capacityPoolId": "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1",
                                "volumeSpecName": "log"
                            }
                        ]
                        """;

        var response = await ExecuteCommandAsync(CreateArguments(duplicateVolumes));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Duplicate volume name", response.Message);
        Assert.Contains("Duplicate creationToken", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("sap", "SapHana or Oracle")]
    public async Task ExecuteAsync_InvalidApplicationValue_ReturnsBadRequest(string value, string expectedMessage)
    {
        var arguments = CreateArguments(ValidVolumes);
        var optionIndex = Array.IndexOf(arguments, "--application-type");
        arguments[optionIndex + 1] = value;

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.CreateVolumeGroupAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<NetAppFilesVolumeGroupVolumeSpecification>>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Azure.RequestFailedException(
                (int)HttpStatusCode.Conflict,
                "backend payload containing internal volume metadata"));

        var response = await ExecuteCommandAsync(CreateArguments(ValidVolumes));

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }

    private static string[] CreateArguments(string volumes) =>
    [
        "--account", "account1",
        "--volume-group", "group1",
        "--location", "eastus",
        "--application-type", "SapHana",
        "--application-identifier", "SH1",
        "--volumes", volumes,
        "--resource-group", "rg",
        "--subscription", "sub"
    ];
}
