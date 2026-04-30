// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.LoadTesting.Commands;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.UnitTests;

public class TestGetCommandTests : CommandUnitTestsBase<TestGetCommand, ILoadTestingService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsLoadTest_WhenExists()
    {
        var expected = new Test { TestId = "testId1", DisplayName = "TestDisplayName", Description = "TestDescription" };
        Service.GetTestAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId1"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId1",
            "--tenant", "tenant123");

        var result = ValidateAndDeserializeResponse(response, LoadTestJsonContext.Default.TestGetCommandResult);

        Assert.Equal(expected.TestId, result.Test.TestId);
        Assert.Equal(expected.DisplayName, result.Test.DisplayName);
        Assert.Equal(expected.Description, result.Test.Description);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestErrors()
    {
        Service.GetTestAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId1"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new Test());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--load-test-name", "loadTestName",
            "--tenant", "tenant123");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetTestAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId1"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId1",
            "--tenant", "tenant123");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
