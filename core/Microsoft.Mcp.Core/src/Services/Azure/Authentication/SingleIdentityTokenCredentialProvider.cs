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
    private readonly ILoggingTokenCredentialFactory _credentialFactory;
    private readonly TokenCredential _credential;
    private readonly Dictionary<string, TokenCredential> _tenantSpecificCredentials
        = new(StringComparer.OrdinalIgnoreCase);

    public SingleIdentityTokenCredentialProvider(
        ILoggerFactory loggerFactory,
        ILoggingTokenCredentialFactory credentialFactory)
    {
        _loggerFactory = loggerFactory;
        _credentialFactory = credentialFactory;
        _credential = credentialFactory.WrapIfEnabled(
            new CustomChainedCredential(null, loggerFactory.CreateLogger<CustomChainedCredential>()),
            null);
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
                    var innerCredential = new CustomChainedCredential(
                        tenantId,
                        _loggerFactory.CreateLogger<CustomChainedCredential>()
                    );
                    tenantCredential = _credentialFactory.WrapIfEnabled(innerCredential, tenantId);
                    _tenantSpecificCredentials[tenantId] = tenantCredential;
                }
            }
        }

        return Task.FromResult(tenantCredential);
    }
}
