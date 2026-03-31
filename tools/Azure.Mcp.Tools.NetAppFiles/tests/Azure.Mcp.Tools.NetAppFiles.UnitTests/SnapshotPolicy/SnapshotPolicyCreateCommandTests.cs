// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
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
        _command = new(_logger);
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
    [InlineData("--snapshotPolicy mypolicy --resource-group myrg --location eastus --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --subscription sub123", false)] // Missing snapshotPolicy
    [InlineData("--account myanfaccount --snapshotPolicy mypolicy --location eastus --subscription sub123", false)] // Missing resource-group
    [InlineData("--account myanfaccount --snapshotPolicy mypolicy --resource-group myrg --subscription sub123", false)] // Missing location
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedPolicy = new SnapshotPolicyCreateResult(
                Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/snapshotPolicies/mypolicy",
                Name: "myanfaccount/mypolicy",
                Type: "Microsoft.NetApp/netAppAccounts/snapshotPolicies",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded",
                Enabled: true,
                HourlyScheduleMinute: null,
                HourlyScheduleSnapshotsToKeep: null,
                DailyScheduleHour: null,
                DailyScheduleMinute: null,
                DailyScheduleSnapshotsToKeep: null,
                WeeklyScheduleDay: null,
                WeeklyScheduleSnapshotsToKeep: null,
                MonthlyScheduleDaysOfMonth: null,
                MonthlyScheduleSnapshotsToKeep: null);

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
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedPolicy);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_CreatesSnapshotPolicy_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var snapshotPolicy = "mypolicy";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedPolicy = new SnapshotPolicyCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/snapshotPolicies/{snapshotPolicy}",
            Name: $"{account}/{snapshotPolicy}",
            Type: "Microsoft.NetApp/netAppAccounts/snapshotPolicies",
            Location: location,
            ResourceGroup: resourceGroup,
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

        _netAppFilesService.CreateSnapshotPolicy(
            Arg.Is(account), Arg.Is(snapshotPolicy), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicy));

        var args = _commandDefinition.Parse([
            "--account", account, "--snapshotPolicy", snapshotPolicy,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription,
            "--hourlyScheduleMinute", "0", "--hourlyScheduleSnapshotsToKeep", "5",
            "--dailyScheduleHour", "12", "--dailyScheduleMinute", "0", "--dailyScheduleSnapshotsToKeep", "5",
            "--weeklyScheduleDay", "Monday", "--weeklyScheduleSnapshotsToKeep", "4",
            "--monthlyScheduleDaysOfMonth", "1,15", "--monthlyScheduleSnapshotsToKeep", "2"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotPolicyCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.SnapshotPolicy);
        Assert.Equal($"{account}/{snapshotPolicy}", result.SnapshotPolicy.Name);
        Assert.Equal(location, result.SnapshotPolicy.Location);
        Assert.Equal(resourceGroup, result.SnapshotPolicy.ResourceGroup);
        Assert.Equal("Succeeded", result.SnapshotPolicy.ProvisioningState);
        Assert.True(result.SnapshotPolicy.Enabled);
        Assert.Equal(0, result.SnapshotPolicy.HourlyScheduleMinute);
        Assert.Equal(5, result.SnapshotPolicy.HourlyScheduleSnapshotsToKeep);
        Assert.Equal(12, result.SnapshotPolicy.DailyScheduleHour);
        Assert.Equal(0, result.SnapshotPolicy.DailyScheduleMinute);
        Assert.Equal(5, result.SnapshotPolicy.DailyScheduleSnapshotsToKeep);
        Assert.Equal("Monday", result.SnapshotPolicy.WeeklyScheduleDay);
        Assert.Equal(4, result.SnapshotPolicy.WeeklyScheduleSnapshotsToKeep);
        Assert.Equal("1,15", result.SnapshotPolicy.MonthlyScheduleDaysOfMonth);
        Assert.Equal(2, result.SnapshotPolicy.MonthlyScheduleSnapshotsToKeep);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesSnapshotPolicy_WithoutOptionalParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var snapshotPolicy = "mypolicy";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedPolicy = new SnapshotPolicyCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/snapshotPolicies/{snapshotPolicy}",
            Name: $"{account}/{snapshotPolicy}",
            Type: "Microsoft.NetApp/netAppAccounts/snapshotPolicies",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            Enabled: true,
            HourlyScheduleMinute: null,
            HourlyScheduleSnapshotsToKeep: null,
            DailyScheduleHour: null,
            DailyScheduleMinute: null,
            DailyScheduleSnapshotsToKeep: null,
            WeeklyScheduleDay: null,
            WeeklyScheduleSnapshotsToKeep: null,
            MonthlyScheduleDaysOfMonth: null,
            MonthlyScheduleSnapshotsToKeep: null);

        _netAppFilesService.CreateSnapshotPolicy(
            Arg.Is(account), Arg.Is(snapshotPolicy), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            null, null, null, null, null, null, null, null, null,
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicy));

        var args = _commandDefinition.Parse([
            "--account", account, "--snapshotPolicy", snapshotPolicy,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _netAppFilesService.CreateSnapshotPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflict()
    {
        // Arrange
        _netAppFilesService.CreateSnapshotPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Snapshot policy already exists"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        _netAppFilesService.CreateSnapshotPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Account not found"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--snapshotPolicy", "mypolicy",
            "--resource-group", "nonexistentrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        _netAppFilesService.CreateSnapshotPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _netAppFilesService.CreateSnapshotPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SnapshotPolicyCreateResult>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedPolicy = new SnapshotPolicyCreateResult(
            Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/snapshotPolicies/mypolicy",
            Name: "myanfaccount/mypolicy",
            Type: "Microsoft.NetApp/netAppAccounts/snapshotPolicies",
            Location: "westus2",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded",
            Enabled: true,
            HourlyScheduleMinute: 30,
            HourlyScheduleSnapshotsToKeep: 3,
            DailyScheduleHour: 8,
            DailyScheduleMinute: 15,
            DailyScheduleSnapshotsToKeep: 7,
            WeeklyScheduleDay: "Friday",
            WeeklyScheduleSnapshotsToKeep: 2,
            MonthlyScheduleDaysOfMonth: "1",
            MonthlyScheduleSnapshotsToKeep: 1);

        _netAppFilesService.CreateSnapshotPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicy));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--snapshotPolicy", "mypolicy",
            "--resource-group", "myrg", "--location", "westus2",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotPolicyCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.SnapshotPolicy);
        Assert.Equal("myanfaccount/mypolicy", result.SnapshotPolicy.Name);
        Assert.Equal("westus2", result.SnapshotPolicy.Location);
        Assert.Equal("myrg", result.SnapshotPolicy.ResourceGroup);
        Assert.Equal("Succeeded", result.SnapshotPolicy.ProvisioningState);
        Assert.Equal("Microsoft.NetApp/netAppAccounts/snapshotPolicies", result.SnapshotPolicy.Type);
        Assert.True(result.SnapshotPolicy.Enabled);
        Assert.Equal(30, result.SnapshotPolicy.HourlyScheduleMinute);
        Assert.Equal(3, result.SnapshotPolicy.HourlyScheduleSnapshotsToKeep);
        Assert.Equal(8, result.SnapshotPolicy.DailyScheduleHour);
        Assert.Equal(15, result.SnapshotPolicy.DailyScheduleMinute);
        Assert.Equal(7, result.SnapshotPolicy.DailyScheduleSnapshotsToKeep);
        Assert.Equal("Friday", result.SnapshotPolicy.WeeklyScheduleDay);
        Assert.Equal(2, result.SnapshotPolicy.WeeklyScheduleSnapshotsToKeep);
        Assert.Equal("1", result.SnapshotPolicy.MonthlyScheduleDaysOfMonth);
        Assert.Equal(1, result.SnapshotPolicy.MonthlyScheduleSnapshotsToKeep);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var snapshotPolicy = "mypolicy";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedPolicy = new SnapshotPolicyCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/snapshotPolicies/{snapshotPolicy}",
            Name: $"{account}/{snapshotPolicy}",
            Type: "Microsoft.NetApp/netAppAccounts/snapshotPolicies",
            Location: location,
            ResourceGroup: resourceGroup,
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

        _netAppFilesService.CreateSnapshotPolicy(
            account, snapshotPolicy, resourceGroup, location, subscription,
            0, 5, 12, 0, 5, "Monday", 4, "1,15", 2,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedPolicy);

        var args = _commandDefinition.Parse([
            "--account", account, "--snapshotPolicy", snapshotPolicy,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription,
            "--hourlyScheduleMinute", "0", "--hourlyScheduleSnapshotsToKeep", "5",
            "--dailyScheduleHour", "12", "--dailyScheduleMinute", "0", "--dailyScheduleSnapshotsToKeep", "5",
            "--weeklyScheduleDay", "Monday", "--weeklyScheduleSnapshotsToKeep", "4",
            "--monthlyScheduleDaysOfMonth", "1,15", "--monthlyScheduleSnapshotsToKeep", "2"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _netAppFilesService.Received(1).CreateSnapshotPolicy(
            account, snapshotPolicy, resourceGroup, location, subscription,
            0, 5, 12, 0, 5, "Monday", 4, "1,15", 2,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }
}
