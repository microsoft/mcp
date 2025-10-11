// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Areas.Server.Configuration;
using Microsoft.AspNetCore.Http;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Token credential provider that passes through JWT bearer tokens from HTTP request headers.
/// </summary>
public sealed class JwtPassthroughCredentialProvider : ITokenCredentialProvider
{
    private readonly OutboundAuthenticationConfig _config;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the JwtPassthroughCredentialProvider class.
    /// </summary>
    /// <param name="config">The outbound authentication configuration containing header name.</param>
    /// <param name="httpContextAccessor">HTTP context accessor to read request headers.</param>
    /// <exception cref="ArgumentNullException">Thrown when config or httpContextAccessor is null.</exception>
    /// <exception cref="ArgumentException">Thrown when config type is not BearerToken or HeaderName is missing.</exception>
    public JwtPassthroughCredentialProvider(OutboundAuthenticationConfig config, IHttpContextAccessor httpContextAccessor)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        if (config.Type != OutboundAuthenticationType.BearerToken)
        {
            throw new ArgumentException($"Expected OutboundAuthenticationType.BearerToken, got {config.Type}", nameof(config));
        }

        if (string.IsNullOrEmpty(config.HeaderName))
        {
            throw new ArgumentException("HeaderName is required for BearerToken authentication", nameof(config));
        }
    }

    /// <summary>
    /// Creates a TokenCredential using the JWT bearer token from the current HTTP request header.
    /// </summary>
    /// <param name="tenant">Optional tenant ID (may be used for validation).</param>
    /// <returns>A TokenCredential that uses the JWT bearer token from the HTTP header.</returns>
    public Task<TokenCredential> CreateAsync(string? tenant = null)
    {
        return Task.FromResult<TokenCredential>(new JwtPassthroughTokenCredential(_httpContextAccessor, _config.HeaderName!));
    }

    /// <summary>
    /// Gets the current user identity extracted from the JWT bearer token.
    /// </summary>
    /// <returns>User identity string extracted from the token, or null if not available.</returns>
    public string? GetCurrentUserId()
    {
        // TODO: In the initial phase, we disable caching for JWT passthrough scenarios
        // by injecting NoCacheService implementation. When caching is enabled in the future,
        // this method should extract user identity from the JWT token (sub, oid, etc.)
        // to provide proper cache isolation between users.
        // 
        // For now, return null since no caching occurs anyway.
        return null;
    }

    /// <summary>
    /// Inner TokenCredential implementation that passes through JWT bearer tokens from HTTP context.
    /// </summary>
    private sealed class JwtPassthroughTokenCredential : TokenCredential
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _headerName;

        public JwtPassthroughTokenCredential(IHttpContextAccessor httpContextAccessor, string headerName)
        {
            _httpContextAccessor = httpContextAccessor;
            _headerName = headerName;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Synchronous token acquisition is not supported. Use GetTokenAsync instead.");
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("No HTTP context available. This credential can only be used within an HTTP request.");
            }

            // Extract token from the configured header
            var headerValue = httpContext.Request.Headers[_headerName].FirstOrDefault();
            if (string.IsNullOrEmpty(headerValue))
            {
                throw new InvalidOperationException($"No JWT bearer token found in header '{_headerName}'.");
            }

            // Remove "Bearer " prefix if present
            var token = headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? headerValue.Substring(7)
                : headerValue;

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException($"Empty JWT bearer token in header '{_headerName}'.");
            }

            // Since we get a fresh token from HTTP headers for each request,
            // so use a short expiration and prevent any caching
            var expiresOn = DateTimeOffset.UtcNow.AddSeconds(15);

            return ValueTask.FromResult(new AccessToken(token, expiresOn));
        }
    }
}
