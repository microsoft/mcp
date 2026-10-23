// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Fabric.Mcp.Tools.DataFactory;

/// <summary>
/// A <see cref="TokenCredential"/> that delegates to <see cref="IAzureTokenCredentialProvider"/>
/// on each token request, preserving per-execution-context credential resolution (e.g. OBO)
/// and avoiding sync-over-async in DI registration.
/// </summary>
internal sealed class ProviderDelegatingCredential(IAzureTokenCredentialProvider provider) : TokenCredential
{
    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        => GetTokenAsync(requestContext, cancellationToken).GetAwaiter().GetResult();

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var credential = await provider.GetTokenCredentialAsync(tenantId: null, cancellationToken).ConfigureAwait(false);
        return await credential.GetTokenAsync(requestContext, cancellationToken).ConfigureAwait(false);
    }
}
