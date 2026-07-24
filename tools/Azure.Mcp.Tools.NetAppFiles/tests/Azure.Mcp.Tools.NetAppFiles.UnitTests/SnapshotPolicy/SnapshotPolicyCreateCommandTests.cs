// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.SnapshotPolicy;

public class SnapshotPolicyCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<SnapshotPolicyCreateCommand> _logger;
    private readonly SnapshotPolicyCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SnapshotPolicyCreateCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<SnapshotPolicyCreateCommand>>();

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
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --snapshotPolicy mypolicy --resource-group myrg --location eastus --subscription sub123", true)]
    [InlineData("--snapshotPolicy mypolicy --resource-group myrg --location eastus --subscription sub123", false)]
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --subscription sub123", false)]
    [InlineData("--account myanfaccount --snapshotPolicy mypolicy --location eastus --subscription sub123", false)]
    [InlineData("--account myanfaccount --snapshotPolicy mypolicy --resource-group myrg --subscription sub123", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedPolicy = BuildExpectedPolicy();
            _netAppFilesService.CreateSnapshotPolicy(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
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
            Assert.Contains("required", response.Message.ToLowerInvariant());
        }
    }

    [Fact]
    public async Task ExecuteAsync_CreatesSnapshotPolicy_WithExpandedParameters()
    {
        TestEnvironment.ClearAzureSubscriptionId();
        var expectedPolicy = BuildExpectedPolicy();

        _netAppFilesService.CreateSnapshotPolicy(
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
            Arg.Is(true),
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
            "--enabled", "true",
            "--tags", "{\"env\":\"test\"}"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _netAppFilesService.Received(1).CreateSnapshotPolicy(
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
            true,
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
    public async Task ExecuteAsync_ParsesLegacyScheduleNames()
    {
        TestEnvironment.ClearAzureSubscriptionId();
        var expectedPolicy = BuildExpectedPolicy();

        _netAppFilesService.CreateSnapshotPolicy(
            Arg.Is("myanfaccount"),
            Arg.Is("mypolicy"),
            Arg.Is("myrg"),
            Arg.Is("eastus"),
            Arg.Is("sub123"),
            Arg.Is(0),
            Arg.Is(5),
            Arg.Is(8),
            Arg.Is(10),
            Arg.Is(4),
            Arg.Is("Friday"),
            Arg.Is(2),
            Arg.Is("1"),
            Arg.Is(1),
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
            "--location", "eastus",
            "--subscription", "sub123",
            "--hourlyScheduleMinute", "0",
            "--hourlyScheduleSnapshotsToKeep", "5",
            "--dailyScheduleHour", "8",
            "--dailyScheduleMinute", "10",
            "--dailyScheduleSnapshotsToKeep", "4",
            "--weeklyScheduleDay", "Friday",
            "--weeklyScheduleSnapshotsToKeep", "2",
            "--monthlyScheduleDaysOfMonth", "1",
            "--monthlyScheduleSnapshotsToKeep", "1"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        TestEnvironment.ClearAzureSubscriptionId();
        _netAppFilesService.CreateSnapshotPolicy(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
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
            "--account", "myanfaccount", "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg", "--location", "eastus", "--subscription", "sub123"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUnsupportedAcquirePolicyToken()
    {
        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg", "--location", "eastus", "--subscription", "sub123",
            "--acquirePolicyToken"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("acquirePolicyToken", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUnsupportedChangeReference()
    {
        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg", "--location", "eastus", "--subscription", "sub123",
            "--changeReference", "CR-123"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("changeReference", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidTagsJson()
    {
        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg", "--location", "eastus", "--subscription", "sub123",
            "--tags", "{invalid-json}"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid tags JSON format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        TestEnvironment.ClearAzureSubscriptionId();
        var expectedPolicy = BuildExpectedPolicy();

        _netAppFilesService.CreateSnapshotPolicy(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
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
            "--account", "myanfaccount", "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg", "--location", "eastus", "--subscription", "sub123"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotPolicyCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.SnapshotPolicy);
        Assert.Equal("myanfaccount/mypolicy", result.SnapshotPolicy.Name);
        Assert.Equal("eastus", result.SnapshotPolicy.Location);
        Assert.Equal("myrg", result.SnapshotPolicy.ResourceGroup);
    }

    private static SnapshotPolicyCreateResult BuildExpectedPolicy()
    {
        return new SnapshotPolicyCreateResult(
            Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/snapshotPolicies/mypolicy",
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
