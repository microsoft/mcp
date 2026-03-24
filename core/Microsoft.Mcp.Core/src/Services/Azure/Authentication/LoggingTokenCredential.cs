// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// A <see cref="TokenCredential"/> decorator that logs the scopes from each token request.
/// </summary>
/// <remarks>
/// This class wraps an underlying <see cref="TokenCredential"/> and logs the scopes
/// from <see cref="TokenRequestContext"/> whenever <see cref="GetToken"/> or
/// <see cref="GetTokenAsync"/> is called. This is useful for recording which OAuth
/// scopes are being requested during server execution.
/// </remarks>
public sealed class LoggingTokenCredential(
    TokenCredential innerCredential,
    string? tenantId,
    ILogger logger) : TokenCredential
{
    private readonly TokenCredential _innerCredential = innerCredential;
    private readonly string? _tenantId = tenantId;
    private readonly ILogger _logger = logger;

    /// <inheritdoc/>
    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        LogScopes(requestContext);
        return _innerCredential.GetToken(requestContext, cancellationToken);
    }

    /// <inheritdoc/>
    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        LogScopes(requestContext);
        return _innerCredential.GetTokenAsync(requestContext, cancellationToken);
    }

    private void LogScopes(TokenRequestContext requestContext)
    {
        _logger.LogInformation(
            "TokenCredential.GetToken invoked. TenantId: {TenantId}, Scopes: {Scopes}",
            _tenantId ?? "(default)",
            requestContext.Scopes);
    }
}
