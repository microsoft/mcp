// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.SnapshotPolicy;

public class SnapshotPolicyUpdateCommandTests
{
    private const string SnapshotPolicyResourceId = "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/snapshotPolicies/mypolicy";

    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<SnapshotPolicyUpdateCommand> _logger;
    private readonly SnapshotPolicyUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SnapshotPolicyUpdateCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<SnapshotPolicyUpdateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger, _netAppFilesService);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --snapshotPolicy mypolicy --resource-group myrg --subscription sub123", true)]
    [InlineData("--snapshotPolicy mypolicy --resource-group myrg --subscription sub123", false)]
    [InlineData("--account myanfaccount --resource-group myrg --subscription sub123", false)]
    [InlineData("--account myanfaccount --snapshotPolicy mypolicy --subscription sub123", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _netAppFilesService.UpdateSnapshotPolicy(
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

        var parseResult = _commandDefinition.Parse(args);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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
        TestEnvironment.ClearAzureSubscriptionId();
        var expectedPolicy = BuildExpectedPolicy();

        _netAppFilesService.UpdateSnapshotPolicy(
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

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount",
            "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg",
            "--location", "eastus",
            "--subscription", "sub123",
            "--hourlyScheduleMinute", "5",
            "--hourlyScheduleSnapshotsToKeep", "3",
            "--dailyScheduleHour", "12",
            "--dailyScheduleMinute", "15",
            "--dailyScheduleSnapshotsToKeep", "7",
            "--weeklyScheduleDay", "Monday",
            "--weeklyScheduleHour", "6",
            "--weeklyScheduleMinute", "25",
            "--weeklyScheduleSnapshotsToKeep", "4",
            "--monthlyScheduleDaysOfMonth", "1,15",
            "--monthlyScheduleHour", "7",
            "--monthlyScheduleMinute", "35",
            "--monthlyScheduleSnapshotsToKeep", "2",
            "--enabled", "false",
            "--tags", "{\"env\":\"test\"}"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _netAppFilesService.Received(1).UpdateSnapshotPolicy(
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

    [Fact]
    public void BindOptions_AcceptsIds()
    {
        var args = _commandDefinition.Parse([
            "--ids", SnapshotPolicyResourceId,
            "--subscription", "sub123"
        ]);

        Assert.Empty(args.Errors);
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
        var parseResult = _commandDefinition.Parse($"--account myanfaccount --snapshotPolicy mypolicy --resource-group myrg --subscription sub123 {extraArgs}");

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedArgument, response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidTagsJson()
    {
        var args = _commandDefinition.Parse([
            "--account", "myanfaccount",
            "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg",
            "--subscription", "sub123",
            "--tags", "{invalid-json}"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid tags JSON format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        TestEnvironment.ClearAzureSubscriptionId();

        _netAppFilesService.UpdateSnapshotPolicy(
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

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount",
            "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg",
            "--subscription", "sub123"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        TestEnvironment.ClearAzureSubscriptionId();
        var expectedPolicy = BuildExpectedPolicy();

        _netAppFilesService.UpdateSnapshotPolicy(
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

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount",
            "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg",
            "--subscription", "sub123"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

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
    public void BindOptions_BindsOptionsCorrectly()
    {
        TestEnvironment.ClearAzureSubscriptionId();
        var args = _commandDefinition.Parse([
            "--account", "myanfaccount",
            "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg",
            "--subscription", "sub123",
            "--hourlyScheduleMinute", "15",
            "--hourlyScheduleSnapshotsToKeep", "3",
            "--dailyScheduleHour", "6",
            "--dailyScheduleMinute", "30",
            "--dailyScheduleSnapshotsToKeep", "7",
            "--weeklyScheduleDay", "Wednesday",
            "--weeklyScheduleHour", "5",
            "--weeklyScheduleMinute", "20",
            "--weeklyScheduleSnapshotsToKeep", "2",
            "--monthlyScheduleDaysOfMonth", "1,15",
            "--monthlyScheduleHour", "8",
            "--monthlyScheduleMinute", "45",
            "--monthlyScheduleSnapshotsToKeep", "1",
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
