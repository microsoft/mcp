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

public class SnapshotPolicyCreateCommandTests
    : SubscriptionCommandUnitTestsBase<SnapshotPolicyCreateCommand, INetAppFilesSnapshotPolicyService>
{
    private static readonly NetAppFilesSnapshotPolicy CreatedPolicy = new(
        "policy1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/snapshotPolicies/policy1",
        "eastus",
        "Succeeded",
        true,
        5,
        6,
        2,
        10,
        7,
        "Monday,Friday",
        3,
        15,
        8,
        "1,15",
        4,
        20,
        9,
        new Dictionary<string, string> { ["environment"] = "test" });

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("create", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_CreatesSnapshotPolicyWithAllSchedules()
    {
        SnapshotPolicyCreateRequest? receivedRequest = null;
        Service.CreateSnapshotPolicyAsync(
            Arg.Do<SnapshotPolicyCreateRequest>(request => receivedRequest = request),
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(CreatedPolicy);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--snapshot-policy", "policy1",
            "--location", "eastus",
            "--enabled", "true",
            "--hourly-minute", "5",
            "--hourly-snapshots-to-keep", "6",
            "--daily-hour", "2",
            "--daily-minute", "10",
            "--daily-snapshots-to-keep", "7",
            "--weekly-day", "Monday,Friday",
            "--weekly-hour", "3",
            "--weekly-minute", "15",
            "--weekly-snapshots-to-keep", "8",
            "--monthly-days-of-month", "1,15",
            "--monthly-hour", "4",
            "--monthly-minute", "20",
            "--monthly-snapshots-to-keep", "9",
            "--tags", "{\"environment\":\"test\"}",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.SnapshotPolicyCreateResult);
        Assert.Equivalent(CreatedPolicy, result.SnapshotPolicy, strict: true);
        Assert.NotNull(receivedRequest);
        Assert.Equal("account1", receivedRequest.Account);
        Assert.Equal("policy1", receivedRequest.SnapshotPolicy);
        Assert.Equal(5, receivedRequest.HourlyMinute);
        Assert.Equal(7, receivedRequest.DailySnapshotsToKeep);
        Assert.Equal("Monday,Friday", receivedRequest.WeeklyDay);
        Assert.Equal("1,15", receivedRequest.MonthlyDaysOfMonth);
        Assert.Equal("test", receivedRequest.Tags!["environment"]);
    }

    [Fact]
    public async Task ExecuteAsync_MinimalHourlySchedule_CreatesSnapshotPolicy()
    {
        Service.CreateSnapshotPolicyAsync(
            Arg.Is<SnapshotPolicyCreateRequest>(request =>
                request.Enabled &&
                request.HourlyMinute == 5 &&
                request.HourlySnapshotsToKeep == 2 &&
                request.DailySnapshotsToKeep == null),
            "sub",
            null,
            Arg.Any<CancellationToken>())
            .Returns(CreatedPolicy);

        var response = await ExecuteCommandAsync(ValidHourlyArguments);

        ValidateAndDeserializeResponse(response, NetAppFilesJsonContext.Default.SnapshotPolicyCreateResult);
    }

    [Theory]
    [InlineData("--account account/name --snapshot-policy policy1 --location eastus --hourly-minute 5 --hourly-snapshots-to-keep 2 --resource-group rg --subscription sub", "--account")]
    [InlineData("--account account1 --snapshot-policy _policy --location eastus --hourly-minute 5 --hourly-snapshots-to-keep 2 --resource-group rg --subscription sub", "--snapshot-policy")]
    [InlineData("--account account1 --snapshot-policy policy.name --location eastus --hourly-minute 5 --hourly-snapshots-to-keep 2 --resource-group rg --subscription sub", "--snapshot-policy")]
    public async Task ExecuteAsync_InvalidName_ReturnsBadRequest(string args, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--hourly-minute 5", "hourly schedule")]
    [InlineData("--daily-hour 2 --daily-minute 5", "daily schedule")]
    [InlineData("--weekly-day Monday --weekly-hour 2 --weekly-minute 5", "weekly schedule")]
    [InlineData("--monthly-days-of-month 1 --monthly-hour 2 --monthly-minute 5", "monthly schedule")]
    public async Task ExecuteAsync_IncompleteSchedule_ReturnsBadRequest(string scheduleArgs, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(
            $"--account account1 --snapshot-policy policy1 --location eastus {scheduleArgs} --resource-group rg --subscription sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--hourly-minute 60 --hourly-snapshots-to-keep 2", "--hourly-minute")]
    [InlineData("--daily-hour 24 --daily-minute 5 --daily-snapshots-to-keep 2", "--daily-hour")]
    [InlineData("--weekly-day January --weekly-hour 2 --weekly-minute 5 --weekly-snapshots-to-keep 2", "--weekly-day")]
    [InlineData("--monthly-days-of-month 0,15 --monthly-hour 2 --monthly-minute 5 --monthly-snapshots-to-keep 2", "--monthly-days-of-month")]
    [InlineData("--monthly-days-of-month 1,,15 --monthly-hour 2 --monthly-minute 5 --monthly-snapshots-to-keep 2", "--monthly-days-of-month")]
    [InlineData("--hourly-minute 5 --hourly-snapshots-to-keep 0", "--hourly-snapshots-to-keep")]
    [InlineData("--hourly-minute 5 --hourly-snapshots-to-keep 256", "--hourly-snapshots-to-keep")]
    public async Task ExecuteAsync_InvalidScheduleValue_ReturnsBadRequest(string scheduleArgs, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(
            $"--account account1 --snapshot-policy policy1 --location eastus {scheduleArgs} --resource-group rg --subscription sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_NoSchedule_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--snapshot-policy", "policy1",
            "--location", "eastus",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("At least one", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidTags_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            $"{ValidHourlyArguments} --tags invalid-json");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--tags", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--snapshot-policy policy1 --location eastus --hourly-minute 5 --hourly-snapshots-to-keep 2 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --location eastus --hourly-minute 5 --hourly-snapshots-to-keep 2 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --snapshot-policy policy1 --hourly-minute 5 --hourly-snapshots-to-keep 2 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --snapshot-policy policy1 --location eastus --hourly-minute 5 --hourly-snapshots-to-keep 2 --subscription sub")]
    [InlineData("--account account1 --snapshot-policy policy1 --location eastus --hourly-minute 5 --hourly-snapshots-to-keep 2 --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.CreateSnapshotPolicyAsync(
            Arg.Any<SnapshotPolicyCreateRequest>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.Conflict,
                "backend payload containing internal policy metadata"));

        var response = await ExecuteCommandAsync(ValidHourlyArguments);

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }

    private const string ValidHourlyArguments =
        "--account account1 --snapshot-policy policy1 --location eastus --hourly-minute 5 --hourly-snapshots-to-keep 2 --resource-group rg --subscription sub";
}
