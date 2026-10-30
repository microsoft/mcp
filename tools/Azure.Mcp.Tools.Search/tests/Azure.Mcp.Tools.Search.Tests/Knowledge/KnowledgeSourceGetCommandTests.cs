// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Search.Commands;
using Azure.Mcp.Tools.Search.Commands.Knowledge;
using Azure.Mcp.Tools.Search.Models;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Search.Tests.Knowledge;

public class KnowledgeSourceGetCommandTests : CommandUnitTestsBase<KnowledgeSourceGetCommand, ISearchService>
{
    [Theory]
    [InlineData(null)]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    [InlineData("Target tenant")]
    public async Task ExecuteAsync_ForwardsTenant(string? tenant)
    {
        string[] args = ["--service", "service123", "--knowledge-source", "source1"];
        var response = await ExecuteCommandAsync(tenant is null ? args : [.. args, "--tenant", tenant]);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Contains(CommandDefinition.Options, option => option.Name == "--tenant");
        await Service.Received(1).ListKnowledgeSources("service123", "source1", tenant, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    public async Task ExecuteAsync_PreservesServiceStatus(int status)
    {
        Service.ListKnowledgeSources("service123", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status, "Search request failed"));

        var response = await ExecuteCommandAsync("--service", "service123");

        Assert.Equal((HttpStatusCode)status, response.Status);
        Assert.Contains("Search request failed", response.Message);
        if (status is 401 or 403)
        {
            Assert.Contains(SearchErrorMessages.Definitions, response.Message);
        }
        else
        {
            Assert.DoesNotContain(SearchErrorMessages.Definitions, response.Message);
        }
    }

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
            null,
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
            null,
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
            null,
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
            null,
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service", serviceName);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
