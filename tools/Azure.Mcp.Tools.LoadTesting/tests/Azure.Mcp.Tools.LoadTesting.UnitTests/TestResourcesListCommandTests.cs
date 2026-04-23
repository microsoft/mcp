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

public class TestResourceListCommandTests : CommandUnitTestsBase<TestResourceListCommand, ILoadTestingService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsLoadTests_FromResourceGroup()
    {
        var expectedLoadTests = new List<TestResource> { new() { Id = "Id1", Name = "loadTest1" }, new() { Id = "Id2", Name = "loadTest2" } };
        Service.GetLoadTestResourcesAsync(
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

        var result = ValidateAndDeserializeResponse(response, LoadTestJsonContext.Default.TestResourceListCommandResult);

        Assert.Equal(expectedLoadTests.Count, result.LoadTest.Count);
        Assert.Collection(result.LoadTest,
            item => Assert.Equal("Id1", item.Id),
            item => Assert.Equal("loadTest2", item.Name));
    }


    [Fact]
    public async Task ExecuteAsync_ReturnsLoadTests_FromTestResource()
    {
        var expectedLoadTests = new List<TestResource> { new() { Id = "Id1", Name = "loadTest1" } };
        Service.GetLoadTestResourcesAsync(
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

        var result = ValidateAndDeserializeResponse(response, LoadTestJsonContext.Default.TestResourceListCommandResult);

        Assert.Equal(expectedLoadTests.Count, result.LoadTest.Count);
        Assert.Collection(result.LoadTest, item => Assert.Equal("Id1", item.Id));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsLoadTests_WhenLoadTestsNotExist()
    {
        Service.GetLoadTestResourcesAsync(
            Arg.Is("sub123"),
            Arg.Is("resourceGroup123"),
            Arg.Is("loadTestName"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
             .Returns([]);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "loadTestName",
            "--tenant", "tenant123");

        var result = ValidateAndDeserializeResponse(response, LoadTestJsonContext.Default.TestResourceListCommandResult);
        Assert.Empty(result.LoadTest);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetLoadTestResourcesAsync(
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
