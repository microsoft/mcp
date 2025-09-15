using Azure.Mcp.Core.Services.Http;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Services
{
    public class NetworkResourceProviderService(ILogger<NetworkResourceProviderService> logger, IHttpClientService httpClientService) : IResourceProviderService
    {
        private readonly ILogger<NetworkResourceProviderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));

        private const string BaseGithubUrl = "https://api.github.com/repos/microsoft/";


        public async Task<string> GetResource(string resourceName)
        {
            _logger.LogInformation("Fetching network resource: {ResourceName}", resourceName);

            var resourceJson = await GetResourceFromGithub(BaseGithubUrl + resourceName);

            if (resourceJson.TryGetProperty("download_url", out var downloadUrl) && !string.IsNullOrEmpty(downloadUrl.GetString()))
            {
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, downloadUrl.GetString());
                requestMessage.Headers.Add("User-Agent", "request");

                var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage);
                httpResponse.EnsureSuccessStatusCode();

                return await httpResponse.Content.ReadAsStringAsync();
            }

            throw new FileNotFoundException($"Resource {resourceName} was not found.");
        }

        public async Task<string[]> ListResourcesInPath(string path, ResourceType? filterResources = null)
        {
            _logger.LogInformation("Listing resources in path: {Path}", path);

            var contentArray = await GetGithubContentArrayAsync(BaseGithubUrl + path);

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

        private async Task<JsonElement> GetResourceFromGithub(string resourceName)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, resourceName);
            requestMessage.Headers.Add("User-Agent", "request");

            var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage);
            httpResponse.EnsureSuccessStatusCode();

            return JsonDocument.Parse(await httpResponse.Content.ReadAsStringAsync()).RootElement;
        }

        private async Task<JsonElement[]> GetGithubContentArrayAsync(string? requestUrl)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            requestMessage.Headers.Add("User-Agent", "request");

            var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage);
            httpResponse.EnsureSuccessStatusCode();

            var jsonDoc = JsonDocument.Parse(await httpResponse.Content.ReadAsStringAsync());
            return [.. jsonDoc.RootElement.EnumerateArray()];
        }
    }
}
