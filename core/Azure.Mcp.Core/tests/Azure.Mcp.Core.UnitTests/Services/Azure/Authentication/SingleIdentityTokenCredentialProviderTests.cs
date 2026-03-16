// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Azure.Authentication;

public class SingleIdentityTokenCredentialProviderTests
{
    [Fact]
    public async Task NullTenantId_ReturnsSameDefaultCredentialInstance()
    {
        var provider = CreateProvider();

        TokenCredential first = await provider.GetTokenCredentialAsync(null, CancellationToken.None);
        TokenCredential second = await provider.GetTokenCredentialAsync(null, CancellationToken.None);

        Assert.Same(first, second);
        Assert.Equal("CustomChainedCredential", first.GetType().Name);
    }

    [Fact]
    public async Task SameTenantId_ReturnsCachedTenantCredentialInstance()
    {
        var provider = CreateProvider();

        TokenCredential first = await provider.GetTokenCredentialAsync("tenant-a", CancellationToken.None);
        TokenCredential second = await provider.GetTokenCredentialAsync("tenant-a", CancellationToken.None);

        Assert.Same(first, second);
        Assert.Equal("TenantAwareCredential", first.GetType().Name);
    }

    [Fact]
    public async Task DifferentTenantIds_ReturnDifferentTenantCredentialInstances()
    {
        var provider = CreateProvider();

        TokenCredential tenantA = await provider.GetTokenCredentialAsync("tenant-a", CancellationToken.None);
        TokenCredential tenantB = await provider.GetTokenCredentialAsync("tenant-b", CancellationToken.None);

        Assert.NotSame(tenantA, tenantB);
        Assert.Equal("TenantAwareCredential", tenantA.GetType().Name);
        Assert.Equal("TenantAwareCredential", tenantB.GetType().Name);
    }

    [Fact]
    public async Task TenantCredential_IsDifferentFromDefaultCredential()
    {
        var provider = CreateProvider();

        TokenCredential defaultCredential = await provider.GetTokenCredentialAsync(null, CancellationToken.None);
        TokenCredential tenantCredential = await provider.GetTokenCredentialAsync("tenant-a", CancellationToken.None);

        Assert.NotSame(defaultCredential, tenantCredential);
        Assert.Equal("CustomChainedCredential", defaultCredential.GetType().Name);
        Assert.Equal("TenantAwareCredential", tenantCredential.GetType().Name);
    }

    private static SingleIdentityTokenCredentialProvider CreateProvider()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        return new SingleIdentityTokenCredentialProvider(loggerFactory);
    }
}
