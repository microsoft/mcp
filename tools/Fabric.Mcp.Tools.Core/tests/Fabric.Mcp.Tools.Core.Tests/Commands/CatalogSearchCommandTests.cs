// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class CatalogSearchCommandTests : CommandUnitTestsBase<CatalogSearchCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("search-catalog", Command.Name);
        Assert.Equal("Search Catalog", Command.Title);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.NotNull(Command.Description);
        Assert.NotEmpty(Command.Description);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("search-catalog", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new CatalogSearchCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFabricCoreServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new CatalogSearchCommand(Logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.False(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.True(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResults_WhenSearchSucceeds()
    {
        var expected = new CatalogSearchResponse
        {
            Value =
            [
                new CatalogEntry
                {
                    Id = "0acd697c-1550-43cd-b998-91bfb12347c6",
                    Type = "Report",
                    CatalogEntryType = "FabricItem",
                    DisplayName = "Monthly Sales Revenue",
                    Hierarchy = new CatalogEntryHierarchy
                    {
                        Workspace = new CatalogWorkspace { Id = "ws-1", DisplayName = "Sales Analytics" }
                    }
                }
            ],
            ContinuationToken = "next-page-token"
        };

        Service.SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--search", "Sales Revenue");

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.CatalogSearchCommandResult);
        Assert.Single(result.Results.Value);
        Assert.Equal("Monthly Sales Revenue", result.Results.Value[0].DisplayName);
        Assert.Equal("next-page-token", result.Results.ContinuationToken);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsAllOptionsToService()
    {
        CatalogSearchRequest? captured = null;
        Service.SearchCatalogAsync(Arg.Do<CatalogSearchRequest>(r => captured = r), Arg.Any<CancellationToken>())
            .Returns(new CatalogSearchResponse());

        await ExecuteCommandAsync("--search", "Customer", "--filter", "Type eq 'Lakehouse'", "--page-size", "25", "--continuation-token", "abc");

        Assert.NotNull(captured);
        Assert.Equal("Customer", captured!.Search);
        Assert.Equal("Type eq 'Lakehouse'", captured.Filter);
        Assert.Equal(25, captured.PageSize);
        Assert.Equal("abc", captured.ContinuationToken);
    }

    [Fact]
    public async Task ExecuteAsync_SearchesWithoutQuery_WhenSearchOmitted()
    {
        Service.SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CatalogSearchResponse());

        var response = await ExecuteCommandAsync(Array.Empty<string>());

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1001")]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenPageSizeOutOfRange(string pageSize)
    {
        var response = await ExecuteCommandAsync("--search", "Sales", "--page-size", pageSize);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1000")]
    public async Task ExecuteAsync_AcceptsBoundaryPageSizes(string pageSize)
    {
        Service.SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CatalogSearchResponse());

        var response = await ExecuteCommandAsync("--search", "Sales", "--page-size", pageSize);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "search criteria, filter, page size, and continuation token")]
    [InlineData(HttpStatusCode.Unauthorized, "configured Fabric identity")]
    [InlineData(HttpStatusCode.Forbidden, "supported and enabled for the tenant and capacity")]
    [InlineData(HttpStatusCode.NotFound, "catalog search resource was not found")]
    [InlineData(HttpStatusCode.Conflict, "Review the error details before retrying")]
    [InlineData(HttpStatusCode.TooManyRequests, "Retry the request later")]
    [InlineData(HttpStatusCode.InternalServerError, "HTTP 500")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "HTTP 503")]
    public async Task ExecuteAsync_PreservesHttpStatusAndProvidesGuidance(HttpStatusCode status, string guidance)
    {
        Service.SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("upstream-error", null, status));

        var response = await ExecuteCommandAsync("--search", "Sales");

        Assert.Equal(status, response.Status);
        Assert.Contains(guidance, response.Message);
        Assert.Contains("upstream-error", response.Message);
        Assert.DoesNotContain("Service unavailable or network connectivity issues", response.Message);
        Assert.NotNull(response.Results);
        await Service.Received(1).SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PreservesNetworkFallback_WhenHttpStatusIsMissing()
    {
        Service.SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("network-failure"));

        var response = await ExecuteCommandAsync("--search", "Sales");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.Status);
        Assert.StartsWith("Service unavailable or network connectivity issues. Details: network-failure", response.Message);
        await Service.Received(1).SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PreservesNonHttpErrorMapping()
    {
        Service.SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("configuration-error"));

        var response = await ExecuteCommandAsync("--search", "Sales");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.StartsWith("configuration-error.", response.Message);
        await Service.Received(1).SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>());
    }
}
