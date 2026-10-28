// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Search.Commands;
using Azure.Mcp.Tools.Search.Commands.Index;
using Azure.Mcp.Tools.Search.Models;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Search.Tests.Index;

public class IndexGetCommandTests : CommandUnitTestsBase<IndexGetCommand, ISearchService>
{
    [Theory]
    [InlineData(null)]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    [InlineData("Target tenant")]
    public async Task ExecuteAsync_ForwardsTenant(string? tenant)
    {
        string[] args = ["--service", "service123", "--index", "index1"];
        var response = await ExecuteCommandAsync(tenant is null ? args : [.. args, "--tenant", tenant]);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Contains(CommandDefinition.Options, option => option.Name == "--tenant");
        await Service.Received(1).GetIndexDetails("service123", "index1", tenant, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    public async Task ExecuteAsync_PreservesServiceStatus(int status)
    {
        Service.GetIndexDetails("service123", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status, "Search request failed", "SearchError", null));

        var response = await ExecuteCommandAsync("--service", "service123");

        Assert.Equal((HttpStatusCode)status, response.Status);
        Assert.Contains("Search request failed", response.Message);
        if (status is 401 or 403)
        {
            Assert.Contains(SearchErrorMessages.Definitions, response.Message);
            Assert.Contains("--tenant", response.Message);
            Assert.Contains("do not require subscription-wide discovery permissions", response.Message);
            Assert.DoesNotContain("az login", response.Message);
        }
        else
        {
            Assert.DoesNotContain(SearchErrorMessages.Definitions, response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_PreservesTenantMismatchMessage_On401()
    {
        Service.GetIndexDetails("service123", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(401, "InvalidAuthenticationTokenTenant: token tenant mismatch"));

        var response = await ExecuteCommandAsync("--service", "service123");

        Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
        Assert.Contains("Authentication failed due to a tenant mismatch.", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsIndexes_WhenIndexesExist()
    {
        var expectedIndexes = new List<IndexInfo> {
            new("index1", null, null),
            new("index2", "This is the second index", null)
        };

        Service.GetIndexDetails(
            Arg.Is("service123"),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            null,
            Arg.Any<CancellationToken>())
            .Returns(expectedIndexes);

        var response = await ExecuteCommandAsync("--service", "service123");

        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.IndexGetCommandResult);

        Assert.Equal(expectedIndexes, result.Indexes);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoIndexes()
    {
        Service.GetIndexDetails(
            Arg.Any<string>(),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            null,
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--service", "service123");

        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.IndexGetCommandResult);

        Assert.Empty(result.Indexes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        var serviceName = "service123";

        Service.GetIndexDetails(
            Arg.Is(serviceName),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            null,
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service", serviceName);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsIndexDefinition_WhenIndexExists()
    {
        // Arrange
        var serviceName = "service123";
        var indexName = "index1";
        var expectedDefinition = CreateMockIndexDefinition();

        // When using ThrowsAsync or Returns with NSubstitute, we need to match the exact parameter signature
        Service.GetIndexDetails(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            null,
            Arg.Any<CancellationToken>())
            .Returns([expectedDefinition]);

        // Act
        var response = await ExecuteCommandAsync("--service", serviceName, "--index", indexName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.IndexGetCommandResult);

        Assert.NotNull(result.Indexes);
        Assert.Single(result.Indexes);
        Assert.Equal(expectedDefinition.Name, result.Indexes[0].Name);
        Assert.Equal(expectedDefinition.Fields?.Count, result.Indexes[0].Fields?.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenDefinitionIsNull()
    {
        // Arrange
        var serviceName = "service123";
        var indexName = "index1";

        Service.GetIndexDetails(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            null,
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--service", serviceName, "--index", indexName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.IndexGetCommandResult);

        Assert.Empty(result.Indexes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var expectedError = "Test error";
        var serviceName = "service123";
        var indexName = "index1";

        Service.GetIndexDetails(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            null,
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync("--service", serviceName, "--index", indexName);

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
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);

        // Verify options
        Assert.Contains(CommandDefinition.Options, o => o.Name == "--service");
        Assert.Contains(CommandDefinition.Options, o => o.Name == "--index");
    }

    private static IndexInfo CreateMockIndexDefinition()
        => new("sampleIndex", null, [
            new("id", "Edm.String", true, null, null, null, null, null),
            new("title", "Edm.String", null, true, null, null, null, null),
            new("content", "Edm.String", null, true, true, null, null, null)
        ]);
}
