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

public class PoolCreateCommandTests : SubscriptionCommandUnitTestsBase<PoolCreateCommand, INetAppFilesPoolService>
{
    private const long FourTebibytes = 4_398_046_511_104;

    private static readonly NetAppFilesPool CreatedPool = new(
        "pool1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1",
        "eastus",
        FourTebibytes,
        "Flexible",
        "Manual",
        true,
        "Double",
        128,
        new Dictionary<string, string> { ["environment"] = "test" },
        "Succeeded");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("create", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_CreatesPoolWithExpandedOptions()
    {
        Service.CreatePoolAsync(
            "account1",
            "pool1",
            FourTebibytes,
            "Flexible",
            "rg",
            "sub",
            "eastus",
            "Manual",
            true,
            "Double",
            128,
            Arg.Is<IReadOnlyDictionary<string, string>>(tags =>
                tags.Count == 1 && tags["environment"] == "test"),
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(CreatedPool);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--size", "4",
            "--service-level", "Flexible",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--location", "eastus",
            "--qos-type", "Manual",
            "--cool-access", "true",
            "--encryption-type", "Double",
            "--custom-throughput-mibps", "128",
            "--tags", "{\"environment\":\"test\"}",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(response, NetAppFilesJsonContext.Default.PoolCreateResult);
        Assert.Equal(CreatedPool.Name, result.Pool.Name);
        Assert.Equal(CreatedPool.Id, result.Pool.Id);
        Assert.Equal(CreatedPool.Location, result.Pool.Location);
        Assert.Equal(CreatedPool.SizeInBytes, result.Pool.SizeInBytes);
        Assert.Equal(CreatedPool.ServiceLevel, result.Pool.ServiceLevel);
        Assert.Equal(CreatedPool.QosType, result.Pool.QosType);
        Assert.Equal(CreatedPool.CoolAccessEnabled, result.Pool.CoolAccessEnabled);
        Assert.Equal(CreatedPool.EncryptionType, result.Pool.EncryptionType);
        Assert.Equal(CreatedPool.CustomThroughputMibps, result.Pool.CustomThroughputMibps);
        Assert.Equal("test", result.Pool.Tags["environment"]);
        Assert.Equal(CreatedPool.ProvisioningState, result.Pool.ProvisioningState);
    }

    [Theory]
    [InlineData("--pool pool1 --size 4 --service-level Premium --resource-group rg --subscription sub")]
    [InlineData("--account account1 --size 4 --service-level Premium --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --service-level Premium --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --size 4 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --size 4 --service-level Premium --subscription sub")]
    [InlineData("--account account1 --pool pool1 --size 4 --service-level Premium --resource-group rg")]
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
            "--size", "4",
            "--service-level", "Premium",
            "--resource-group", "rg",
            "--subscription", "sub");

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
            "--service-level", "Premium",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--size", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--service-level Archive", "--service-level")]
    [InlineData("--service-level Premium --qos-type Dynamic", "--qos-type")]
    [InlineData("--service-level Premium --encryption-type Triple", "--encryption-type")]
    public async Task ExecuteAsync_InvalidEnumValue_ReturnsBadRequest(string options, string expectedOption)
    {
        var response = await ExecuteCommandAsync(
            $"--account account1 --pool pool1 --size 4 --resource-group rg --subscription sub {options}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedOption, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--service-level Premium --qos-type Manual --custom-throughput-mibps 128")]
    [InlineData("--service-level Flexible --qos-type Auto --custom-throughput-mibps 128")]
    [InlineData("--service-level Flexible --qos-type Manual --custom-throughput-mibps 0")]
    public async Task ExecuteAsync_InvalidCustomThroughput_ReturnsBadRequest(string options)
    {
        var response = await ExecuteCommandAsync(
            $"--account account1 --pool pool1 --size 4 --resource-group rg --subscription sub {options}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--custom-throughput-mibps", response.Message);
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
            "--size", "4",
            "--service-level", "Premium",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tags", tags);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--tags", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.CreatePoolAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<long>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
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
            "--size", "4",
            "--service-level", "Premium",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
