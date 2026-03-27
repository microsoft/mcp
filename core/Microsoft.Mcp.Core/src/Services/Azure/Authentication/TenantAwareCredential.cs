// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using Azure.Identity.Broker;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Tenant-scoped credential for explicit tenant switching scenarios.
/// This class is isolated from <see cref="CustomChainedCredential"/> behavior so default auth
/// flows are unchanged unless a tenant-specific credential is requested.
/// </summary>
internal class TenantAwareCredential(string tenantId, ILogger<CustomChainedCredential>? logger = null)
    : CustomChainedCredential(tenantId, logger)
{
    private const string BrowserAuthenticationTimeoutEnvVarName = "AZURE_MCP_BROWSER_AUTH_TIMEOUT_SECONDS";
    private const string ClientIdEnvVarName = "AZURE_MCP_CLIENT_ID";
    private const string TokenCacheName = "azure-mcp-msal.cache";

    private readonly string _tenantId = tenantId;
    private readonly TokenCredential _tenantScopedCredential = CreateTenantScopedCredential(tenantId);

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_tenantId))
        {
            return base.GetToken(requestContext, cancellationToken);
        }

        return _tenantScopedCredential.GetToken(requestContext, cancellationToken);
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_tenantId))
        {
            return base.GetTokenAsync(requestContext, cancellationToken);
        }

        return _tenantScopedCredential.GetTokenAsync(requestContext, cancellationToken);
    }

    private static TokenCredential CreateTenantScopedCredential(string tenantId)
    {
        // Explicit tenant switcher: avoid sticky-account behavior and stale auth record reuse.
        // This enables a clean tenant-specific interactive auth path where MFA/2FA prompts can
        // follow the external tenant policy.
        IntPtr handle = WindowHandleProvider.GetWindowHandle();

        string? clientId = Environment.GetEnvironmentVariable(ClientIdEnvVarName);

        var brokerOptions = new InteractiveBrowserCredentialBrokerOptions(handle)
        {
            TenantId = tenantId,
            UseDefaultBrokerAccount = false,
            AuthenticationRecord = null,
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions
            {
                Name = TokenCacheName,
            },
        };

        if (CustomChainedCredential.CloudConfiguration != null)
        {
            brokerOptions.AuthorityHost = CustomChainedCredential.CloudConfiguration.AuthorityHost;
        }

        if (!string.IsNullOrWhiteSpace(clientId))
        {
            brokerOptions.ClientId = clientId;
        }

        var browserCredential = new InteractiveBrowserCredential(brokerOptions);

        string? timeoutValue = Environment.GetEnvironmentVariable(BrowserAuthenticationTimeoutEnvVarName);
        int timeoutSeconds = 300;
        if (!string.IsNullOrEmpty(timeoutValue) && int.TryParse(timeoutValue, out int parsedTimeout) && parsedTimeout > 0)
        {
            timeoutSeconds = parsedTimeout;
        }

        return new TimeoutTokenCredential(browserCredential, TimeSpan.FromSeconds(timeoutSeconds));
    }
}
