// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Services.Caching.Pagination;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Services.Caching.Pagination;

public class PaginationServiceTests : IDisposable
{
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly CursorRegistry _cursorRegistry;
    private readonly PaginationService _service;

    public PaginationServiceTests()
    {
        _cursorRegistry = new CursorRegistry(_memoryCache);
        _service = new PaginationService(
            _cursorRegistry,
            NullLogger<PaginationService>.Instance);
    }

    // --- ComputeRequestFingerprint ---

    [Fact]
    public void ComputeRequestFingerprint_ReturnsSha256Prefix()
    {
        var fingerprint = _service.ComputeRequestFingerprint(new Dictionary<string, string?>
        {
            ["operation"] = "resourcegroup.list",
        });

        Assert.StartsWith("sha256:", fingerprint);
    }

    [Fact]
    public void ComputeRequestFingerprint_IsDeterministic()
    {
        var parameters = new Dictionary<string, string?>
        {
            ["operation"] = "resourcegroup.list",
            ["subscription"] = "sub-1",
        };

        var first = _service.ComputeRequestFingerprint(parameters);
        var second = _service.ComputeRequestFingerprint(parameters);

        Assert.Equal(first, second);
    }

    [Fact]
    public void ComputeRequestFingerprint_OrderIndependent()
    {
        var aFirst = new Dictionary<string, string?>
        {
            ["a"] = "1",
            ["b"] = "2",
        };
        var bFirst = new Dictionary<string, string?>
        {
            ["b"] = "2",
            ["a"] = "1",
        };

        Assert.Equal(
            _service.ComputeRequestFingerprint(aFirst),
            _service.ComputeRequestFingerprint(bFirst));
    }

    [Fact]
    public void ComputeRequestFingerprint_ExcludesNullValues()
    {
        var withNull = new Dictionary<string, string?>
        {
            ["operation"] = "list",
            ["filter"] = null,
        };
        var withoutNull = new Dictionary<string, string?>
        {
            ["operation"] = "list",
        };

        Assert.Equal(
            _service.ComputeRequestFingerprint(withNull),
            _service.ComputeRequestFingerprint(withoutNull));
    }

