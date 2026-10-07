// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Core.Models;
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
        var envelope = JsonSerializer.SerializeToElement(response, ModelsJsonContext.Default.CommandResponse);
        var page = envelope.GetProperty("results").GetProperty("results");
        Assert.Equal("next-page-token", page.GetProperty("continuationToken").GetString());
        var entry = Assert.Single(page.GetProperty("value").EnumerateArray());
        Assert.Equal("Sales Analytics", entry.GetProperty("hierarchy").GetProperty("workspace").GetProperty("displayName").GetString());
        await Service.Received(1).SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsInitialSearchOptionsToService()
    {
        CatalogSearchRequest? captured = null;
        Service.SearchCatalogAsync(Arg.Do<CatalogSearchRequest>(r => captured = r), Arg.Any<CancellationToken>())
            .Returns(new CatalogSearchResponse());

        var response = await ExecuteCommandAsync("--search", "Customer", "--filter", "Type eq 'Lakehouse'", "--page-size", "25");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Equal("Customer", captured!.Search);
        Assert.Equal("Type eq 'Lakehouse'", captured.Filter);
        Assert.Equal(25, captured.PageSize);
        Assert.Null(captured.ContinuationToken);
        await Service.Received(1).SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("25")]
    [InlineData("1000")]
    public async Task ExecuteAsync_ForwardsContinuationTokenWithoutSearchOrFilter(string? pageSize)
    {
        const string token = "raw+/%3D%G1";
        CatalogSearchRequest? captured = null;
        Service.SearchCatalogAsync(Arg.Do<CatalogSearchRequest>(request => captured = request), Arg.Any<CancellationToken>())
            .Returns(new CatalogSearchResponse { ContinuationToken = "next-page" });
        List<string> args = ["--continuation-token", token];
        if (pageSize is not null)
        {
            args.AddRange(["--page-size", pageSize]);
        }

        var response = await ExecuteCommandAsync([.. args]);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.CatalogSearchCommandResult);
        Assert.Empty(result.Results.Value);
        Assert.Equal("next-page", result.Results.ContinuationToken);
        Assert.NotNull(captured);
        Assert.Equal(token, captured.ContinuationToken);
        Assert.Null(captured.Search);
        Assert.Null(captured.Filter);
        Assert.Equal(pageSize is null ? null : int.Parse(pageSize), captured.PageSize);
        await Service.Received(1).SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--search", "Sales")]
    [InlineData("--filter", "Type eq 'Report'")]
    [InlineData("--search", "")]
    [InlineData("--filter", " ")]
    public async Task ExecuteAsync_RejectsContinuationWithSearchOrFilterBeforeService(string option, string value)
    {
        var response = await ExecuteCommandAsync("--continuation-token", "next-page", option, value);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--continuation-token must not be combined with --search or --filter", response.Message);
        Assert.Empty(Service.ReceivedCalls());
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
    [InlineData(HttpStatusCode.Conflict, "Check the catalog state before retrying")]
    [InlineData(HttpStatusCode.TooManyRequests, "Retry the request later")]
    [InlineData(HttpStatusCode.InternalServerError, "HTTP 500")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "HTTP 503")]
    public async Task ExecuteAsync_PreservesHttpStatusAndProvidesGuidance(HttpStatusCode status, string guidance)
    {
        Service.SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException(FabricCoreErrorTestData.PrivateDetails, null, status));

        var response = await ExecuteCommandAsync("--search", "Sales");

        Assert.Equal(status, response.Status);
        Assert.Contains(guidance, response.Message);
        Assert.DoesNotContain("Service unavailable or network connectivity issues", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        await Service.Received(1).SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PreservesNetworkFallback_WhenHttpStatusIsMissing()
    {
        Service.SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException(FabricCoreErrorTestData.PrivateDetails));

        var response = await ExecuteCommandAsync("--search", "Sales");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.Status);
        Assert.Contains("network connectivity", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        await Service.Received(1).SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("argument", HttpStatusCode.BadRequest)]
    [InlineData("configuration", HttpStatusCode.UnprocessableEntity)]
    [InlineData("json", HttpStatusCode.InternalServerError)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("task-canceled", HttpStatusCode.GatewayTimeout)]
    [InlineData("operation-canceled", HttpStatusCode.InternalServerError)]
    [InlineData("service", HttpStatusCode.Forbidden)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesOtherFailuresWithoutChangingStatus(string failure, HttpStatusCode status)
    {
        Service.SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(FabricCoreErrorTestData.CreateException(failure));

        var response = await ExecuteCommandAsync("--search", "Sales");

        Assert.Equal(status, response.Status);
        FabricCoreErrorTestData.AssertSanitized(response);
        await Service.Received(1).SearchCatalogAsync(Arg.Any<CatalogSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--page-size")]
    [InlineData("--unknown")]
    public async Task ExecuteAsync_SanitizesParserErrorsBeforeService(string option)
    {
        var response = await ExecuteCommandAsync(option, FabricCoreErrorTestData.PrivateDetails);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid catalog search request", response.Message);
        FabricCoreErrorTestData.AssertSanitized(response);
        Assert.Empty(Service.ReceivedCalls());
    }
}
