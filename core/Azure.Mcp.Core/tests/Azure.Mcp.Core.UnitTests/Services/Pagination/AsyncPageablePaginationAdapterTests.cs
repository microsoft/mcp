// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Azure.Mcp.Core.Services.Pagination;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Pagination;

public class AsyncPageablePaginationAdapterTests
{
    private static readonly Func<string, string> s_converter = s => s.ToUpperInvariant();

    [Fact]
    public void Provider_ReturnsAsyncPageable()
    {
        var adapter = CreateAdapter(CreatePageable([], null));
        Assert.Equal("asyncpageable", adapter.Provider);
    }

    [Fact]
    public async Task FetchFirstPageAsync_ReturnsConvertedItems()
    {
        var adapter = CreateAdapter(CreatePageable(["alpha", "beta", "gamma"], null));

        var result = await adapter.FetchFirstPageAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["ALPHA", "BETA", "GAMMA"], result.Items);
        Assert.Null(result.NativeState);
    }

    [Fact]
    public async Task FetchFirstPageAsync_EmptyResults_ReturnsEmptyList()
    {
        var adapter = CreateAdapter(CreatePageable([], null));

        var result = await adapter.FetchFirstPageAsync(TestContext.Current.CancellationToken);

        Assert.Empty(result.Items);
        Assert.Null(result.NativeState);
    }

    [Fact]
    public async Task FetchFirstPageAsync_WithContinuationToken_ReturnsToken()
    {
        var adapter = CreateAdapter(CreatePageable(["a", "b"], "token-1"));

        var result = await adapter.FetchFirstPageAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["A", "B"], result.Items);
        Assert.Equal("token-1", result.NativeState);
    }

    [Fact]
    public async Task FetchNextPageAsync_PassesContinuationToken()
    {
        string? capturedToken = null;
        int? capturedPageSize = null;

        var adapter = new AsyncPageablePaginationAdapter<string, string>(
            (token, size) =>
            {
                capturedToken = token;
                capturedPageSize = size;
                var pageable = CreatePageable(["c", "d"], null);
                return Task.FromResult(new AsyncPageablePaginationAdapter<string, string>.PageableResult(pageable));
            },
            s_converter,
            pageSize: 5);

        var result = await adapter.FetchNextPageAsync("token-1", TestContext.Current.CancellationToken);

        Assert.Equal("token-1", capturedToken);
        Assert.Equal(5, capturedPageSize);
        Assert.Equal(["C", "D"], result.Items);
        Assert.Null(result.NativeState);
    }

    [Fact]
    public async Task FetchNextPageAsync_NullState_ThrowsArgumentNullException()
    {
        var adapter = CreateAdapter(CreatePageable([], null));

        await Assert.ThrowsAsync<ArgumentNullException>(() => adapter.FetchNextPageAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FetchNextPageAsync_EmptyState_ThrowsArgumentException()
    {
        var adapter = CreateAdapter(CreatePageable([], null));

        await Assert.ThrowsAsync<ArgumentException>(() => adapter.FetchNextPageAsync("", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FetchFirstPageAsync_OnlyFetchesOnePage()
    {
        // Pageable yields two pages, but adapter should only consume the first
        var pageable = new TestAsyncPageable([
            Page<string>.FromValues(["a"], "token-1", Substitute.For<Response>()),
            Page<string>.FromValues(["b"], null, Substitute.For<Response>()),
        ]);
        var adapter = CreateAdapter(pageable);

        var result = await adapter.FetchFirstPageAsync(TestContext.Current.CancellationToken);

        Assert.Single(result.Items);
        Assert.Equal("A", result.Items[0]);
        Assert.Equal("token-1", result.NativeState);
    }

    [Fact]
    public async Task FetchFirstPageAsync_PassesPageSizeToFactory()
    {
        int? capturedPageSize = null;

        var adapter = new AsyncPageablePaginationAdapter<string, string>(
            (token, size) =>
            {
                capturedPageSize = size;
                var pageable = CreatePageable(["x"], null);
                return Task.FromResult(new AsyncPageablePaginationAdapter<string, string>.PageableResult(pageable));
            },
            s_converter,
            pageSize: 25);

        await adapter.FetchFirstPageAsync(TestContext.Current.CancellationToken);

        Assert.Equal(25, capturedPageSize);
    }

    [Fact]
    public async Task FetchFirstPageAsync_PassesNullTokenToFactory()
    {
        string? capturedToken = "not-null";

        var adapter = new AsyncPageablePaginationAdapter<string, string>(
            (token, size) =>
            {
                capturedToken = token;
                var pageable = CreatePageable([], null);
                return Task.FromResult(new AsyncPageablePaginationAdapter<string, string>.PageableResult(pageable));
            },
            s_converter);

        await adapter.FetchFirstPageAsync(TestContext.Current.CancellationToken);

        Assert.Null(capturedToken);
    }

    [Fact]
    public async Task FetchNextPageAsync_MultiplePages_ReturnsSecondPageItems()
    {
        var calls = 0;
        var adapter = new AsyncPageablePaginationAdapter<string, string>(
            (token, size) =>
            {
                calls++;
                var pageable = calls == 1
                    ? CreatePageable(["first"], "next-token")
                    : CreatePageable(["second"], null);
                return Task.FromResult(new AsyncPageablePaginationAdapter<string, string>.PageableResult(pageable));
            },
            s_converter);

        var firstResult = await adapter.FetchFirstPageAsync(TestContext.Current.CancellationToken);
        Assert.Equal("next-token", firstResult.NativeState);

        var secondResult = await adapter.FetchNextPageAsync(firstResult.NativeState!, TestContext.Current.CancellationToken);
        Assert.Equal(["SECOND"], secondResult.Items);
        Assert.Null(secondResult.NativeState);
    }

    private static AsyncPageablePaginationAdapter<string, string> CreateAdapter(AsyncPageable<string> pageable) =>
        new(
            (_, _) => Task.FromResult(new AsyncPageablePaginationAdapter<string, string>.PageableResult(pageable)),
            s_converter);

    private static AsyncPageable<string> CreatePageable(IReadOnlyList<string> items, string? continuationToken)
    {
        return new TestAsyncPageable([
            Page<string>.FromValues(items, continuationToken, Substitute.For<Response>()),
        ]);
    }

    /// <summary>
    /// A test <see cref="AsyncPageable{T}"/> that returns pages as-is regardless of
    /// the continuation token passed to <see cref="AsPages"/>.
    /// This avoids <c>AsyncPageable.FromPages</c> which filters pages by continuation token.
    /// </summary>
    private sealed class TestAsyncPageable(IReadOnlyList<Page<string>> pages) : AsyncPageable<string>
    {
        public override async IAsyncEnumerable<Page<string>> AsPages(
            string? continuationToken = null,
            int? pageSizeHint = null)
        {
            foreach (var page in pages)
            {
                yield return page;
            }

            await Task.CompletedTask;
        }
    }
}
