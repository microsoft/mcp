// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tests.Client;

internal sealed class PlaybackTokenCredentialProvider : IAzureTokenCredentialProvider
{
    private static readonly TokenCredential s_credential = new PlaybackTokenCredential();

    public Task<TokenCredential> GetTokenCredentialAsync(string? tenantId, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        return Task.FromResult(s_credential);
    }
}
