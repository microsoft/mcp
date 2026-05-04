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

public class TestCreateCommandTests : CommandUnitTestsBase<TestCreateCommand, ILoadTestingService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_CreateLoadTest_WhenExists()
    {
        var expected = new Test { TestId = "testId1", DisplayName = "TestDisplayName", Description = "TestDescription" };
        Service.CreateTestAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId1"), Arg.Is("resourceGroup123"),
            Arg.Is("TestDisplayName"), Arg.Is("TestDescription"),
            Arg.Is((int?)20), Arg.Is((int?)50), Arg.Is((int?)1), Arg.Is("https://example.com/api/test"),
            Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId1",
            "--tenant", "tenant123",
            "--display-name", "TestDisplayName",
            "--description", "TestDescription",
            "--duration", "20",
            "--virtual-users", "50",
            "--ramp-up-time", "1",
            "--endpoint", "https://example.com/api/test");

        // Assert
        var result = ValidateAndDeserializeResponse(response, LoadTestJsonContext.Default.TestCreateCommandResult);

        Assert.Equal(expected.TestId, result.Test.TestId);
        Assert.Equal(expected.DisplayName, result.Test.DisplayName);
        Assert.Equal(expected.Description, result.Test.Description);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestErrors()
    {
        Service.CreateTestAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId1"), Arg.Is("resourceGroup123"),
            Arg.Is("TestDisplayName"), Arg.Is("TestDescription"),
            Arg.Is((int?)20), Arg.Is((int?)50), Arg.Is((int?)1), Arg.Is((string?)null),
            Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new Test());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--tenant", "tenant123");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.CreateTestAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId1"), Arg.Is("resourceGroup123"),
            Arg.Is("TestDisplayName"), Arg.Is("TestDescription"),
            Arg.Is((int?)20), Arg.Is((int?)50), Arg.Is((int?)1), Arg.Is("https://example.com/api/test"),
            Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId1",
            "--tenant", "tenant123",
            "--display-name", "TestDisplayName",
            "--description", "TestDescription",
            "--duration", "20",
            "--virtual-users", "50",
            "--ramp-up-time", "1",
            "--endpoint", "https://example.com/api/test");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
