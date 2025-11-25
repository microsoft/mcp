using System.Text.Json;
using System.Threading;
using Azure.Mcp.Core.Services.Http;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Services
{
    public class NetworkResourceProviderService(ILogger<NetworkResourceProviderService> logger, IHttpClientService httpClientService) : IResourceProviderService
    {
        private readonly ILogger<NetworkResourceProviderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));

        private const string BaseGithubUrl = "https://api.github.com/repos/microsoft/";


        public async Task<string> GetResource(string resourceName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching network resource: {ResourceName}", resourceName);

            var resourceJson = await GetResourceFromGithub(BaseGithubUrl + resourceName, cancellationToken);

            if (resourceJson.TryGetProperty("download_url", out var downloadUrl) && !string.IsNullOrEmpty(downloadUrl.GetString()))
            {
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, downloadUrl.GetString());
                requestMessage.Headers.Add("User-Agent", "request");

                using (var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage, cancellationToken))
                {
                    httpResponse.EnsureSuccessStatusCode();

                    return await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                }
            }

            throw new FileNotFoundException($"Resource {resourceName} was not found.");
        }

        public async Task<string[]> ListResourcesInPath(string path, ResourceType? filterResources = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Listing resources in path: {Path}", path);

            var contentArray = await GetGithubContentArrayAsync(BaseGithubUrl + path, cancellationToken);

            return contentArray
                .Where(content => filterResources switch
                {
                    ResourceType.File => content.TryGetProperty("type", out var type) && type.GetString()! == "file",
                    ResourceType.Directory => content.TryGetProperty("type", out var type) && type.GetString()! == "dir",
                    _ => true
                })
                .Select(item => item.GetProperty("name").GetString() ?? string.Empty)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray();
        }

        private async Task<JsonElement> GetResourceFromGithub(string resourceName, CancellationToken cancellationToken)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, resourceName);
            requestMessage.Headers.Add("User-Agent", "request");

            using (var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage, cancellationToken))
            {
                httpResponse.EnsureSuccessStatusCode();

                await using var content = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
                using (var jsonDoc = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken))
                {
                    return jsonDoc.RootElement.Clone();
                }
            }
        }

        private async Task<JsonElement[]> GetGithubContentArrayAsync(string? requestUrl, CancellationToken cancellationToken)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            requestMessage.Headers.Add("User-Agent", "request");

            using (var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage, cancellationToken))
            {
                httpResponse.EnsureSuccessStatusCode();

                await using var content = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
                using (var jsonDoc = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken))
                {
                    return jsonDoc.RootElement.EnumerateArray()
                        .Select(element => element.Clone())
                        .ToArray();
                }
            }
        }
    }
}
