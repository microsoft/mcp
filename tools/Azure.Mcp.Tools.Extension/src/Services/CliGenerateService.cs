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
        // AzCli copilot 1P app scope
        const string apiScope = "a5ede409-60d3-4a6c-93e6-eb2e7271e8e3/.default";

        var credential = new CustomChainedCredential();
        var accessToken = await credential.GetTokenAsync(new TokenRequestContext([apiScope]), CancellationToken.None);

        // AzCli copilot API endpoint
        const string url = "https://azclis-copilot-apim-ppe-eus.azure-api.net/azcli/copilot";

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
