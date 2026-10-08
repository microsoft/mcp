// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests.SnapshotPolicy;

public class SnapshotPolicyGetCommandTests
    : SubscriptionCommandUnitTestsBase<SnapshotPolicyGetCommand, INetAppFilesSnapshotPolicyService>
{
    private static readonly NetAppFilesSnapshotPolicy ExistingPolicy = new(
        "policy1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/snapshotPolicies/policy1",
        "eastus",
        "Succeeded",
        true,
        5,
        2,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        new Dictionary<string, string> { ["environment"] = "test" });

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("get", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_GetsSnapshotPolicy()
    {
        Service.GetSnapshotPolicyAsync(
            "account1",
            "policy1",
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(ExistingPolicy);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--snapshot-policy", "policy1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.SnapshotPolicyGetResult);
        Assert.Equivalent(ExistingPolicy, result.SnapshotPolicy, strict: true);
    }

    [Theory]
    [InlineData("--account account/name --snapshot-policy policy1 --resource-group rg --subscription sub", "--account")]
    [InlineData("--account account1 --snapshot-policy _policy --resource-group rg --subscription sub", "--snapshot-policy")]
    [InlineData("--account account1 --snapshot-policy policy.name --resource-group rg --subscription sub", "--snapshot-policy")]
    public async Task ExecuteAsync_InvalidName_ReturnsBadRequest(string args, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_OversizedSnapshotPolicyName_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--snapshot-policy", new string('a', 65),
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("1-64", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--snapshot-policy policy1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --snapshot-policy policy1 --subscription sub")]
    [InlineData("--account account1 --snapshot-policy policy1 --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_SnapshotPolicyNotFound_ReturnsSafeMessage()
    {
        Service.GetSnapshotPolicyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.NotFound,
                "backend payload containing internal policy metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--snapshot-policy", "policy1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("snapshot policy was not found", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
