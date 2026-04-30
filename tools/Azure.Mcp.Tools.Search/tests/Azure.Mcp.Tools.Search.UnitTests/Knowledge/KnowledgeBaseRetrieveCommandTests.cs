// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Search.Commands.Knowledge;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Search.UnitTests.Knowledge;

public class KnowledgeBaseRetrieveCommandTests : CommandUnitTestsBase<KnowledgeBaseRetrieveCommand, ISearchService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsResult_WhenSuccessful_WithQuery()
    {
        var json = "{\"answer\":\"42\"}";
        Service.RetrieveFromKnowledgeBase(
            Arg.Is("svc"),
            Arg.Is("base1"),
            Arg.Is("life"),
            Arg.Is<List<(string role, string message)>?>(m => m == null),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(json);

        var response = await ExecuteCommandAsync(
            "--service", "svc",
            "--knowledge-base", "base1",
            "--query", "life");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResult_WhenSuccessful_WithMessages()
    {
        var json = "{\"conversation\":true}";
        Service.RetrieveFromKnowledgeBase(
            Arg.Is("svc"),
            Arg.Is("base1"),
            Arg.Is<string?>(q => q == null),
            Arg.Is<List<(string role, string message)>?>(m => m != null && m.Count == 1 && m[0].role == "user"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(json);

        var response = await ExecuteCommandAsync(
            "--service", "svc",
            "--knowledge-base", "base1",
            "--messages", "user:Hello");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenMissingQueryAndMessages()
    {
        var response = await ExecuteCommandAsync("--service", "svc", "--knowledge-base", "base1");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Either --query or at least one --messages", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenHasBothQueryAndMessages()
    {
        var response = await ExecuteCommandAsync(
            "--service", "svc",
            "--knowledge-base", "base1",
            "--query", "life",
            "--messages", "user:Hello");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Specifying both --query and --messages is not allowed.", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenMessageFormatInvalid()
    {
        var response = await ExecuteCommandAsync(
            "--service", "svc",
            "--knowledge-base", "base1",
            "--messages", "bad-format");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid message format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        Service.RetrieveFromKnowledgeBase(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<List<(string role, string message)>?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test failure"));

        var response = await ExecuteCommandAsync(
            "--service", "svc",
            "--knowledge-base", "base1",
            "--query", "hi");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test failure", response.Message);
    }
}
