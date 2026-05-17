// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Search.Commands;
using Azure.Mcp.Tools.Search.Commands.Knowledge;
using Azure.Mcp.Tools.Search.Models;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Search.UnitTests.Knowledge;

public class KnowledgeSourceGetCommandTests : CommandUnitTestsBase<KnowledgeSourceGetCommand, ISearchService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsKnowledgeSources_WhenSourcesExist()
    {
        var expectedSources = new List<KnowledgeSourceInfo>
        {
            new("source1", "BlobSource", "First source"),
            new("source2", "IndexSource", "Second source")
        };

        Service.ListKnowledgeSources(
            Arg.Is("service123"),
            Arg.Is((string?)null),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedSources);

        var response = await ExecuteCommandAsync("--service", "service123");

        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.KnowledgeSourceGetCommandResult);

        Assert.Equal(expectedSources, result.KnowledgeSources);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSingleKnowledgeSource_WhenNameProvided()
    {
        var expectedSource = new KnowledgeSourceInfo("source1", "BlobSource", "First source");

        Service.ListKnowledgeSources(
            Arg.Is("service123"),
            Arg.Is("source1"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([expectedSource]);

        var response = await ExecuteCommandAsync("--service", "service123", "--knowledge-source", "source1");

        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.KnowledgeSourceGetCommandResult);

        Assert.Single(result.KnowledgeSources);
        Assert.Equal(expectedSource, result.KnowledgeSources[0]);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoSources()
    {
        Service.ListKnowledgeSources(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>()).Returns([]);

        var response = await ExecuteCommandAsync("--service", "service123");

        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.KnowledgeSourceGetCommandResult);

        Assert.Empty(result.KnowledgeSources);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        var serviceName = "service123";

        Service.ListKnowledgeSources(
            Arg.Is(serviceName),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service", serviceName);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
