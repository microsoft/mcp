// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Search.Commands.Index;
using Azure.Mcp.Tools.Search.Options.Index;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Search.Tests.Index;

public class IndexQueryCommandTests : CommandUnitTestsBase<IndexQueryCommand, ISearchService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsResults_WhenSearchSucceeds()
    {
        // Arrange
        var serviceName = "service123";
        var indexName = "index1";
        var queryText = "test query";

        List<JsonElement> expectedResults = [
            JsonDocument.Parse(
                """
                {
                    "totalCount": 1,
                    "results": [
                        {
                            "id": "1",
                            "title": "Test Document"
                        }
                    ]
                }
                """
            ).RootElement
        ];

        Service.QueryIndex(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            Arg.Is(queryText),
            Arg.Any<IndexQueryType?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        // Act
        var response = await ExecuteCommandAsync("--service", serviceName, "--index", indexName, "--query", queryText);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        Assert.Contains("totalCount", json);
        Assert.Contains("results", json);
    }

    [Theory]
    [InlineData("simple", IndexQueryType.Simple)]
    [InlineData("full", IndexQueryType.Full)]
    [InlineData("semantic", IndexQueryType.Semantic)]
    public async Task ExecuteAsync_PassesQueryTypeToService(string queryTypeValue, IndexQueryType expectedQueryType)
    {
        // Arrange
        var serviceName = "service123";
        var indexName = "index1";
        var queryText = "test query";

        Service.QueryIndex(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            Arg.Is(queryText),
            Arg.Is<IndexQueryType?>(expectedQueryType),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--service", serviceName, "--index", indexName, "--query", queryText, "--query-type", queryTypeValue);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).QueryIndex(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            Arg.Is(queryText),
            Arg.Is<IndexQueryType?>(expectedQueryType),
            Arg.Is<string?>(s => s == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesSemanticConfigurationToService()
    {
        // Arrange
        var serviceName = "service123";
        var indexName = "index1";
        var queryText = "test query";
        var semanticConfiguration = "food-semantic";

        Service.QueryIndex(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            Arg.Is(queryText),
            Arg.Is<IndexQueryType?>(IndexQueryType.Semantic),
            Arg.Is<string?>(semanticConfiguration),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--service", serviceName,
            "--index", indexName,
            "--query", queryText,
            "--query-type", "semantic",
            "--semantic-configuration", semanticConfiguration);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).QueryIndex(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            Arg.Is(queryText),
            Arg.Is<IndexQueryType?>(IndexQueryType.Semantic),
            Arg.Is<string?>(semanticConfiguration),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_UsesNoQueryType_WhenNotSpecified()
    {
        // Arrange
        var serviceName = "service123";
        var indexName = "index1";
        var queryText = "test query";

        Service.QueryIndex(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            Arg.Is(queryText),
            Arg.Any<IndexQueryType?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--service", serviceName, "--index", indexName, "--query", queryText);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).QueryIndex(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            Arg.Is(queryText),
            Arg.Is<IndexQueryType?>(t => t == null),
            Arg.Is<string?>(s => s == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesQueryType()
    {
        // Act
        var response = await ExecuteCommandAsync(
            "--service", "service123", "--index", "index1", "--query", "test query", "--query-type", "invalid");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("query-type", response.Message ?? string.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var expectedError = "Test error";
        var serviceName = "service123";
        var indexName = "index1";
        var queryText = "test query";

        Service.QueryIndex(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            Arg.Is(queryText),
            Arg.Any<IndexQueryType?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync("--service", serviceName, "--index", indexName, "--query", queryText);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message ?? string.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesRequiredOptions()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync("");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("service", response.Message);
        Assert.Contains("index", response.Message);
        Assert.Contains("query", response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("query", CommandDefinition.Name);
        Assert.NotEmpty(CommandDefinition.Description ?? string.Empty);
    }
}
