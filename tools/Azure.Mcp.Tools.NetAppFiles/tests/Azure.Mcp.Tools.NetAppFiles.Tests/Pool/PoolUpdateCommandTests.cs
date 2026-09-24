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

public class PoolUpdateCommandTests : SubscriptionCommandUnitTestsBase<PoolUpdateCommand, INetAppFilesPoolService>
{
    private const long EightTebibytes = 8_796_093_022_208;

    private static readonly NetAppFilesPool UpdatedPool = new(
        "pool1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1",
        "eastus",
        EightTebibytes,
        "Flexible",
        "Manual",
        false,
        "Single",
        128,
        new Dictionary<string, string> { ["environment"] = "test" },
        "Succeeded");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("update", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesAllSupportedProperties()
    {
        Service.UpdatePoolAsync(
            "account1",
            "pool1",
            "rg",
            "sub",
            EightTebibytes,
            "Manual",
            false,
            128,
            Arg.Is<IReadOnlyDictionary<string, string>>(tags =>
                tags.Count == 1 && tags["environment"] == "test"),
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(UpdatedPool);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--size", "8",
            "--qos-type", "Manual",
            "--cool-access", "false",
            "--custom-throughput-mibps", "128",
            "--tags", "{\"environment\":\"test\"}",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(response, NetAppFilesJsonContext.Default.PoolUpdateResult);
        Assert.Equal(UpdatedPool.Name, result.Pool.Name);
        Assert.Equal(UpdatedPool.SizeInBytes, result.Pool.SizeInBytes);
        Assert.Equal(UpdatedPool.QosType, result.Pool.QosType);
        Assert.False(result.Pool.CoolAccessEnabled);
        Assert.Equal(UpdatedPool.CustomThroughputMibps, result.Pool.CustomThroughputMibps);
        Assert.Equal("test", result.Pool.Tags["environment"]);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyTags_ClearsTags()
    {
        Service.UpdatePoolAsync(
            "account1",
            "pool1",
            "rg",
            "sub",
            null,
            null,
            null,
            null,
            Arg.Is<IReadOnlyDictionary<string, string>>(tags => tags.Count == 0),
            null,
            Arg.Any<CancellationToken>())
            .Returns(UpdatedPool);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tags", "{}");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_NoUpdateProperty_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("At least one update property", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("_account")]
    [InlineData("-account")]
    [InlineData("account/name")]
    public async Task ExecuteAsync_InvalidAccountName_ReturnsBadRequest(string account)
    {
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--pool", "pool1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tags", "{}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--account", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("_pool")]
    [InlineData("-pool")]
    [InlineData("pool/name")]
    public async Task ExecuteAsync_InvalidPoolName_ReturnsBadRequest(string pool)
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", pool,
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tags", "{}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--pool", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("0")]
    [InlineData("2")]
    [InlineData("5")]
    [InlineData("8388608")]
    public async Task ExecuteAsync_InvalidSize_ReturnsBadRequest(string size)
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--size", size,
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--size", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--qos-type Dynamic", "--qos-type")]
    [InlineData("--custom-throughput-mibps 0", "--custom-throughput-mibps")]
    [InlineData("--qos-type Auto --custom-throughput-mibps 128", "--custom-throughput-mibps")]
    public async Task ExecuteAsync_InvalidUpdateCombination_ReturnsBadRequest(string updateOptions, string expectedOption)
    {
        var response = await ExecuteCommandAsync(
            $"--account account1 --pool pool1 --resource-group rg --subscription sub {updateOptions}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedOption, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("{invalid-json}")]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("{\"environment\":1}")]
    public async Task ExecuteAsync_InvalidTags_ReturnsBadRequest(string tags)
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tags", tags);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--tags", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--pool pool1 --resource-group rg --subscription sub --tags {}")]
    [InlineData("--account account1 --resource-group rg --subscription sub --tags {}")]
    [InlineData("--account account1 --pool pool1 --subscription sub --tags {}")]
    [InlineData("--account account1 --pool pool1 --resource-group rg --tags {}")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.UpdatePoolAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.Conflict,
                "backend payload containing internal pool metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--size", "8");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
