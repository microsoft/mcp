// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Search.Commands.Index;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Search.UnitTests.Index;

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
            Arg.Any<RetryPolicyOptions?>(),
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
            Arg.Any<RetryPolicyOptions?>(),
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
