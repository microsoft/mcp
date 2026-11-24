// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Pool;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests.Pool;

public class PoolGetCommandTests : SubscriptionCommandUnitTestsBase<PoolGetCommand, INetAppFilesPoolService>
{
    private static readonly IReadOnlyList<NetAppFilesPool> ExistingPools = [new(
        "pool1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1",
        "eastus",
        4_398_046_511_104,
        "Premium",
        "Auto",
        false,
        "Single",
        null,
        new Dictionary<string, string> { ["environment"] = "test" },
        "Succeeded")];

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("get", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_GetsPool()
    {
        Service.GetPoolAsync(
            "account1",
            "rg",
            "sub",
            "pool1",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(ExistingPools);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(response, NetAppFilesJsonContext.Default.PoolGetResult);
        Assert.Equal(ExistingPools[0].Name, result.Pools[0].Name);
        Assert.Equal(ExistingPools[0].Id, result.Pools[0].Id);
        Assert.Equal(ExistingPools[0].Location, result.Pools[0].Location);
        Assert.Equal(ExistingPools[0].SizeInBytes, result.Pools[0].SizeInBytes);
        Assert.Equal(ExistingPools[0].ServiceLevel, result.Pools[0].ServiceLevel);
        Assert.Equal(ExistingPools[0].QosType, result.Pools[0].QosType);
        Assert.Equal(ExistingPools[0].CoolAccessEnabled, result.Pools[0].CoolAccessEnabled);
        Assert.Equal(ExistingPools[0].EncryptionType, result.Pools[0].EncryptionType);
        Assert.Equal("test", result.Pools[0].Tags["environment"]);
        Assert.Equal(ExistingPools[0].ProvisioningState, result.Pools[0].ProvisioningState);
    }

    [Theory]
    [InlineData("--pool pool1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --subscription sub")]
    [InlineData("--account account1 --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("_pool")]
    [InlineData("-pool")]
    [InlineData("pool/name")]
    [InlineData("pool.name")]
    public async Task ExecuteAsync_InvalidPoolName_ReturnsBadRequest(string pool)
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", pool,
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--pool", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_PoolNotFound_ReturnsSafeMessage()
    {
        Service.GetPoolAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.NotFound,
                "backend payload containing internal pool metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("capacity pool not found", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
