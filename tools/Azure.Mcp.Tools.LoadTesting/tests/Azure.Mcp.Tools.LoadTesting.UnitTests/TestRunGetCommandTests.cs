// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.LoadTesting.Commands;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.UnitTests;

public class TestRunGetCommandTests : CommandUnitTestsBase<TestRunGetCommand, ILoadTestingService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsLoadTestRun_WhenExists()
    {
        var expected = new TestRun { TestId = "testId1", TestRunId = "testRunId1" };
        Service.GetLoadTestRunAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("run1"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--testrun-id", "run1",
            "--tenant", "tenant123");

        var result = ValidateAndDeserializeResponse(response, LoadTestJsonContext.Default.TestRunGetCommandResult);

        Assert.NotNull(result.TestRuns);
        Assert.Single(result.TestRuns);
        Assert.Equal(expected.TestId, result.TestRuns.First().TestId);
        Assert.Equal(expected.TestRunId, result.TestRuns.First().TestRunId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestErrors()
    {
        Service.GetLoadTestRunAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("run1"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new TestRun());

        var response = await ExecuteCommandAsync("--tenant", "tenant123");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetLoadTestRunAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("run1"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--testrun-id", "run1",
            "--tenant", "tenant123");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsLoadTestRuns_WhenTestIdProvided()
    {
        var expected = new List<TestRun>
        {
            new() { TestId = "testId1", TestRunId = "testRunId1" },
            new() { TestId = "testId2", TestRunId = "testRunId2" }
        };
        Service.GetLoadTestRunsFromTestIdAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId",
            "--tenant", "tenant123");

        var result = ValidateAndDeserializeResponse(response, LoadTestJsonContext.Default.TestRunGetCommandResult);

        Assert.NotNull(result.TestRuns);
        Assert.Equal(2, result.TestRuns.Count);
        Assert.Equal(expected.First().TestId, result.TestRuns.First().TestId);
        Assert.Equal(expected.First().TestRunId, result.TestRuns.First().TestRunId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesListServiceErrors()
    {
        Service.GetLoadTestRunsFromTestIdAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId",
            "--tenant", "tenant123");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
