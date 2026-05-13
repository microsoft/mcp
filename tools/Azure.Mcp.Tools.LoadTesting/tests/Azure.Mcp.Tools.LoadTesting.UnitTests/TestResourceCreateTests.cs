// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.LoadTesting.Commands;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.UnitTests;

public class TestResourceCreateCommandTests : CommandUnitTestsBase<TestResourceCreateCommand, ILoadTestingService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_CreateLoadTests()
    {
        var expectedLoadTests = new TestResource { Id = "Id1", Name = "loadTest1" };
        Service.CreateOrUpdateLoadTestingResourceAsync(
            Arg.Is("sub123"),
            Arg.Is("resourceGroup123"),
            Arg.Is("testResourceName"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedLoadTests);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--tenant", "tenant123");

        var result = ValidateAndDeserializeResponse(response, LoadTestJsonContext.Default.TestResourceCreateCommandResult);

        Assert.Equal("Id1", result.LoadTest.Id);
        Assert.Equal("loadTest1", result.LoadTest.Name);
    }


    [Fact]
    public async Task ExecuteAsync_CreateLoadTests_FromDefaultResource()
    {
        var expectedLoadTests = new TestResource { Id = "Id1", Name = "loadTest1" };
        Service.CreateOrUpdateLoadTestingResourceAsync(
            Arg.Is("sub123"),
            Arg.Is("resourceGroup123"),
            Arg.Is((string?)null),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedLoadTests);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--tenant", "tenant123");

        var result = ValidateAndDeserializeResponse(response, LoadTestJsonContext.Default.TestResourceCreateCommandResult);

        Assert.Equal("Id1", result.LoadTest.Id);
        Assert.Equal("loadTest1", result.LoadTest.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.CreateOrUpdateLoadTestingResourceAsync(
            Arg.Is("sub123"),
            Arg.Is("resourceGroup123"),
            Arg.Is("loadTestName"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "loadTestName",
            "--tenant", "tenant123");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
