// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Services.Azure.Authentication;

public class TenantPinnedTokenCredentialTests
{
    private const string PinnedTenant = "11111111-1111-1111-1111-111111111111";

    [Fact]
    public void GetToken_OverridesTenantOnRequestContext_AndKeepsOtherFields()
    {
        var inner = new CapturingCredential();
        var credential = new TenantPinnedTokenCredential(inner, PinnedTenant);
        var request = new TokenRequestContext(
            ["https://management.azure.com/.default"], "parent-1", "claims-1", "22222222-2222-2222-2222-222222222222", isCaeEnabled: true);

        credential.GetToken(request, CancellationToken.None);

        Assert.Equal(PinnedTenant, inner.Captured.TenantId);
        Assert.Equal(request.Scopes, inner.Captured.Scopes);
        Assert.Equal("parent-1", inner.Captured.ParentRequestId);
        Assert.Equal("claims-1", inner.Captured.Claims);
        Assert.True(inner.Captured.IsCaeEnabled);
    }

    [Fact]
    public async Task GetTokenAsync_SetsTenant_WhenRequestHadNone()
    {
        var inner = new CapturingCredential();
        var credential = new TenantPinnedTokenCredential(inner, PinnedTenant);

        await credential.GetTokenAsync(new TokenRequestContext(["https://management.azure.com/.default"]), CancellationToken.None);

        Assert.Equal(PinnedTenant, inner.Captured.TenantId);
    }

    private sealed class CapturingCredential : TokenCredential
    {
        public TokenRequestContext Captured { get; private set; }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            Captured = requestContext;
            return new AccessToken("token", DateTimeOffset.UtcNow.AddHours(1));
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            Captured = requestContext;
            return new ValueTask<AccessToken>(new AccessToken("token", DateTimeOffset.UtcNow.AddHours(1)));
        }
    }
}
