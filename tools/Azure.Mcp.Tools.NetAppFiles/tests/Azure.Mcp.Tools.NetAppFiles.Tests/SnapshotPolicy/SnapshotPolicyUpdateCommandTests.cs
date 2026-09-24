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

public class SnapshotPolicyUpdateCommandTests
    : SubscriptionCommandUnitTestsBase<SnapshotPolicyUpdateCommand, INetAppFilesSnapshotPolicyService>
{
    private static readonly NetAppFilesSnapshotPolicy UpdatedPolicy = new(
        "policy1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/snapshotPolicies/policy1",
        "eastus",
        "Succeeded",
        false,
        10,
        3,
        2,
        15,
        7,
        "Monday,Friday",
        3,
        20,
        8,
        "1,15",
        4,
        25,
        9,
        new Dictionary<string, string> { ["environment"] = "test" });

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("update", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesSnapshotPolicyWithAllProperties()
    {
        SnapshotPolicyUpdateRequest? receivedRequest = null;
        Service.UpdateSnapshotPolicyAsync(
            Arg.Do<SnapshotPolicyUpdateRequest>(request => receivedRequest = request),
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(UpdatedPolicy);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--snapshot-policy", "policy1",
            "--location", "eastus",
            "--enabled", "false",
            "--hourly-minute", "10",
            "--hourly-snapshots-to-keep", "3",
            "--daily-hour", "2",
            "--daily-minute", "15",
            "--daily-snapshots-to-keep", "7",
            "--weekly-day", "Monday,Friday",
            "--weekly-hour", "3",
            "--weekly-minute", "20",
            "--weekly-snapshots-to-keep", "8",
            "--monthly-days-of-month", "1,15",
            "--monthly-hour", "4",
            "--monthly-minute", "25",
            "--monthly-snapshots-to-keep", "9",
            "--tags", "{\"environment\":\"test\"}",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.SnapshotPolicyUpdateResult);
        Assert.Equivalent(UpdatedPolicy, result.SnapshotPolicy, strict: true);
        Assert.NotNull(receivedRequest);
        Assert.Equal("eastus", receivedRequest.Location);
        Assert.False(receivedRequest.Enabled);
        Assert.Equal(10, receivedRequest.HourlyMinute);
        Assert.Equal(7, receivedRequest.DailySnapshotsToKeep);
        Assert.Equal("Monday,Friday", receivedRequest.WeeklyDay);
        Assert.Equal("1,15", receivedRequest.MonthlyDaysOfMonth);
        Assert.Equal("test", receivedRequest.Tags!["environment"]);
    }

    [Fact]
    public async Task ExecuteAsync_EnabledOnly_ForwardsSparseRequest()
    {
        Service.UpdateSnapshotPolicyAsync(
            Arg.Is<SnapshotPolicyUpdateRequest>(request =>
                request.Enabled == false &&
                request.Location == null &&
                request.HourlyMinute == null &&
                request.Tags == null),
            "sub",
            null,
            Arg.Any<CancellationToken>())
            .Returns(UpdatedPolicy);

        var response = await ExecuteCommandAsync(ValidEnabledArguments);

        ValidateAndDeserializeResponse(response, NetAppFilesJsonContext.Default.SnapshotPolicyUpdateResult);
    }

    [Fact]
    public async Task ExecuteAsync_CompleteSchedule_ForwardsOnlyThatSchedule()
    {
        Service.UpdateSnapshotPolicyAsync(
            Arg.Is<SnapshotPolicyUpdateRequest>(request =>
                request.HourlyMinute == 10 &&
                request.HourlySnapshotsToKeep == 3 &&
                request.Enabled == null),
            "sub",
            null,
            Arg.Any<CancellationToken>())
            .Returns(UpdatedPolicy);

        var response = await ExecuteCommandAsync(
            "--account account1 --snapshot-policy policy1 --hourly-minute 10 --hourly-snapshots-to-keep 3 --resource-group rg --subscription sub");

        ValidateAndDeserializeResponse(response, NetAppFilesJsonContext.Default.SnapshotPolicyUpdateResult);
    }

    [Theory]
    [InlineData("--hourly-minute 10", "hourly schedule")]
    [InlineData("--daily-hour 2 --daily-minute 15", "daily schedule")]
    [InlineData("--weekly-day Monday --weekly-hour 3 --weekly-minute 20", "weekly schedule")]
    [InlineData("--monthly-days-of-month 1,15 --monthly-hour 4 --monthly-minute 25", "monthly schedule")]
    public async Task ExecuteAsync_IncompleteSchedule_ReturnsBadRequest(string updateArgs, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(
            $"--account account1 --snapshot-policy policy1 {updateArgs} --resource-group rg --subscription sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--account account/name --snapshot-policy policy1 --enabled false --resource-group rg --subscription sub", "--account")]
    [InlineData("--account account1 --snapshot-policy _policy --enabled false --resource-group rg --subscription sub", "--snapshot-policy")]
    [InlineData("--account account1 --snapshot-policy policy.name --enabled false --resource-group rg --subscription sub", "--snapshot-policy")]
    public async Task ExecuteAsync_InvalidName_ReturnsBadRequest(string args, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_NoUpdateProperty_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account account1 --snapshot-policy policy1 --resource-group rg --subscription sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("At least one update property", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--hourly-minute 60", "--hourly-minute")]
    [InlineData("--daily-hour 24", "--daily-hour")]
    [InlineData("--weekly-day January", "--weekly-day")]
    [InlineData("--monthly-days-of-month 0,15", "--monthly-days-of-month")]
    [InlineData("--monthly-days-of-month 1,,15", "--monthly-days-of-month")]
    [InlineData("--hourly-snapshots-to-keep 0", "--hourly-snapshots-to-keep")]
    [InlineData("--hourly-snapshots-to-keep 256", "--hourly-snapshots-to-keep")]
    public async Task ExecuteAsync_InvalidScheduleValue_ReturnsBadRequest(string updateArgs, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(
            $"--account account1 --snapshot-policy policy1 {updateArgs} --resource-group rg --subscription sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidTags_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account account1 --snapshot-policy policy1 --tags invalid-json --resource-group rg --subscription sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--tags", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyTags_ForwardsEmptyDictionary()
    {
        Service.UpdateSnapshotPolicyAsync(
            Arg.Is<SnapshotPolicyUpdateRequest>(request => request.Tags != null && request.Tags.Count == 0),
            "sub",
            null,
            Arg.Any<CancellationToken>())
            .Returns(UpdatedPolicy);

        var response = await ExecuteCommandAsync(
            "--account account1 --snapshot-policy policy1 --tags {} --resource-group rg --subscription sub");

        ValidateAndDeserializeResponse(response, NetAppFilesJsonContext.Default.SnapshotPolicyUpdateResult);
    }

    [Theory]
    [InlineData("--snapshot-policy policy1 --enabled false --resource-group rg --subscription sub")]
    [InlineData("--account account1 --enabled false --resource-group rg --subscription sub")]
    [InlineData("--account account1 --snapshot-policy policy1 --enabled false --subscription sub")]
    [InlineData("--account account1 --snapshot-policy policy1 --enabled false --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.UpdateSnapshotPolicyAsync(
            Arg.Any<SnapshotPolicyUpdateRequest>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.Conflict,
                "backend payload containing internal policy metadata"));

        var response = await ExecuteCommandAsync(ValidEnabledArguments);

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ReturnsSafeMessage()
    {
        Service.UpdateSnapshotPolicyAsync(
            Arg.Any<SnapshotPolicyUpdateRequest>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.NotFound,
                "backend payload containing internal policy metadata"));

        var response = await ExecuteCommandAsync(ValidEnabledArguments);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("snapshot policy was not found", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }

    private const string ValidEnabledArguments =
        "--account account1 --snapshot-policy policy1 --enabled false --resource-group rg --subscription sub";
}
