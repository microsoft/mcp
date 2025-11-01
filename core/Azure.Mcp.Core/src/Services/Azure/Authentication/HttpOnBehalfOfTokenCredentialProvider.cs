// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

public class HttpOnBehalfOfTokenCredentialProvider : IAzureTokenCredentialProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HttpOnBehalfOfTokenCredentialProvider> _logger;

    public HttpOnBehalfOfTokenCredentialProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<HttpOnBehalfOfTokenCredentialProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

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

        if (tenantId is not null
            && httpContext.User.FindFirst("tid")?.Value is string tidClaim
            && tidClaim != tenantId)
        {
            _logger.LogWarning(
                "The requested token tenant '{GetTokenTenant}' does not match the tenant of the authenticated user '{TidClaim}'. Going to throw.",
                tenantId,
                tidClaim);

            throw new InvalidOperationException(
                $"The requested token tenant '{tenantId}' does not match the tenant of the authenticated user '{tidClaim}'.");
        }

        // MicrosoftIdentityTokenCredential is registered as scoped, so we
        // can get it from the request services to ensure we get the right instance.
        MicrosoftIdentityTokenCredential tokenCredential = httpContext
            .RequestServices
            .GetRequiredService<MicrosoftIdentityTokenCredential>();
        return Task.FromResult<TokenCredential>(tokenCredential);
    }
}
