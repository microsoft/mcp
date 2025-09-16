// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Http;

namespace Azure.Mcp.Tools.Extension.Services;

internal class CliGenerateService(IHttpClientService httpClientService) : ICliGenerateService
{
    private readonly IHttpClientService _httpClientService = httpClientService;

    public async Task<AccessToken> GetAzCliGenerateTokenAsync()
    {
        // GHCP4A Prod app
        const string apiScope = "9577cf87-9600-4e0d-94cd-0941e6f3c187/.default";

        var credential = new CustomChainedCredential();
        var accessToken = await credential.GetTokenAsync(new TokenRequestContext([apiScope]), CancellationToken.None);
        return accessToken;
    }

    public async Task<HttpResponseMessage> SendHttpRequestAsync(HttpRequestMessage request)
    {
        HttpResponseMessage responseMessage = await _httpClientService.DefaultClient.SendAsync(request);
        return responseMessage;
    }
}
