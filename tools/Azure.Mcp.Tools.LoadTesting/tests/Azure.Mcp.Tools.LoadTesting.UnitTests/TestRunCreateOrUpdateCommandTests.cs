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

public class TestRunCreateOrUpdateCommandTests : CommandUnitTestsBase<TestRunCreateOrUpdateCommand, ILoadTestingService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("createorupdate", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateLoadTestRun_TestNotExisting()
    {
        var expected = new TestRun { TestId = "testId1", TestRunId = "testRunId1", DisplayName = "displayName" };
        Service.CreateOrUpdateLoadTestRunAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId1"),
            Arg.Is("run1"),
            Arg.Is((string?)null),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Is("displayName"),
            Arg.Is((string?)null),
            Arg.Is(false),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--testrun-id", "run1",
            "--tenant", "tenant123",
            "--test-id", "testId1",
            "--display-name", "displayName");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestErrors()
    {
        Service.CreateOrUpdateLoadTestRunAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId1"),
            Arg.Is("run1"),
            Arg.Is((string?)null),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Is((string?)null),
            Arg.Is((string?)null),
            Arg.Is(false),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new TestRun());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--tenant", "tenant123",
            "--testrun-id", "run1");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesLoadTestRun()
    {
        var expected = new TestRun { TestId = "testId1", TestRunId = "testRunId1", DisplayName = "displayName" };
        Service.CreateOrUpdateLoadTestRunAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId1"),
            Arg.Is("run1"),
            Arg.Is((string?)null),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Is("displayName"),
            Arg.Is((string?)null),
            Arg.Is(false),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--testrun-id", "run1",
            "--tenant", "tenant123",
            "--test-id", "testId1",
            "--display-name", "displayName");

        var result = ValidateAndDeserializeResponse(response, LoadTestJsonContext.Default.TestRunCreateOrUpdateCommandResult);

        Assert.Equal(expected.TestId, result.TestRun.TestId);
        Assert.Equal(expected.TestRunId, result.TestRun.TestRunId);
        Assert.Equal(expected.DisplayName, result.TestRun.DisplayName);
    }

    [Fact]
    public async Task ExecuteAsync_RerunLoadTestRun()
    {
        var expected = new TestRun { TestId = "testId1", TestRunId = "testRunId1" };
        Service.CreateOrUpdateLoadTestRunAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId1"),
            Arg.Is("run1"),
            Arg.Is("oldId1"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Is((string?)null),
            Arg.Is((string?)null),
            Arg.Is(false),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--testrun-id", "run1",
            "--tenant", "tenant123",
            "--test-id", "testId1",
            "--old-testrun-id", "oldId1");

        var result = ValidateAndDeserializeResponse(response, LoadTestJsonContext.Default.TestRunCreateOrUpdateCommandResult);

        Assert.Equal(expected.TestId, result.TestRun.TestId);
        Assert.Equal(expected.TestRunId, result.TestRun.TestRunId);
    }


    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.CreateOrUpdateLoadTestRunAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId1"),
            Arg.Is("run1"),
            Arg.Is((string?)null),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Is((string?)null),
            Arg.Is((string?)null),
            Arg.Is(false),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--testrun-id", "run1",
            "--tenant", "tenant123",
            "--test-id", "testId1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}

