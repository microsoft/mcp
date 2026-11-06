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

public class KnowledgeBaseGetCommandTests : CommandUnitTestsBase<KnowledgeBaseGetCommand, ISearchService>
{
    [Theory]
    [InlineData(null)]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    [InlineData("Target tenant")]
    public async Task ExecuteAsync_ForwardsTenant(string? tenant)
    {
        string[] args = ["--service", "service123", "--knowledge-base", "base1"];
        var response = await ExecuteCommandAsync(tenant is null ? args : [.. args, "--tenant", tenant]);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Contains(CommandDefinition.Options, option => option.Name == "--tenant");
        await Service.Received(1).ListKnowledgeBases("service123", "base1", tenant, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    public async Task ExecuteAsync_PreservesServiceStatus(int status)
    {
        Service.ListKnowledgeBases("service123", null, null, Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_ReturnsKnowledgeBases_WhenBasesExist()
    {
        var expectedBases = new List<KnowledgeBaseInfo>
        {
            new("base1", "First base", ["source1"]),
            new("base2", "Second base", ["source2", "source3"])
        };

        Service.ListKnowledgeBases(
            Arg.Is("service123"),
            Arg.Is((string?)null),
            null,
            Arg.Any<CancellationToken>())
            .Returns(expectedBases);

        var response = await ExecuteCommandAsync("--service", "service123");

        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.KnowledgeBaseGetCommandResult);

        Assert.Equal(expectedBases.Count, result.KnowledgeBases.Count);
        for (int i = 0; i < expectedBases.Count; i++)
        {
            Assert.Equal(expectedBases[i].Name, result.KnowledgeBases[i].Name);
            Assert.Equal(expectedBases[i].Description, result.KnowledgeBases[i].Description);
            Assert.Equal(expectedBases[i].KnowledgeSources, result.KnowledgeBases[i].KnowledgeSources);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSingleKnowledgeBase_WhenNameProvided()
    {
        var expectedBase = new KnowledgeBaseInfo("base1", "First base", ["source1"]);

        Service.ListKnowledgeBases(
            Arg.Is("service123"),
            Arg.Is("base1"),
            null,
            Arg.Any<CancellationToken>())
            .Returns([expectedBase]);

        var response = await ExecuteCommandAsync("--service", "service123", "--knowledge-base", "base1");

        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.KnowledgeBaseGetCommandResult);

        Assert.Single(result.KnowledgeBases);
        Assert.Equal(expectedBase.Name, result.KnowledgeBases[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoBases()
    {
        Service.ListKnowledgeBases(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            null,
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--service", "service123");

        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.KnowledgeBaseGetCommandResult);

        Assert.Empty(result.KnowledgeBases);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        var serviceName = "service123";

        Service.ListKnowledgeBases(
            Arg.Is(serviceName),
            Arg.Any<string?>(),
            null,
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service", "service123");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
