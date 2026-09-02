// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tests.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.SnapshotPolicy;

public class SnapshotPolicyUpdateCommandTests : SubscriptionCommandUnitTestsBase<SnapshotPolicyUpdateCommand, INetAppFilesService>
{
    private const string SnapshotPolicyResourceId = "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/snapshotPolicies/mypolicy";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --snapshot-policy mypolicy --resource-group myrg --subscription sub123", true)]
    [InlineData("--snapshot-policy mypolicy --resource-group myrg --subscription sub123", false)]
    [InlineData("--account myanfaccount --resource-group myrg --subscription sub123", false)]
    [InlineData("--account myanfaccount --snapshot-policy mypolicy --subscription sub123", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.UpdateSnapshotPolicy(
                default!,
                default!,
                default!,
                default,
                default!,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                TestContext.Current.CancellationToken)
                .ReturnsForAnyArgs(BuildExpectedPolicy());
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.Equal("Success", response.Message);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.True(
                response.Message.Contains("provided", StringComparison.OrdinalIgnoreCase) ||
                response.Message.Contains("required", StringComparison.OrdinalIgnoreCase),
                $"Expected a validation message, got: {response.Message}");
        }
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesSnapshotPolicy_WithExpandedParameters()
    {
        var expectedPolicy = BuildExpectedPolicy();

        Service.UpdateSnapshotPolicy(
            Arg.Is("myanfaccount"),
            Arg.Is("mypolicy"),
            Arg.Is("myrg"),
            Arg.Is("eastus"),
            Arg.Is("sub123"),
            Arg.Is(5),
            Arg.Is(3),
            Arg.Is(12),
            Arg.Is(15),
            Arg.Is(7),
            Arg.Is("Monday"),
            Arg.Is(4),
            Arg.Is("1,15"),
            Arg.Is(2),
            Arg.Is(false),
            Arg.Is(6),
            Arg.Is(25),
            Arg.Is(7),
            Arg.Is(35),
            Arg.Is<Dictionary<string, string>?>(tags => tags != null && tags.Count == 1 && tags["env"] == "test"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedPolicy);

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount",
            "--snapshot-policy", "mypolicy",
            "--resource-group", "myrg",
            "--location", "eastus",
            "--subscription", "sub123",
            "--hourly-schedule-minute", "5",
            "--hourly-schedule-snapshots-to-keep", "3",
            "--daily-schedule-hour", "12",
            "--daily-schedule-minute", "15",
            "--daily-schedule-snapshots-to-keep", "7",
            "--weekly-schedule-day", "Monday",
            "--weekly-schedule-hour", "6",
            "--weekly-schedule-minute", "25",
            "--weekly-schedule-snapshots-to-keep", "4",
            "--monthly-schedule-days-of-month", "1,15",
            "--monthly-schedule-hour", "7",
            "--monthly-schedule-minute", "35",
            "--monthly-schedule-snapshots-to-keep", "2",
            "--enabled", "false",
            "--tags", "{\"env\":\"test\"}"
        ]);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateSnapshotPolicy(
            "myanfaccount",
            "mypolicy",
            "myrg",
            "eastus",
            "sub123",
            5,
            3,
            12,
            15,
            7,
            "Monday",
            4,
            "1,15",
            2,
            false,
            6,
            25,
            7,
            35,
            Arg.Is<Dictionary<string, string>?>(tags => tags != null && tags.Count == 1 && tags["env"] == "test"),
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--no-wait", "no-wait")]
    [InlineData("--acquirePolicyToken", "acquirePolicyToken")]
    [InlineData("--changeReference CR-123", "changeReference")]
    [InlineData("--add properties.enabled=false", "add")]
    [InlineData("--set properties.enabled=false", "set")]
    [InlineData("--remove properties.weeklySchedule", "remove")]
    [InlineData("--force-string", "force-string")]
    public async Task ExecuteAsync_RejectsUnsupportedArguments(string extraArgs, string expectedArgument)
    {
        var response = await ExecuteCommandAsync($"--account myanfaccount --snapshot-policy mypolicy --resource-group myrg --subscription sub123 {extraArgs}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedArgument, response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidTagsJson()
    {
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount",
            "--snapshot-policy", "mypolicy",
            "--resource-group", "myrg",
            "--subscription", "sub123",
            "--tags", "{invalid-json}"
        ]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid tags JSON format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        Service.UpdateSnapshotPolicy(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount",
            "--snapshot-policy", "mypolicy",
            "--resource-group", "myrg",
            "--subscription", "sub123"
        ]);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var expectedPolicy = BuildExpectedPolicy();

        Service.UpdateSnapshotPolicy(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedPolicy);

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount",
            "--snapshot-policy", "mypolicy",
            "--resource-group", "myrg",
            "--subscription", "sub123"
        ]);

        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotPolicyUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.SnapshotPolicy);
        Assert.Equal("myanfaccount/mypolicy", result.SnapshotPolicy.Name);
        Assert.Equal("eastus", result.SnapshotPolicy.Location);
        Assert.Equal("myrg", result.SnapshotPolicy.ResourceGroup);
        Assert.True(result.SnapshotPolicy.Enabled);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        // Act
        var args = CommandDefinition.Parse([
            "--account", "myanfaccount",
            "--snapshot-policy", "mypolicy",
            "--resource-group", "myrg",
            "--subscription", "sub123",
            "--hourly-schedule-minute", "15",
            "--hourly-schedule-snapshots-to-keep", "3",
            "--daily-schedule-hour", "6",
            "--daily-schedule-minute", "30",
            "--daily-schedule-snapshots-to-keep", "7",
            "--weekly-schedule-day", "Wednesday",
            "--weekly-schedule-hour", "5",
            "--weekly-schedule-minute", "20",
            "--weekly-schedule-snapshots-to-keep", "2",
            "--monthly-schedule-days-of-month", "1,15",
            "--monthly-schedule-hour", "8",
            "--monthly-schedule-minute", "45",
            "--monthly-schedule-snapshots-to-keep", "1",
            "--enabled", "true",
            "--tags", "{\"env\":\"test\"}"
        ]);

        Assert.Empty(args.Errors);
    }

    private static SnapshotPolicyCreateResult BuildExpectedPolicy()
    {
        return new SnapshotPolicyCreateResult(
            Id: SnapshotPolicyResourceId,
            Name: "myanfaccount/mypolicy",
            Type: "Microsoft.NetApp/netAppAccounts/snapshotPolicies",
            Location: "eastus",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded",
            Enabled: true,
            HourlyScheduleMinute: 0,
            HourlyScheduleSnapshotsToKeep: 5,
            DailyScheduleHour: 12,
            DailyScheduleMinute: 0,
            DailyScheduleSnapshotsToKeep: 5,
            WeeklyScheduleDay: "Monday",
            WeeklyScheduleSnapshotsToKeep: 4,
            MonthlyScheduleDaysOfMonth: "1,15",
            MonthlyScheduleSnapshotsToKeep: 2);
    }
}
