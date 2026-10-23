// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers.Text;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Core;

/// <summary>
/// <see cref="DelegatingHandler"/> that adds a Bearer access token to each outgoing request.
/// </summary>
public sealed class AccessTokenHandler : DelegatingHandler
{
    private readonly IAzureTokenCredentialProvider? _tokenCredentialProvider;
    private readonly TokenCredential? _credential;
    private readonly string[] _oauthScopes;

    public AccessTokenHandler(IAzureTokenCredentialProvider tokenCredentialProvider, string[] oauthScopes)
    {
        _tokenCredentialProvider = tokenCredentialProvider;
        _oauthScopes = oauthScopes;
    }

    public AccessTokenHandler(TokenCredential credential, string[] oauthScopes)
    {
        _credential = credential;
        _oauthScopes = oauthScopes;
    }

    /// <summary>
    /// Sends an HTTP request with a Bearer access token fetched using the embedded <see cref="IAzureTokenCredentialProvider"/>.
    /// This method will overwrite the Authorization header if it already exist on the request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Set by RegistryToolLoader when the caller passed a tenant; null means the hosting
        // identity's own tenant, which is every non-registry caller.
        var tenantId = RegistryTenantContext.CurrentTenantId;

        if (tenantId is not null && _tokenCredentialProvider is null)
        {
            throw new InvalidOperationException(
                "A per-call tenant was requested but this AccessTokenHandler was constructed with a fixed " +
                "TokenCredential, which cannot issue tokens for another tenant. Construct it with an " +
                "IAzureTokenCredentialProvider instead.");
        }

        TokenCredential credential = _credential
            ?? await _tokenCredentialProvider!.GetTokenCredentialAsync(tenantId, cancellationToken);
        var tokenContext = new TokenRequestContext(_oauthScopes);
        var token = await credential.GetTokenAsync(tokenContext, cancellationToken);

        EnsureTokenBelongsToRequestedTenant(token.Token, tenantId);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Fails the request when the token's tid claim contradicts the requested tenant, so a broken
    /// tenant flow surfaces as an error instead of as another tenant's data.
    /// </summary>
    private static void EnsureTokenBelongsToRequestedTenant(string accessToken, string? requestedTenantId)
    {
        if (requestedTenantId is null)
        {
            return;
        }

        var actualTenantId = TryReadTenantClaim(accessToken);
        if (actualTenantId is null || string.Equals(actualTenantId, requestedTenantId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Acquired an access token for tenant '{actualTenantId}' while tenant '{requestedTenantId}' was " +
            "requested. Refusing to send the request rather than return data from the wrong tenant.");
    }

    /// <summary>
    /// Reads the tid claim from a JWT access token, or null when the token is not a JWT or has no
    /// such claim. Signature is not validated; the issuer already did that.
    /// </summary>
    private static string? TryReadTenantClaim(string accessToken)
    {
        var firstDot = accessToken.IndexOf('.');
        if (firstDot < 0)
        {
            return null;
        }

        var secondDot = accessToken.IndexOf('.', firstDot + 1);
        if (secondDot < 0)
        {
            return null;
        }

        var payload = accessToken.AsSpan(firstDot + 1, secondDot - firstDot - 1);
        var decoded = new byte[Base64Url.GetMaxDecodedLength(payload.Length)];
        if (!Base64Url.TryDecodeFromChars(payload, decoded, out var written))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(decoded.AsMemory(0, written));
            return document.RootElement.TryGetProperty("tid", out var tid) && tid.ValueKind == JsonValueKind.String
                ? tid.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
