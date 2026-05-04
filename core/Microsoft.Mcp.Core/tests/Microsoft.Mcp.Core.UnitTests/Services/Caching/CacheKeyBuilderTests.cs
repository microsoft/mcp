// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Services.Caching;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Services.Caching;

public class CacheKeyBuilderTests
{
    [Fact]
    public void Build_DifferentTuplesThatWouldCollideWithUnderscore_DoNotCollide()
    {
        var keyA = CacheKeyBuilder.Build("clusters", "sub1", "rg_x", null);
        var keyB = CacheKeyBuilder.Build("clusters", "sub1_rg", "x", null);

        Assert.NotEqual(keyA, keyB);
    }

    [Fact]
    public void Build_IncludesEmptyMarkersForNulls_SoTupleIsUnambiguous()
    {
        var keyWithNullTenant = CacheKeyBuilder.Build("cluster", "sub", "rg", "c", null);
        var keyWithEmptyTenant = CacheKeyBuilder.Build("cluster", "sub", "rg", "c", "");

        Assert.Equal(keyWithNullTenant, keyWithEmptyTenant);
    }

    [Fact]
    public void Build_IsStable()
    {
        var key1 = CacheKeyBuilder.Build("nodepool", "sub", "rg", "cluster", "np", "tenant");
        var key2 = CacheKeyBuilder.Build("nodepool", "sub", "rg", "cluster", "np", "tenant");

        Assert.Equal(key1, key2);
    }
}
