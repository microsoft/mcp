// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Commands.Search;
using Azure.Mcp.Tools.Adme.Models;
using Azure.Mcp.Tools.Adme.Models.Search;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Commands.Search;

public sealed class SearchCommandTests : CommandUnitTestsBase<SearchCommand, ISearchService>
{
    [Fact]
    public async Task Execute_WithoutPaginationOptions_UsesQueryApi()
    {
        SearchQueryRequest? captured = null;
        Service.QueryAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, Arg.Any<SearchQueryRequest>(),
                TestConstants.Tenant, Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                captured = callInfo.ArgAt<SearchQueryRequest>(2);
                return new AdmeResponse<SearchQueryResponse>(
                    new SearchQueryResponse { Results = [], TotalCount = 42 },
                    "test-correlation-id");
            });

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--tenant", TestConstants.Tenant,
            "--kind", "osdu:wks:*:*",
            "--query", "   ",
            "--limit", "100",
            "--returned-fields", "id");

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.AdmeResponseSearchResponse);
        Assert.Null(result.Result.Cursor);
        Assert.Equal(42, result.Result.TotalCount);
        Assert.Equal("test-correlation-id", result.CorrelationId);
        Assert.NotNull(captured);
        Assert.Null(captured.Query);
        Assert.Equal(100, captured.Limit);
        Assert.Equal(["id"], captured.ReturnedFields);
        await Service.DidNotReceiveWithAnyArgs().QueryWithCursorAsync(
            default!, default!, default!, default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Execute_WithCursorPaginationMode_UsesCursorApi()
    {
        Service.QueryWithCursorAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, Arg.Any<SearchCursorRequest>(),
                false, null, Arg.Any<CancellationToken>())
            .Returns(new AdmeResponse<SearchCursorResponse>(
                new SearchCursorResponse { Results = [], Cursor = "NEXT", TotalCount = 42 }, null));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--cursor-pagination-mode");

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.AdmeResponseSearchResponse);
        Assert.Equal("NEXT", result.Result.Cursor);
        Assert.Equal(42, result.Result.TotalCount);
        await Service.DidNotReceiveWithAnyArgs().QueryAsync(
            default!, default!, default!, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Execute_WithCursor_ForwardsContinuation()
    {
        Service.QueryWithCursorAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, Arg.Any<SearchCursorRequest>(),
                false, null, Arg.Any<CancellationToken>())
            .Returns(new AdmeResponse<SearchCursorResponse>(
                new SearchCursorResponse { Results = [], Cursor = "NEXT" }, null));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--cursor", "PREV");

        Assert.True(response.Status == System.Net.HttpStatusCode.OK, response.Message);
        ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.AdmeResponseSearchResponse);
        await Service.Received(1).QueryWithCursorAsync(
            TestConstants.Endpoint, TestConstants.DataPartition,
            Arg.Is<SearchCursorRequest>(request => request.Cursor == "PREV"),
            false, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WithZeroOffset_UsesQueryApi()
    {
        Service.QueryAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, Arg.Any<SearchQueryRequest>(),
                null, Arg.Any<CancellationToken>())
            .Returns(new AdmeResponse<SearchQueryResponse>(new SearchQueryResponse { Results = [] }, null));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--offset", "0");

        ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.AdmeResponseSearchResponse);
        await Service.Received(1).QueryAsync(
            TestConstants.Endpoint, TestConstants.DataPartition,
            Arg.Is<SearchQueryRequest>(request => request.Offset == 0),
            null, Arg.Any<CancellationToken>());
        await Service.DidNotReceiveWithAnyArgs().QueryWithCursorAsync(
            default!, default!, default!, default, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--offset", "5")]
    [InlineData("--aggregate-by", "kind")]
    public async Task Execute_WithQueryOnlyOption_UsesQueryApi(string option, string value)
    {
        Service.QueryAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, Arg.Any<SearchQueryRequest>(),
                null, Arg.Any<CancellationToken>())
            .Returns(new AdmeResponse<SearchQueryResponse>(new SearchQueryResponse
            {
                Results = [],
                TotalCount = 7,
                Aggregations = [new SearchAggregation { Key = "kind", Count = 7 }],
                PhraseSuggestions = ["well"],
            }, null));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            option, value);

        Assert.True(response.Status == System.Net.HttpStatusCode.OK, response.Message);
        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.AdmeResponseSearchResponse);
        Assert.Equal(7, result.Result.TotalCount);
        Assert.NotNull(result.Result.Aggregations);
        Assert.Equal(["well"], result.Result.PhraseSuggestions);
        await Service.DidNotReceiveWithAnyArgs().QueryWithCursorAsync(
            default!, default!, default!, default, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--offset", "1")]
    [InlineData("--aggregate-by", "kind")]
    public async Task Execute_WithCursorAndQueryOnlyOption_DoesNotCallService(string option, string value)
    {
        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--cursor", "NEXT",
            option, value);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().QueryAsync(
            default!, default!, default!, default, TestContext.Current.CancellationToken);
        await Service.DidNotReceiveWithAnyArgs().QueryWithCursorAsync(
            default!, default!, default!, default, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--offset", "1")]
    [InlineData("--aggregate-by", "kind")]
    public async Task Execute_WithCursorModeAndQueryOnlyOption_DoesNotCallService(string option, string value)
    {
        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--cursor-pagination-mode",
            option, value);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().QueryAsync(
            default!, default!, default!, default, TestContext.Current.CancellationToken);
        await Service.DidNotReceiveWithAnyArgs().QueryWithCursorAsync(
            default!, default!, default!, default, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--offset", "1")]
    [InlineData("--aggregate-by", "kind")]
    public async Task Execute_WithSearchAfterAndQueryOnlyOption_DoesNotCallService(string option, string value)
    {
        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--search-after",
            option, value);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().QueryAsync(
            default!, default!, default!, default, TestContext.Current.CancellationToken);
        await Service.DidNotReceiveWithAnyArgs().QueryWithCursorAsync(
            default!, default!, default!, default, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--limit", "0")]
    [InlineData("--limit", "1001")]
    [InlineData("--offset", "-1")]
    [InlineData("--offset", "9991")]
    [InlineData("--spatial-filter", "[]")]
    [InlineData("--kind", "garbage")]
    [InlineData("--sort", "null")]
    [InlineData("--sort", "{}")]
    [InlineData("--sort", "{\"field\":[\"id\"],\"order\":[\"SIDEWAYS\"]}")]
    public async Task Execute_WithInvalidInput_DoesNotCallService(string option, string value)
    {
        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            option, value);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().QueryAsync(
            default!, default!, default!, default, TestContext.Current.CancellationToken);
        await Service.DidNotReceiveWithAnyArgs().QueryWithCursorAsync(
            default!, default!, default!, default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Execute_WithLowercaseSortOrder_CallsService()
    {
        Service.QueryAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, Arg.Any<SearchQueryRequest>(),
                null, Arg.Any<CancellationToken>())
            .Returns(new AdmeResponse<SearchQueryResponse>(new SearchQueryResponse { Results = [] }, null));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--sort", "{\"field\":[\"id\"],\"order\":[\"asc\"]}");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
        await Service.Received(1).QueryAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            Arg.Is<SearchQueryRequest>(request => request.Sort!.Order[0] == "asc"),
            null,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Execute_WithSearchAfter_UsesCursorApi()
    {
        Service.QueryWithCursorAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, Arg.Any<SearchCursorRequest>(),
                true, null, Arg.Any<CancellationToken>())
            .Returns(new AdmeResponse<SearchCursorResponse>(
                new SearchCursorResponse { Results = [], Cursor = "NEXT", TotalCount = 42 }, null));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--search-after");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
        await Service.Received(1).QueryWithCursorAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            Arg.Any<SearchCursorRequest>(),
            true,
            null,
            TestContext.Current.CancellationToken);
        await Service.DidNotReceiveWithAnyArgs().QueryAsync(
            default!, default!, default!, default, TestContext.Current.CancellationToken);
    }
}
