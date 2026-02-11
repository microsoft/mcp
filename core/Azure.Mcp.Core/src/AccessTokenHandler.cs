// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Core;

/// <summary>
/// <see cref="DelegatingHandler"/> that adds a Bearer access token to each outgoing request.
/// </summary>
public sealed class AccessTokenHandler : DelegatingHandler
{
    private readonly IAzureTokenCredentialProvider _tokenCredentialProvider;
    private readonly string[] _oauthScopes;

    public AccessTokenHandler(IAzureTokenCredentialProvider tokenCredentialProvider, string[] oauthScopes)
    {
        _tokenCredentialProvider = tokenCredentialProvider;
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
        var credential = await _tokenCredentialProvider.GetTokenCredentialAsync(tenantId: null, cancellationToken);
        var tokenContext = new TokenRequestContext(_oauthScopes);
        var token = await credential.GetTokenAsync(tokenContext, cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return await base.SendAsync(request, cancellationToken);
    }
}