    [Fact]
    public void ComputeRequestFingerprint_DifferentValueProducesDifferentHash()
    {
        var a = _service.ComputeRequestFingerprint(new Dictionary<string, string?> { ["op"] = "list" });
        var b = _service.ComputeRequestFingerprint(new Dictionary<string, string?> { ["op"] = "get" });

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ComputeRequestFingerprint_ThrowsForNullParameters()
    {
        Assert.Throws<ArgumentNullException>(() => _service.ComputeRequestFingerprint(null!));
    }

    // --- SaveCursorAsync ---

    [Fact]
    public async Task SaveCursorAsync_ReturnsCursorIdWithPrefix()
    {
        var ct = TestContext.Current.CancellationToken;

        var cursorId = await _service.SaveCursorAsync(
            "arm", "resourcegroup.list", "sha256:abc",
            "https://next", cancellationToken: ct);

        Assert.StartsWith(CursorRegistry.CursorPrefix, cursorId);
    }

    [Fact]
    public async Task SaveCursorAsync_StoresRetrievableRecord()
    {
        var ct = TestContext.Current.CancellationToken;

        var cursorId = await _service.SaveCursorAsync(
            "arm", "resourcegroup.list", "sha256:abc",
            "https://next", cancellationToken: ct);

        var record = await _cursorRegistry.GetAsync(cursorId, ct);
        Assert.NotNull(record);
        Assert.Equal("arm", record.Provider);
        Assert.Equal("resourcegroup.list", record.Operation);
        Assert.Equal("sha256:abc", record.RequestFingerprint);
        Assert.Equal("https://next", record.NativeState);
    }

    [Fact]
    public async Task SaveCursorAsync_StoresResourceMetadata()
    {
        var ct = TestContext.Current.CancellationToken;
        var metadata = new Dictionary<string, string> { ["subscription"] = "sub-1" };

        var cursorId = await _service.SaveCursorAsync(
            "arm", "resourcegroup.list", "sha256:abc",
            "https://next", resourceMetadata: metadata, cancellationToken: ct);

        var record = await _cursorRegistry.GetAsync(cursorId, ct);
        Assert.NotNull(record);
        Assert.NotNull(record.ResourceMetadata);
        Assert.Equal("sub-1", record.ResourceMetadata["subscription"]);
    }

    [Fact]
    public async Task SaveCursorAsync_UsesCustomTtl()
    {
        var ct = TestContext.Current.CancellationToken;
        var customTtl = TimeSpan.FromMinutes(5);

        var cursorId = await _service.SaveCursorAsync(
            "arm", "op", "sha256:abc",
            "state", ttl: customTtl, cancellationToken: ct);

        var record = await _cursorRegistry.GetAsync(cursorId, ct);
        Assert.NotNull(record);
        // TTL should be approximately 5 minutes from now
        var remainingTtl = record.ExpiresAtUtc - DateTimeOffset.UtcNow;
        Assert.True(remainingTtl <= customTtl);
        Assert.True(remainingTtl > TimeSpan.FromMinutes(4));
    }

    [Theory]
    [InlineData(null, "op", "fp", "state")]
    [InlineData("", "op", "fp", "state")]
    [InlineData("arm", null, "fp", "state")]
    [InlineData("arm", "", "fp", "state")]
    [InlineData("arm", "op", null, "state")]
    [InlineData("arm", "op", "", "state")]
    [InlineData("arm", "op", "fp", null)]
    [InlineData("arm", "op", "fp", "")]
    public async Task SaveCursorAsync_ThrowsForInvalidArguments(
        string? provider, string? operation, string? fingerprint, string? nativeState)
    {
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _service.SaveCursorAsync(provider!, operation!, fingerprint!, nativeState!, cancellationToken: ct).AsTask());
    }

    // --- LoadAndValidateCursorAsync ---

    [Fact]
    public async Task LoadAndValidateCursorAsync_ReturnsValidRecord()
    {
        var ct = TestContext.Current.CancellationToken;
        var fingerprint = "sha256:test";

        var cursorId = await _service.SaveCursorAsync(
            "arm", "resourcegroup.list", fingerprint,
            "https://next", cancellationToken: ct);

        var record = await _service.LoadAndValidateCursorAsync(cursorId, fingerprint, ct);

        Assert.Equal(cursorId, record.CursorId);
        Assert.Equal("arm", record.Provider);
        Assert.Equal("https://next", record.NativeState);
    }

    [Fact]
    public async Task LoadAndValidateCursorAsync_ThrowsNotFoundForUnknownCursor()
    {
        var ct = TestContext.Current.CancellationToken;

        var ex = await Assert.ThrowsAsync<InvalidCursorException>(() =>
            _service.LoadAndValidateCursorAsync("cur_unknown", "sha256:test", ct).AsTask());

        Assert.Equal(InvalidCursorReason.NotFound, ex.Reason);
    }

    [Fact]
    public async Task LoadAndValidateCursorAsync_ThrowsFingerprintMismatch()
    {
        var ct = TestContext.Current.CancellationToken;

        var cursorId = await _service.SaveCursorAsync(
            "arm", "op", "sha256:original",
            "state", cancellationToken: ct);

        var ex = await Assert.ThrowsAsync<InvalidCursorException>(() =>
            _service.LoadAndValidateCursorAsync(cursorId, "sha256:different", ct).AsTask());

        Assert.Equal(InvalidCursorReason.FingerprintMismatch, ex.Reason);
    }

    [Fact]
    public async Task LoadAndValidateCursorAsync_ThrowsForNullCursorId()
    {
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _service.LoadAndValidateCursorAsync(null!, "sha256:test", ct).AsTask());
    }

    [Fact]
    public async Task LoadAndValidateCursorAsync_ThrowsForEmptyFingerprint()
    {
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.LoadAndValidateCursorAsync("cur_abc", "", ct).AsTask());
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }
}
