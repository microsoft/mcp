// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Claims;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Mcp.Core.Helpers;

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

public class HttpOnBehalfOfTokenCredentialProvider(
    IHttpContextAccessor httpContextAccessor,
    ILogger<HttpOnBehalfOfTokenCredentialProvider> logger) : IAzureTokenCredentialProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<HttpOnBehalfOfTokenCredentialProvider> _logger = logger;

    /// <inheritdoc/>
    public Task<TokenCredential> GetTokenCredentialAsync(string? tenantId, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext is not HttpContext httpContext)
        {
            throw new InvalidOperationException("There is no ongoing HTTP request.");
        }

        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            throw new InvalidOperationException(
                "The current HTTP request must be authenticated to make an on-behalf-of token request.");
        }

        // OBO only works for user principals. Fail fast instead of letting Entra ID reject the
        // exchange with a generic MSAL invalid_request error.
        if (IsAppOnlyToken(httpContext.User))
        {
            _logger.LogWarning("The incoming request was authenticated with an application-only token, which cannot be used for an on-behalf-of token request. Going to throw.");
            throw new OnBehalfOfUserTokenRequiredException();
        }

        if (tenantId is not null)
        {
            if (httpContext.User.FindFirst("tid")?.Value is string tidClaim
                && !string.Equals(tidClaim, tenantId, StringComparisons.TenantId))
            {
                _logger.LogWarning(
                    "The requested token tenant '{GetTokenTenant}' does not match the tenant of the authenticated user '{TidClaim}'. Going to throw.",
                    tenantId,
                    tidClaim);

                throw new InvalidOperationException(
                    $"The requested token tenant '{tenantId}' does not match the tenant of the authenticated user '{tidClaim}'.");
            }
        }

        // MicrosoftIdentityTokenCredential is registered as scoped, so we
        // can get it from the request services to ensure we get the right instance.
        MicrosoftIdentityTokenCredential tokenCredential = httpContext
            .RequestServices
            .GetRequiredService<MicrosoftIdentityTokenCredential>();
        return Task.FromResult<TokenCredential>(tokenCredential);
    }

    /// <summary>
    /// Determines whether the principal was authenticated with an application-only (client credentials) token.
    /// Requires positive evidence: the optional <c>idtyp</c> claim is <c>app</c>, or the token carries
    /// application roles without any delegated scopes.
    /// </summary>
    internal static bool IsAppOnlyToken(ClaimsPrincipal user)
    {
        var idtyp = user.FindFirst(IdentityTypeClaim)?.Value;
        if (!string.IsNullOrEmpty(idtyp))
        {
            return string.Equals(idtyp, "app", StringComparison.OrdinalIgnoreCase);
        }

        var hasDelegatedScopes = user.HasClaim(c => c.Type is ClaimConstants.Scp or ClaimConstants.Scope);
        var hasAppRoles = user.HasClaim(c => c.Type is ClaimConstants.Roles or ClaimTypes.Role);
        return hasAppRoles && !hasDelegatedScopes;
    }

    private const string IdentityTypeClaim = "idtyp";
}
