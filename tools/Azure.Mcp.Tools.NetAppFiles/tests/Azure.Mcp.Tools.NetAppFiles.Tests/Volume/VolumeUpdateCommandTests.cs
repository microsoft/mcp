// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Volume;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests.Volume;

public class VolumeUpdateCommandTests : SubscriptionCommandUnitTestsBase<VolumeUpdateCommand, INetAppFilesVolumeService>
{
    private static readonly NetAppFilesVolume UpdatedVolume = new(
        "volume1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1/volumes/volume1",
        "eastus",
        "Succeeded",
        200,
        "Standard");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("update", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesVolumeQuota()
    {
        Service.UpdateVolumeAsync(
            "account1",
            "pool1",
            "volume1",
            200,
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(UpdatedVolume);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--volume", "volume1",
            "--quota-gib", "200",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.VolumeUpdateResult);
        Assert.Equal(UpdatedVolume, result.Volume);
    }

    [Theory]
    [InlineData("--account account1 --pool pool1 --volume volume1 --quota-gib 49 --resource-group rg --subscription sub", "--quota-gib")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --quota-gib 102401 --resource-group rg --subscription sub", "--quota-gib")]
    [InlineData("--account account/name --pool pool1 --volume volume1 --quota-gib 200 --resource-group rg --subscription sub", "--account")]
    [InlineData("--account account1 --pool pool/name --volume volume1 --quota-gib 200 --resource-group rg --subscription sub", "--pool")]
    [InlineData("--account account1 --pool pool1 --volume volume/name --quota-gib 200 --resource-group rg --subscription sub", "--volume")]
    public async Task ExecuteAsync_InvalidOption_ReturnsBadRequest(string args, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--pool pool1 --volume volume1 --quota-gib 200 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --volume volume1 --quota-gib 200 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --quota-gib 200 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --quota-gib 200 --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --quota-gib 200 --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.UpdateVolumeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<long>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.Conflict,
                "backend payload containing internal volume metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--volume", "volume1",
            "--quota-gib", "200",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}