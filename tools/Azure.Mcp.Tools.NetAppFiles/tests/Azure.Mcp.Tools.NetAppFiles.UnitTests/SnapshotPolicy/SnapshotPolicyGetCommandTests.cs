// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
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

public class SnapshotPolicyGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<SnapshotPolicyGetCommand> _logger;
    private readonly SnapshotPolicyGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SnapshotPolicyGetCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<SnapshotPolicyGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoSnapshotPolicyParameter_ReturnsAllSnapshotPolicies()
    {
        // Arrange
        var subscription = "sub123";
        var expectedPolicies = new ResourceQueryResults<SnapshotPolicyInfo>(
        [
            new("account1/policy1", "eastus", "rg1", "Succeeded", true, 0, 5, 12, 0, 5, "Monday", 4, "1,15", 2),
            new("account1/policy2", "westus", "rg2", "Succeeded", false, 30, 3, 8, 30, 3, "Friday", 2, "1", 1)
        ], false);

        _netAppFilesService.GetSnapshotPolicyDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicies));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotPolicyGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.SnapshotPolicies);
        Assert.Equal(expectedPolicies.Results.Count, result.SnapshotPolicies.Count);
        Assert.Equal(expectedPolicies.Results.Select(p => p.Name), result.SnapshotPolicies.Select(p => p.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoSnapshotPolicies()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetSnapshotPolicyDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<SnapshotPolicyInfo>([], false));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotPolicyGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.SnapshotPolicies);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        _netAppFilesService.GetSnapshotPolicyDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --account myanfaccount", true)]
    [InlineData("--subscription sub123 --account myanfaccount --snapshotPolicy mypolicy", true)]
    [InlineData("--account myanfaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedPolicies = new ResourceQueryResults<SnapshotPolicyInfo>(
                [new("account1/policy1", "eastus", "rg1", "Succeeded", true, 0, 5, 12, 0, 5, "Monday", 4, "1,15", 2)],
                false);

            _netAppFilesService.GetSnapshotPolicyDetails(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedPolicies));
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
    public async Task ExecuteAsync_ReturnsSnapshotPolicyDetails_WhenPolicyExists()
    {
        // Arrange
        var account = "myanfaccount";
        var snapshotPolicy = "mypolicy";
        var subscription = "sub123";
        var expectedPolicies = new ResourceQueryResults<SnapshotPolicyInfo>(
            [new($"{account}/{snapshotPolicy}", "eastus", "rg1", "Succeeded", true, 0, 5, 12, 0, 5, "Monday", 4, "1,15", 2)],
            false);

        _netAppFilesService.GetSnapshotPolicyDetails(
            Arg.Is(account), Arg.Is(snapshotPolicy), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicies));

        var args = _commandDefinition.Parse(["--account", account, "--snapshotPolicy", snapshotPolicy, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotPolicyGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.SnapshotPolicies);
        Assert.Equal($"{account}/{snapshotPolicy}", result.SnapshotPolicies[0].Name);
        Assert.Equal("eastus", result.SnapshotPolicies[0].Location);
        Assert.Equal("rg1", result.SnapshotPolicies[0].ResourceGroup);
        Assert.True(result.SnapshotPolicies[0].Enabled);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetSnapshotPolicyDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var subscription = "sub123";
        var snapshotPolicy = "nonexistentpolicy";

        _netAppFilesService.GetSnapshotPolicyDetails(
            Arg.Any<string?>(), Arg.Is(snapshotPolicy), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Snapshot policy not found"));

        var parseResult = _commandDefinition.Parse(["--snapshotPolicy", snapshotPolicy, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Snapshot policy not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetSnapshotPolicyDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "sub123";
        var expectedPolicies = new ResourceQueryResults<SnapshotPolicyInfo>(
            [new("account1/policy1", "eastus", "rg1", "Succeeded", true, 0, 5, 12, 0, 5, "Monday", 4, "1,15", 2)],
            false);

        _netAppFilesService.GetSnapshotPolicyDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicies));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotPolicyGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.SnapshotPolicies);
        var policyInfo = result.SnapshotPolicies[0];
        Assert.Equal("account1/policy1", policyInfo.Name);
        Assert.Equal("eastus", policyInfo.Location);
        Assert.Equal("rg1", policyInfo.ResourceGroup);
        Assert.Equal("Succeeded", policyInfo.ProvisioningState);
        Assert.True(policyInfo.Enabled);
        Assert.Equal(0, policyInfo.HourlyScheduleMinute);
        Assert.Equal(5, policyInfo.HourlyScheduleSnapshotsToKeep);
        Assert.Equal(12, policyInfo.DailyScheduleHour);
        Assert.Equal(0, policyInfo.DailyScheduleMinute);
        Assert.Equal(5, policyInfo.DailyScheduleSnapshotsToKeep);
        Assert.Equal("Monday", policyInfo.WeeklyScheduleDay);
        Assert.Equal(4, policyInfo.WeeklyScheduleSnapshotsToKeep);
        Assert.Equal("1,15", policyInfo.MonthlyScheduleDaysOfMonth);
        Assert.Equal(2, policyInfo.MonthlyScheduleSnapshotsToKeep);
    }
}
