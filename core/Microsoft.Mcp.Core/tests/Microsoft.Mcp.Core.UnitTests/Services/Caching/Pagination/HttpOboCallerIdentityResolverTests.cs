// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Mcp.Core.Services.Caching.Pagination;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Services.Caching.Pagination;

public class HttpOboCallerIdentityResolverTests
{
    private static IHttpContextAccessor CreateAccessor(HttpContext? context)
    {
        var accessor = new HttpContextAccessor { HttpContext = context };
        return accessor;
    }

    private static DefaultHttpContext CreateAuthenticatedContext(
        string? tenantId = null, string? objectId = null, string? subject = null)
    {
        var claims = new List<Claim>();
        if (tenantId is not null)
            claims.Add(new Claim("tid", tenantId));
        if (objectId is not null)
            claims.Add(new Claim("oid", objectId));
        if (subject is not null)
            claims.Add(new Claim("sub", subject));

        var identity = new ClaimsIdentity(claims, "Bearer");
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity),
        };
        return context;
    }

    [Fact]
    public async Task ResolveAsync_ReturnsOboModeBinding()
    {
        var context = CreateAuthenticatedContext(tenantId: "tenant-1", objectId: "user-1");
        var resolver = new HttpOboCallerIdentityResolver(CreateAccessor(context));
        var ct = TestContext.Current.CancellationToken;

        var binding = await resolver.ResolveAsync(ct);

        Assert.Equal("obo", binding.Mode);
        Assert.Equal("tenant-1", binding.TenantId);
        Assert.NotNull(binding.PrincipalIdHash);
        Assert.StartsWith("sha256:", binding.PrincipalIdHash);
    }

    [Fact]
    public async Task ResolveAsync_UsesOidClaim()
    {
        var context = CreateAuthenticatedContext(tenantId: "t1", objectId: "oid-value", subject: "sub-value");
        var resolver = new HttpOboCallerIdentityResolver(CreateAccessor(context));
        var ct = TestContext.Current.CancellationToken;

        var binding = await resolver.ResolveAsync(ct);

        // oid should take precedence over sub
        var expectedHash = HttpOboCallerIdentityResolver.HashPrincipalId("oid-value");
        Assert.Equal(expectedHash, binding.PrincipalIdHash);
    }

    [Fact]
    public async Task ResolveAsync_FallsBackToSubClaim()
    {
        var context = CreateAuthenticatedContext(tenantId: "t1", subject: "sub-value");
        var resolver = new HttpOboCallerIdentityResolver(CreateAccessor(context));
        var ct = TestContext.Current.CancellationToken;

        var binding = await resolver.ResolveAsync(ct);

        var expectedHash = HttpOboCallerIdentityResolver.HashPrincipalId("sub-value");
        Assert.Equal(expectedHash, binding.PrincipalIdHash);
    }

    [Fact]
    public async Task ResolveAsync_NullPrincipalIdHashWhenNoClaims()
    {
        var context = CreateAuthenticatedContext(tenantId: "t1");
        var resolver = new HttpOboCallerIdentityResolver(CreateAccessor(context));
        var ct = TestContext.Current.CancellationToken;

        var binding = await resolver.ResolveAsync(ct);

        Assert.Equal("obo", binding.Mode);
        Assert.Equal("t1", binding.TenantId);
        Assert.Null(binding.PrincipalIdHash);
    }

    [Fact]
    public async Task ResolveAsync_ThrowsWhenNotAuthenticated()
    {
        var context = new DefaultHttpContext(); // no authenticated identity
        var resolver = new HttpOboCallerIdentityResolver(CreateAccessor(context));
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => resolver.ResolveAsync(ct).AsTask());
    }

    [Fact]
    public async Task ResolveAsync_ThrowsWhenNoHttpContext()
    {
        var resolver = new HttpOboCallerIdentityResolver(CreateAccessor(null));
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => resolver.ResolveAsync(ct).AsTask());
    }

    [Fact]
    public void HashPrincipalId_IsDeterministic()
    {
        var first = HttpOboCallerIdentityResolver.HashPrincipalId("test-principal");
        var second = HttpOboCallerIdentityResolver.HashPrincipalId("test-principal");

        Assert.Equal(first, second);
        Assert.StartsWith("sha256:", first);
    }

    [Fact]
    public void HashPrincipalId_DifferentInputsProduceDifferentHashes()
    {
        var a = HttpOboCallerIdentityResolver.HashPrincipalId("user-a");
        var b = HttpOboCallerIdentityResolver.HashPrincipalId("user-b");

        Assert.NotEqual(a, b);
    }
}
