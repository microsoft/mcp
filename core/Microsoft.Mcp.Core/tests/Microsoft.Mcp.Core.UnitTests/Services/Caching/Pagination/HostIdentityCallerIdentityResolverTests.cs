// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Services.Caching.Pagination;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Services.Caching.Pagination;

public class HostIdentityCallerIdentityResolverTests
{
    [Fact]
    public async Task ResolveAsync_ReturnsHostIdentityBinding()
    {
        var resolver = new HostIdentityCallerIdentityResolver();
        var ct = TestContext.Current.CancellationToken;

        var binding = await resolver.ResolveAsync(ct);

        Assert.Equal("hostIdentity", binding.Mode);
        Assert.Null(binding.TenantId);
        Assert.Null(binding.PrincipalIdHash);
    }

    [Fact]
    public async Task ResolveAsync_ReturnsSameInstanceEachTime()
    {
        var resolver = new HostIdentityCallerIdentityResolver();
        var ct = TestContext.Current.CancellationToken;

        var first = await resolver.ResolveAsync(ct);
        var second = await resolver.ResolveAsync(ct);

        Assert.Same(first, second);
    }
}
