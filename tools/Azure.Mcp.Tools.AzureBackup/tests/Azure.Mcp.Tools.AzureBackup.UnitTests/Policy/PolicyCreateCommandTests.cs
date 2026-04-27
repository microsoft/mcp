// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Policy;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.Mcp.Tools.AzureBackup.Services.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Policy;

public class PolicyCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureBackupService _backupService;
    private readonly ILogger<PolicyCreateCommand> _logger;
    private readonly PolicyCreateCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    public PolicyCreateCommandTests()
    {
        _backupService = Substitute.For<IAzureBackupService>();
        _logger = Substitute.For<ILogger<PolicyCreateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_backupService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger, _backupService);
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

    [Fact]
    public async Task ExecuteAsync_CreatesPolicy_Successfully()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "Policy created successfully");

        _backupService.CreatePolicyAsync(
            Arg.Is<PolicyCreateRequest>(r => r.Policy == "myPolicy" && r.WorkloadType == "AzureIaasVM"),
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--policy", "myPolicy", "--workload-type", "AzureIaasVM", "--daily-retention-days", "30"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.PolicyCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        _backupService.CreatePolicyAsync(
            Arg.Is<PolicyCreateRequest>(r => r.Policy == "p" && r.WorkloadType == "AzureIaasVM"),
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--policy", "p", "--workload-type", "AzureIaasVM", "--daily-retention-days", "30"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --policy p --workload-type VM --daily-retention-days 30", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // Missing policy and workload-type
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _backupService.CreatePolicyAsync(
                Arg.Is<PolicyCreateRequest>(r => r.Policy == "p" && r.WorkloadType == "VM"),
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
                Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new OperationResult("Succeeded", null, null)));
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var command = _command.GetCommand();
        var options = command.Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--policy");
        Assert.Contains(options, o => o.Name == "--workload-type");
        Assert.Contains(options, o => o.Name == "--daily-retention-days");
        // Common schedule flags added by the policy create overhaul.
        Assert.Contains(options, o => o.Name == "--time-zone");
        Assert.Contains(options, o => o.Name == "--schedule-frequency");
        Assert.Contains(options, o => o.Name == "--schedule-times");
        Assert.Contains(options, o => o.Name == "--schedule-days-of-week");
        Assert.Contains(options, o => o.Name == "--hourly-interval-hours");
        Assert.Contains(options, o => o.Name == "--hourly-window-start-time");
        Assert.Contains(options, o => o.Name == "--hourly-window-duration-hours");
        // Retention flags added by the policy create overhaul.
        Assert.Contains(options, o => o.Name == "--weekly-retention-weeks");
        Assert.Contains(options, o => o.Name == "--weekly-retention-days-of-week");
        Assert.Contains(options, o => o.Name == "--monthly-retention-months");
        Assert.Contains(options, o => o.Name == "--monthly-retention-week-of-month");
        Assert.Contains(options, o => o.Name == "--monthly-retention-days-of-week");
        Assert.Contains(options, o => o.Name == "--monthly-retention-days-of-month");
        Assert.Contains(options, o => o.Name == "--yearly-retention-years");
        Assert.Contains(options, o => o.Name == "--yearly-retention-months");
        Assert.Contains(options, o => o.Name == "--yearly-retention-week-of-month");
        Assert.Contains(options, o => o.Name == "--yearly-retention-days-of-week");
        Assert.Contains(options, o => o.Name == "--yearly-retention-days-of-month");
        Assert.Contains(options, o => o.Name == "--archive-tier-after-days");
        Assert.Contains(options, o => o.Name == "--archive-tier-mode");
        // RSV-VM only.
        Assert.Contains(options, o => o.Name == "--policy-sub-type");
        Assert.Contains(options, o => o.Name == "--instant-rp-retention-days");
        Assert.Contains(options, o => o.Name == "--instant-rp-resource-group");
        Assert.Contains(options, o => o.Name == "--snapshot-consistency");
        // RSV-VmWorkload.
        Assert.Contains(options, o => o.Name == "--full-schedule-frequency");
        Assert.Contains(options, o => o.Name == "--full-schedule-days-of-week");
        Assert.Contains(options, o => o.Name == "--differential-schedule-days-of-week");
        Assert.Contains(options, o => o.Name == "--differential-retention-days");
        Assert.Contains(options, o => o.Name == "--incremental-schedule-days-of-week");
        Assert.Contains(options, o => o.Name == "--incremental-retention-days");
        Assert.Contains(options, o => o.Name == "--log-frequency-minutes");
        Assert.Contains(options, o => o.Name == "--log-retention-days");
        Assert.Contains(options, o => o.Name == "--is-compression");
        Assert.Contains(options, o => o.Name == "--is-sql-compression");
    }

    [Fact]
    public void BindOptions_AllPolicyCreateOverhaulOptionsRegistered()
    {
        // Sanity check: the full set of new policy-create flags from the overhaul plan is registered.
        var command = _command.GetCommand();
        var optionNames = command.Options.Select(o => o.Name).ToHashSet();

        var expected = new[]
        {
            // common schedule
            "--time-zone", "--schedule-frequency", "--schedule-times", "--schedule-days-of-week",
            "--hourly-interval-hours", "--hourly-window-start-time", "--hourly-window-duration-hours",
            // retention
            "--weekly-retention-weeks", "--weekly-retention-days-of-week",
            "--monthly-retention-months", "--monthly-retention-week-of-month",
            "--monthly-retention-days-of-week", "--monthly-retention-days-of-month",
            "--yearly-retention-years", "--yearly-retention-months",
            "--yearly-retention-week-of-month", "--yearly-retention-days-of-week",
            "--yearly-retention-days-of-month",
            "--archive-tier-after-days", "--archive-tier-mode",
            // RSV-VM
            "--policy-sub-type", "--instant-rp-retention-days",
            "--instant-rp-resource-group", "--snapshot-consistency",
            // RSV-VmWorkload
            "--full-schedule-frequency", "--full-schedule-days-of-week",
            "--differential-schedule-days-of-week", "--differential-retention-days",
            "--incremental-schedule-days-of-week", "--incremental-retention-days",
            "--log-frequency-minutes", "--log-retention-days",
            "--is-compression", "--is-sql-compression",
        };

        var missing = expected.Where(n => !optionNames.Contains(n)).ToList();
        Assert.Empty(missing);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "Policy created successfully");

        _backupService.CreatePolicyAsync(
            Arg.Any<PolicyCreateRequest>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--policy", "p", "--workload-type", "VM", "--daily-retention-days", "30"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.PolicyCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Succeeded", result.Result.Status);
        Assert.Equal("Policy created successfully", result.Result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        _backupService.CreatePolicyAsync(
            Arg.Is<PolicyCreateRequest>(r => r.Policy == "p" && r.WorkloadType == "VM"),
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--policy", "p", "--workload-type", "VM", "--daily-retention-days", "30"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
