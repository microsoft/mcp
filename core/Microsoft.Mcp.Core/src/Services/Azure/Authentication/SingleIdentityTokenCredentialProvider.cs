// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Extensions.Logging;

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Implementation of <see cref="IAzureTokenCredentialProvider"/> that uses and caches
/// instances of <see cref="CustomChainedCredential"/>.
/// </summary>
public class SingleIdentityTokenCredentialProvider : IAzureTokenCredentialProvider
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly bool _forceBrowserFallback;
    private readonly TokenCredential _credential;
    private readonly Dictionary<string, TokenCredential> _tenantSpecificCredentials
        = new(StringComparer.OrdinalIgnoreCase);

    /// <param name="forceBrowserFallback">Allow an interactive browser prompt when every silent
    /// credential fails. Only appropriate where the user's own identity drives auth.</param>
    public SingleIdentityTokenCredentialProvider(ILoggerFactory loggerFactory, bool forceBrowserFallback = false)
    {
        _loggerFactory = loggerFactory;
        _forceBrowserFallback = forceBrowserFallback;
        _credential = new CustomChainedCredential(
            null,
            _loggerFactory.CreateLogger<CustomChainedCredential>(),
            forceBrowserFallback
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
                        _loggerFactory.CreateLogger<CustomChainedCredential>(),
                        _forceBrowserFallback
                    );
                    _tenantSpecificCredentials[tenantId] = tenantCredential;
                }
            }
        }

        return Task.FromResult(tenantCredential);
    }
}
