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
    private readonly IDictionary<string, TokenCredential> _tenantSpecificCredentials
        = new Dictionary<string, TokenCredential>();

    public SingleIdentityTokenCredentialProvider(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _credential = new CustomChainedCredential(
            null,
            _loggerFactory.CreateLogger<CustomChainedCredential>()
        );
    }

    /// <inheritdoc/>
    public Task<TokenCredential> GetTokenAsync(
        string? tenantId,
        CancellationToken cancellation)
    {
        if (tenantId is null)
        {
            return Task.FromResult(_credential);
        }

        if (!_tenantSpecificCredentials.TryGetValue(tenantId, out TokenCredential? tenantCredential))
        {
            tenantCredential = new CustomChainedCredential(
                tenantId,
                _loggerFactory.CreateLogger<CustomChainedCredential>()
            );
            _tenantSpecificCredentials[tenantId] = tenantCredential;
        }

        return Task.FromResult(tenantCredential);
    }
}
