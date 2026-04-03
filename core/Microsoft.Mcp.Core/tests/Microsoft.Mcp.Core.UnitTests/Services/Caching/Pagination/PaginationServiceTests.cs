// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Services.Caching.Pagination;
using NSubstitute;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Services.Caching.Pagination;

public class PaginationServiceTests : IDisposable
{
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly CursorRegistry _cursorRegistry;
    private readonly ICallerIdentityResolver _callerIdentityResolver = Substitute.For<ICallerIdentityResolver>();
    private readonly PaginationService _service;

    private static readonly CallerBinding s_hostBinding = new() { Mode = "hostIdentity" };

    public PaginationServiceTests()
    {
        _cursorRegistry = new CursorRegistry(_memoryCache);
        _callerIdentityResolver.ResolveAsync(Arg.Any<CancellationToken>())
            .Returns(new ValueTask<CallerBinding>(s_hostBinding));
        _service = new PaginationService(
            _cursorRegistry,
            _callerIdentityResolver,
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

    // --- ResolveCallerBindingAsync ---

    [Fact]
    public async Task ResolveCallerBindingAsync_DelegatesToResolver()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _service.ResolveCallerBindingAsync(ct);

        Assert.Equal("hostIdentity", result.Mode);
        await _callerIdentityResolver.Received(1).ResolveAsync(ct);
    }

    // --- SaveCursorAsync ---

    [Fact]
    public async Task SaveCursorAsync_ReturnsCursorIdWithPrefix()
    {
        var ct = TestContext.Current.CancellationToken;

        var cursorId = await _service.SaveCursorAsync(
            "arm", "resourcegroup.list", "sha256:abc", s_hostBinding,
            "https://next", cancellationToken: ct);

        Assert.StartsWith(CursorRegistry.CursorPrefix, cursorId);
    }

    [Fact]
    public async Task SaveCursorAsync_StoresRetrievableRecord()
    {
        var ct = TestContext.Current.CancellationToken;

        var cursorId = await _service.SaveCursorAsync(
            "arm", "resourcegroup.list", "sha256:abc", s_hostBinding,
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
            "arm", "resourcegroup.list", "sha256:abc", s_hostBinding,
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
            "arm", "op", "sha256:abc", s_hostBinding,
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
            _service.SaveCursorAsync(provider!, operation!, fingerprint!, s_hostBinding, nativeState!, cancellationToken: ct).AsTask());
    }

    [Fact]
    public async Task SaveCursorAsync_ThrowsForNullCallerBinding()
    {
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.SaveCursorAsync("arm", "op", "fp", null!, "state", cancellationToken: ct).AsTask());
    }

    // --- LoadAndValidateCursorAsync ---

    [Fact]
    public async Task LoadAndValidateCursorAsync_ReturnsValidRecord()
    {
        var ct = TestContext.Current.CancellationToken;
        var fingerprint = "sha256:test";

        var cursorId = await _service.SaveCursorAsync(
            "arm", "resourcegroup.list", fingerprint, s_hostBinding,
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
            "arm", "op", "sha256:original", s_hostBinding,
            "state", cancellationToken: ct);

        var ex = await Assert.ThrowsAsync<InvalidCursorException>(() =>
            _service.LoadAndValidateCursorAsync(cursorId, "sha256:different", ct).AsTask());

        Assert.Equal(InvalidCursorReason.FingerprintMismatch, ex.Reason);
    }

    [Fact]
    public async Task LoadAndValidateCursorAsync_ThrowsCallerBindingMismatch()
    {
        var ct = TestContext.Current.CancellationToken;
        var fingerprint = "sha256:test";

        // Save with host identity binding
        var cursorId = await _service.SaveCursorAsync(
            "arm", "op", fingerprint, s_hostBinding,
            "state", cancellationToken: ct);

        // Now the resolver returns a different binding
        var oboBinding = new CallerBinding
        {
            Mode = "obo",
            TenantId = "tenant-1",
            PrincipalIdHash = "sha256:user1",
        };
        _callerIdentityResolver.ResolveAsync(Arg.Any<CancellationToken>())
            .Returns(new ValueTask<CallerBinding>(oboBinding));

        var ex = await Assert.ThrowsAsync<InvalidCursorException>(() =>
            _service.LoadAndValidateCursorAsync(cursorId, fingerprint, ct).AsTask());

        Assert.Equal(InvalidCursorReason.CallerBindingMismatch, ex.Reason);
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

    // --- CallerBindingsMatch ---

    [Fact]
    public void CallerBindingsMatch_ReturnsTrueForIdenticalBindings()
    {
        var a = new CallerBinding { Mode = "obo", TenantId = "t1", PrincipalIdHash = "sha256:p1" };
        var b = new CallerBinding { Mode = "obo", TenantId = "t1", PrincipalIdHash = "sha256:p1" };

        Assert.True(PaginationService.CallerBindingsMatch(a, b));
    }

    [Fact]
    public void CallerBindingsMatch_ReturnsTrueForHostIdentityBindings()
    {
        var a = new CallerBinding { Mode = "hostIdentity" };
        var b = new CallerBinding { Mode = "hostIdentity" };

        Assert.True(PaginationService.CallerBindingsMatch(a, b));
    }

    [Fact]
    public void CallerBindingsMatch_ReturnsFalseForDifferentModes()
    {
        var a = new CallerBinding { Mode = "hostIdentity" };
        var b = new CallerBinding { Mode = "obo", TenantId = "t1" };

        Assert.False(PaginationService.CallerBindingsMatch(a, b));
    }

    [Fact]
    public void CallerBindingsMatch_ReturnsFalseForDifferentTenants()
    {
        var a = new CallerBinding { Mode = "obo", TenantId = "t1", PrincipalIdHash = "sha256:p1" };
        var b = new CallerBinding { Mode = "obo", TenantId = "t2", PrincipalIdHash = "sha256:p1" };

        Assert.False(PaginationService.CallerBindingsMatch(a, b));
    }

    [Fact]
    public void CallerBindingsMatch_ReturnsFalseForDifferentPrincipals()
    {
        var a = new CallerBinding { Mode = "obo", TenantId = "t1", PrincipalIdHash = "sha256:p1" };
        var b = new CallerBinding { Mode = "obo", TenantId = "t1", PrincipalIdHash = "sha256:p2" };

        Assert.False(PaginationService.CallerBindingsMatch(a, b));
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }
}
