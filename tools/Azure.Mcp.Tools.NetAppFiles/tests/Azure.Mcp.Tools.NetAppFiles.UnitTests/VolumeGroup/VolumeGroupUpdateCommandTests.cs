// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.VolumeGroup;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tests.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.VolumeGroup;

public class VolumeGroupUpdateCommandTests : SubscriptionCommandUnitTestsBase<VolumeGroupUpdateCommand, INetAppFilesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --volume-group myvg --resource-group myrg --location eastus --subscription sub123", true)]
    [InlineData("--account myanfaccount --volume-group myvg --resource-group myrg --location eastus --subscription sub123 --group-description UpdatedDescription", true)]
    [InlineData("--ids /subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/volumeGroups/myvg --location eastus --subscription sub123", true)]
    [InlineData("--volume-group myvg --resource-group myrg --location eastus --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --subscription sub123", false)] // Missing volumeGroup
    [InlineData("--account myanfaccount --volume-group myvg --location eastus --subscription sub123", false)] // Missing resource-group
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedVolumeGroup = new VolumeGroupCreateResult(
                Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/volumeGroups/myvg",
                Name: "myanfaccount/myvg",
                Type: "Microsoft.NetApp/netAppAccounts/volumeGroups",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded",
                GroupMetaDataApplicationType: "SAP-HANA",
                GroupMetaDataApplicationIdentifier: "SH1",
                GroupMetaDataDescription: null);

            Service.UpdateVolumeGroup(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<Dictionary<string, string>?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedVolumeGroup);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.True(
                response.Message.Contains("required", StringComparison.OrdinalIgnoreCase) ||
                response.Message.Contains("either --ids", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesVolumeGroup_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var volumeGroup = "myvg";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var groupDescription = "Updated volume group description";

        var expectedVolumeGroup = new VolumeGroupCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/volumeGroups/{volumeGroup}",
            Name: $"{account}/{volumeGroup}",
            Type: "Microsoft.NetApp/netAppAccounts/volumeGroups",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            GroupMetaDataApplicationType: "SAP-HANA",
            GroupMetaDataApplicationIdentifier: "SH1",
            GroupMetaDataDescription: groupDescription);

        Service.UpdateVolumeGroup(
            Arg.Is(account), Arg.Is(volumeGroup), Arg.Is(resourceGroup),
            Arg.Is(location), Arg.Is(subscription), Arg.Is(groupDescription),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumeGroup));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--volume-group", volumeGroup,
            "--resource-group", resourceGroup, "--location", location,
            "--group-description", groupDescription, "--subscription", subscription
        ]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGroupUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.VolumeGroup);
        Assert.Equal($"{account}/{volumeGroup}", result.VolumeGroup.Name);
        Assert.Equal(location, result.VolumeGroup.Location);
        Assert.Equal(resourceGroup, result.VolumeGroup.ResourceGroup);
        Assert.Equal("Succeeded", result.VolumeGroup.ProvisioningState);
        Assert.Equal("SAP-HANA", result.VolumeGroup.GroupMetaDataApplicationType);
        Assert.Equal("SH1", result.VolumeGroup.GroupMetaDataApplicationIdentifier);
        Assert.Equal(groupDescription, result.VolumeGroup.GroupMetaDataDescription);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesVolumeGroupWithTags_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var volumeGroup = "myvg";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var tagsJson = "{\"env\":\"prod\",\"team\":\"storage\"}";

        var expectedVolumeGroup = new VolumeGroupCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/volumeGroups/{volumeGroup}",
            Name: $"{account}/{volumeGroup}",
            Type: "Microsoft.NetApp/netAppAccounts/volumeGroups",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            GroupMetaDataApplicationType: "SAP-HANA",
            GroupMetaDataApplicationIdentifier: "SH1",
            GroupMetaDataDescription: null);

        Service.UpdateVolumeGroup(
            Arg.Is(account), Arg.Is(volumeGroup), Arg.Is(resourceGroup),
            Arg.Is(location), Arg.Is(subscription), Arg.Any<string>(),
            Arg.Is<Dictionary<string, string>>(d => d.ContainsKey("env") && d["env"] == "prod"),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumeGroup));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--volume-group", volumeGroup,
            "--resource-group", resourceGroup, "--location", location,
            "--tags", tagsJson, "--subscription", subscription
        ]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidTagsJson()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--volume-group", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--tags", "not-valid-json", "--subscription", "sub123"
        ]);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid tags JSON format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.UpdateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--volume-group", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        Service.UpdateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Volume group not found"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--volume-group", "myvg",
            "--resource-group", "nonexistentrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        Service.UpdateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--volume-group", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.UpdateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<VolumeGroupCreateResult>(new Exception("Test error")));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--volume-group", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        // Act
        var args = CommandDefinition.Parse([
            "--account", "myanfaccount", "--volume-group", "myvg",
            "--resource-group", "myrg", "--location", "eastus",
            "--group-description", "Updated description",
            "--tags", "{\"env\":\"prod\"}",
            "--subscription", "sub123"
        ]);

        // Assert - if no exception, binding worked correctly
        Assert.Empty(args.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesIdsAndCallsServiceWithResolvedArguments()
    {
        // Arrange
        var id = "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/volumeGroups/myvg";

        var expectedVolumeGroup = new VolumeGroupCreateResult(
            Id: id,
            Name: "myanfaccount/myvg",
            Type: "Microsoft.NetApp/netAppAccounts/volumeGroups",
            Location: "eastus",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded",
            GroupMetaDataApplicationType: "SAP-HANA",
            GroupMetaDataApplicationIdentifier: "SH1",
            GroupMetaDataDescription: null);

        Service.UpdateVolumeGroup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumeGroup));

        // Act
        var response = await ExecuteCommandAsync([
            "--ids", id,
            "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateVolumeGroup(
            Arg.Is("myanfaccount"), Arg.Is("myvg"), Arg.Is("myrg"),
            Arg.Is("eastus"), Arg.Is("sub123"), Arg.Any<string?>(),
            Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--no-wait")]
    [InlineData("--add properties.groupMetaData={}")]
    [InlineData("--set properties.groupMetaData.applicationIdentifier=SH2")]
    [InlineData("--remove properties.groupMetaData")]
    [InlineData("--force-string")]
    [InlineData("--group-meta-data {\"applicationType\":\"SAP-HANA\"}")]
    [InlineData("--volumes []")]
    public async Task ExecuteAsync_ReturnsBadRequest_ForUnsupportedUpdateArguments(string unsupportedArg)
    {
        // Act
        var response = await ExecuteCommandAsync($"--account myanfaccount --volume-group myvg --resource-group myrg --location eastus --subscription sub123 {unsupportedArg}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not supported", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
