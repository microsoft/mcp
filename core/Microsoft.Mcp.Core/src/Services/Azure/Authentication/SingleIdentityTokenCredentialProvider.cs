// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
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
    // Concurrent: in HTTP mode the tenant comes from a per-call tool argument, so arbitrary keys
    // arrive from several requests at once.
    private readonly ConcurrentDictionary<string, TokenCredential> _tenantSpecificCredentials
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

        var tenantCredential = _tenantSpecificCredentials.GetOrAdd(
            tenantId,
            id => new CustomChainedCredential(
                id,
                _loggerFactory.CreateLogger<CustomChainedCredential>(),
                _forceBrowserFallback
            ));

        return Task.FromResult(tenantCredential);
    }
}
