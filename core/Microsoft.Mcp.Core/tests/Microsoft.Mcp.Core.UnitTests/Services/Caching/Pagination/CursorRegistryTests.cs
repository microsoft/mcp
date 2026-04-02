// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Services.Caching;
using Microsoft.Mcp.Core.Services.Caching.Pagination;
using NSubstitute;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Services.Caching.Pagination;

public class CursorRegistryTests
{
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly CursorRegistry _registry;

    public CursorRegistryTests()
    {
        _registry = new CursorRegistry(_cacheService);
    }

    private static CursorRecord CreateRecord(string? cursorId = null, DateTimeOffset? expiresAt = null) => new()
    {
        CursorId = cursorId ?? CursorRegistry.GenerateCursorId(),
        Provider = "arm",
        Operation = "resourcegroup.list",
        RequestFingerprint = "sha256:abc123",
        CallerBinding = new CallerBinding { Mode = "hostIdentity" },
        NativeState = "https://management.azure.com/next",
        CreatedAtUtc = DateTimeOffset.UtcNow,
        ExpiresAtUtc = expiresAt ?? DateTimeOffset.UtcNow.AddHours(1),
    };

    [Fact]
    public async Task SetAsync_StoresRecordAndReturnsCursorId()
    {
        var ct = TestContext.Current.CancellationToken;
        var record = CreateRecord();

        var result = await _registry.SetAsync(record, ct);

        Assert.Equal(record.CursorId, result);
        await _cacheService.Received(1).SetAsync(
            CursorRegistry.CacheGroup,
            record.CursorId,
            record,
            Arg.Is<TimeSpan?>(t => t!.Value > TimeSpan.Zero),
            ct);
    }

    [Fact]
    public async Task SetAsync_ThrowsForExpiredRecord()
    {
        var ct = TestContext.Current.CancellationToken;
        var record = CreateRecord(expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));

        await Assert.ThrowsAsync<ArgumentException>(() => _registry.SetAsync(record, ct).AsTask());
    }

    [Fact]
    public async Task SetAsync_ThrowsForNullRecord()
    {
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ArgumentNullException>(() => _registry.SetAsync(null!, ct).AsTask());
    }

    [Fact]
    public async Task GetAsync_ReturnsCachedRecord()
    {
        var ct = TestContext.Current.CancellationToken;
        var record = CreateRecord();
        _cacheService.GetAsync<CursorRecord>(CursorRegistry.CacheGroup, record.CursorId, cancellationToken: ct)
            .Returns(new ValueTask<CursorRecord?>(record));

        var result = await _registry.GetAsync(record.CursorId, ct);

        Assert.NotNull(result);
        Assert.Equal(record.CursorId, result.CursorId);
        Assert.Equal(record.Provider, result.Provider);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullForUnknownCursor()
    {
        var ct = TestContext.Current.CancellationToken;
        _cacheService.GetAsync<CursorRecord>(CursorRegistry.CacheGroup, "cur_unknown", cancellationToken: ct)
            .Returns(new ValueTask<CursorRecord?>(default(CursorRecord)));

        var result = await _registry.GetAsync("cur_unknown", ct);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_ThrowsForNullCursorId()
    {
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ArgumentNullException>(() => _registry.GetAsync(null!, ct).AsTask());
    }

    [Fact]
    public async Task GetAsync_ThrowsForEmptyCursorId()
    {
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ArgumentException>(() => _registry.GetAsync("", ct).AsTask());
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToCacheService()
    {
        var ct = TestContext.Current.CancellationToken;
        var cursorId = CursorRegistry.GenerateCursorId();

        await _registry.DeleteAsync(cursorId, ct);

        await _cacheService.Received(1).DeleteAsync(CursorRegistry.CacheGroup, cursorId, ct);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsForNullCursorId()
    {
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ArgumentNullException>(() => _registry.DeleteAsync(null!, ct).AsTask());
    }

    [Fact]
    public async Task DeleteAsync_ThrowsForEmptyCursorId()
    {
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ArgumentException>(() => _registry.DeleteAsync("", ct).AsTask());
    }

    [Fact]
    public void GenerateCursorId_HasCorrectPrefix()
    {
        var id = CursorRegistry.GenerateCursorId();

        Assert.StartsWith(CursorRegistry.CursorPrefix, id);
    }

    [Fact]
    public void GenerateCursorId_ProducesUniqueIds()
    {
        var ids = Enumerable.Range(0, 100).Select(_ => CursorRegistry.GenerateCursorId()).ToHashSet();

        Assert.Equal(100, ids.Count);
    }
}
