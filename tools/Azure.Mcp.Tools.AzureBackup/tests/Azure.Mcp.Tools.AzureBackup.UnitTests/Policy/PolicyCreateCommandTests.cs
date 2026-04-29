// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Policy;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.Mcp.Tools.AzureBackup.Services.Policy;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Policy;

public class PolicyCreateCommandTests : CommandUnitTestsBase<PolicyCreateCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesPolicy_Successfully()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "Policy created successfully");

        Service.CreatePolicyAsync(
            Arg.Is<PolicyCreateRequest>(r => r.Policy == "myPolicy" && r.WorkloadType == "AzureIaasVM"),
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "myPolicy",
            "--workload-type", "AzureIaasVM",
            "--daily-retention-days", "30");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.PolicyCreateCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.CreatePolicyAsync(
            Arg.Is<PolicyCreateRequest>(r => r.Policy == "p" && r.WorkloadType == "AzureIaasVM"),
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p",
            "--workload-type", "AzureIaasVM",
            "--daily-retention-days", "30");

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
            Service.CreatePolicyAsync(
                Arg.Is<PolicyCreateRequest>(r => r.Policy == "p" && r.WorkloadType == "VM"),
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
                Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new OperationResult("Succeeded", null, null));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

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
        var options = CommandDefinition.Options;

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
        var command = Command.GetCommand();
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

        Service.CreatePolicyAsync(
            Arg.Any<PolicyCreateRequest>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p",
            "--workload-type", "VM",
            "--daily-retention-days", "30");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.PolicyCreateCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
        Assert.Equal("Policy created successfully", result.Result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        Service.CreatePolicyAsync(
            Arg.Is<PolicyCreateRequest>(r => r.Policy == "p" && r.WorkloadType == "VM"),
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p",
            "--workload-type", "VM",
            "--daily-retention-days", "30");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
