// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Mcp.Core.Services.Pagination;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Services.Pagination;

public class CursorRegistryTests : IDisposable
{
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly CursorRegistry _registry;

    public CursorRegistryTests()
    {
        _registry = new CursorRegistry(_memoryCache);
    }

    private static CursorRecord CreateRecord(string? cursorId = null, DateTimeOffset? expiresAt = null) => new()
    {
        CursorId = cursorId ?? CursorRegistry.GenerateCursorId(),
        Provider = "arm",
        Operation = "resourcegroup.list",
        RequestFingerprint = "sha256:abc123",
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

        // Verify the record was actually stored
        var retrieved = await _registry.GetAsync(record.CursorId, ct);
        Assert.NotNull(retrieved);
        Assert.Equal(record.CursorId, retrieved.CursorId);
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
        await _registry.SetAsync(record, ct);

        var result = await _registry.GetAsync(record.CursorId, ct);

        Assert.NotNull(result);
        Assert.Equal(record.CursorId, result.CursorId);
        Assert.Equal(record.Provider, result.Provider);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullForUnknownCursor()
    {
        var ct = TestContext.Current.CancellationToken;

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
    public async Task DeleteAsync_RemovesFromCache()
    {
        var ct = TestContext.Current.CancellationToken;
        var cursorId = CursorRegistry.GenerateCursorId();
        var record = CreateRecord(cursorId: cursorId);
        await _registry.SetAsync(record, ct);

        await _registry.DeleteAsync(cursorId, ct);

        var result = await _registry.GetAsync(cursorId, ct);
        Assert.Null(result);
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

    public void Dispose()
    {
        _memoryCache.Dispose();
    }
}
