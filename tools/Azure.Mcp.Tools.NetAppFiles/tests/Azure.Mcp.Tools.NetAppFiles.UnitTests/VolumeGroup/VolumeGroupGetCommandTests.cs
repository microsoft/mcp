// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
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

public class VolumeGroupGetCommandTests : SubscriptionCommandUnitTestsBase<VolumeGroupGetCommand, INetAppFilesService>
{
    [Fact]
    public async Task ExecuteAsync_NoVolumeGroupParameter_ReturnsAllVolumeGroups()
    {
        // Arrange
        var subscription = "sub123";
        var expectedVolumeGroups = new ResourceQueryResults<VolumeGroupInfo>(
        [
            new("account1/vg1", "eastus", "rg1", "Succeeded", "SAP-HANA", "SH1", "Volume group for SAP HANA"),
            new("account1/vg2", "westus", "rg2", "Succeeded", "SAP-HANA", "SH2", "Another volume group")
        ], false);

        Service.GetVolumeGroupDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumeGroups));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGroupGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.VolumeGroups);
        Assert.Equal(expectedVolumeGroups.Results.Count, result.VolumeGroups.Count);
        Assert.Equal(expectedVolumeGroups.Results.Select(v => v.Name), result.VolumeGroups.Select(v => v.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoVolumeGroups()
    {
        // Arrange
        var subscription = "sub123";

        Service.GetVolumeGroupDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<VolumeGroupInfo>([], false));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGroupGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.VolumeGroups);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        Service.GetVolumeGroupDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --account myanfaccount", true)]
    [InlineData("--subscription sub123 --account myanfaccount --volumeGroup myvg", true)]
    [InlineData("--subscription sub123 --volumeGroup myvg", true)] // VolumeGroup without account is valid
    [InlineData("--account myanfaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedVolumeGroups = new ResourceQueryResults<VolumeGroupInfo>(
                [new("account1/vg1", "eastus", "rg1", "Succeeded", "SAP-HANA", "SH1", "Volume group for SAP HANA")],
                false);

            Service.GetVolumeGroupDetails(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedVolumeGroups));
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
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsVolumeGroupDetails_WhenVolumeGroupExists()
    {
        // Arrange
        var account = "myanfaccount";
        var volumeGroup = "myvg";
        var subscription = "sub123";
        var expectedVolumeGroups = new ResourceQueryResults<VolumeGroupInfo>(
            [new($"{account}/{volumeGroup}", "eastus", "rg1", "Succeeded", "SAP-HANA", "SH1", "Volume group for SAP HANA")],
            false);

        Service.GetVolumeGroupDetails(
            Arg.Is(account), Arg.Is(volumeGroup), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumeGroups));

        // Act
        var response = await ExecuteCommandAsync(["--account", account, "--volumeGroup", volumeGroup, "--subscription", subscription]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGroupGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.VolumeGroups);
        Assert.Equal($"{account}/{volumeGroup}", result.VolumeGroups[0].Name);
        Assert.Equal("eastus", result.VolumeGroups[0].Location);
        Assert.Equal("rg1", result.VolumeGroups[0].ResourceGroup);
        Assert.Equal("SAP-HANA", result.VolumeGroups[0].GroupMetaDataApplicationType);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "sub123";

        Service.GetVolumeGroupDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var subscription = "sub123";
        var volumeGroup = "nonexistentvg";

        Service.GetVolumeGroupDetails(
            Arg.Any<string?>(), Arg.Is(volumeGroup), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Volume group not found"));

        var response = await ExecuteCommandAsync(["--volumeGroup", volumeGroup, "--subscription", subscription]);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Volume group not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var subscription = "sub123";

        Service.GetVolumeGroupDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "sub123";
        var expectedVolumeGroups = new ResourceQueryResults<VolumeGroupInfo>(
            [new("account1/vg1", "eastus", "rg1", "Succeeded", "SAP-HANA", "SH1", "Volume group for SAP HANA")],
            false);

        Service.GetVolumeGroupDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumeGroups));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGroupGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.VolumeGroups);
        var vg = result.VolumeGroups[0];
        Assert.Equal("account1/vg1", vg.Name);
        Assert.Equal("eastus", vg.Location);
        Assert.Equal("rg1", vg.ResourceGroup);
        Assert.Equal("Succeeded", vg.ProvisioningState);
        Assert.Equal("SAP-HANA", vg.GroupMetaDataApplicationType);
        Assert.Equal("SH1", vg.GroupMetaDataApplicationIdentifier);
        Assert.Equal("Volume group for SAP HANA", vg.GroupMetaDataDescription);
    }

    [Fact]
    public async Task ExecuteAsync_PassesResourceGroupFilterToService()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";

        Service.GetVolumeGroupDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<VolumeGroupInfo>([], false));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription, "--resource-group", resourceGroup]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetVolumeGroupDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(resourceGroup),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesIdsFilterToService()
    {
        // Arrange
        var subscription = "sub123";
        const string id1 = "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.NetApp/netAppAccounts/account1/volumeGroups/vg1";

        Service.GetVolumeGroupDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<VolumeGroupInfo>([], false));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription, "--ids", id1]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetVolumeGroupDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is<IReadOnlyList<string>?>(ids => ids != null && ids.Count == 1 && ids[0] == id1),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
