// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Extension.Models;

namespace Azure.Mcp.Tools.Extension.Services;

internal class CliGenerateService(IHttpClientService httpClientService) : ICliGenerateService
{
    private readonly IHttpClientService _httpClientService = httpClientService;

    public async Task<HttpResponseMessage> GenerateAzureCLICommandAsync(string intent)
    {
        // GHCP4A Prod app scope
        const string apiScope = "9577cf87-9600-4e0d-94cd-0941e6f3c187/.default";

        var credential = new CustomChainedCredential();
        var accessToken = await credential.GetTokenAsync(new TokenRequestContext([apiScope]), CancellationToken.None);

        // GHCP4A Prod app endpoint
        const string url = "https://aiservice.ghcpaz-prod.azure.com/api/azurecli/generate";

        var requestBody = new AzureCliGenerateRequest()
        {
            Question = intent,
            History = [],
            EnableParameterInjection = true
        };
        var content = new StringContent(
                JsonSerializer.Serialize(requestBody, ExtensionJsonContext.Default.AzureCliGenerateRequest),
                Encoding.UTF8,
                "application/json");

        using HttpRequestMessage requestMessage = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(url),
            Content = content
        };
        requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Token);
        HttpResponseMessage responseMessage = await _httpClientService.DefaultClient.SendAsync(requestMessage);
        return responseMessage;
    }
}
