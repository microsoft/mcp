// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Implementation of <see cref="IAzureTokenCredentialProvider"/> that uses and caches
/// instances of <see cref="CustomChainedCredential"/>.
/// </summary>
public class SingleIdentityTokenCredentialProvider : IAzureTokenCredentialProvider
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly TokenCredential _credential;
    private readonly Dictionary<string, TokenCredential> _tenantSpecificCredentials
        = new(StringComparer.OrdinalIgnoreCase);

    public SingleIdentityTokenCredentialProvider(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _credential = new CustomChainedCredential(
            null,
            _loggerFactory.CreateLogger<CustomChainedCredential>()
        );
    }

    /// <inheritdoc/>
    public Task<TokenCredential> GetTokenCredentialAsync(
        string? tenantId,
        CancellationToken cancellation)
    {
        if (tenantId is null)
        {
            return Task.FromResult(_credential);
        }

        if (!_tenantSpecificCredentials.TryGetValue(tenantId, out TokenCredential? tenantCredential))
        {
            lock (_tenantSpecificCredentials)
            {
                if (!_tenantSpecificCredentials.TryGetValue(tenantId, out tenantCredential))
                {
                    tenantCredential = new CustomChainedCredential(
                        tenantId,
                        _loggerFactory.CreateLogger<CustomChainedCredential>()
                    );
                    _tenantSpecificCredentials[tenantId] = tenantCredential;
                }
            }
        }

        return Task.FromResult(tenantCredential);
    }
}
