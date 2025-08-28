// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Core.Services.ProcessExecution;
using Fabric.Mcp.Tools.PublicApi.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Fabric.Mcp.Tools.PublicApi.Services;

public class FabricPublicApiService(ILogger<FabricPublicApiService> logger, IHttpClientService httpClientService) : IFabricPublicApiService
{
    private readonly ILogger<FabricPublicApiService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));

    private const string PublicAPISpecRepo = "fabric-rest-api-specs";
    private const string GithubRepoContentUrlFormated = "https://api.github.com/repos/microsoft/{0}/contents/{1}";

    private async Task<JArray> GetGithubContentAsync(string? requestUrl)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        requestMessage.Headers.Add("User-Agent", "request");

        var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();

        return JArray.Parse(await httpResponse.Content.ReadAsStringAsync());
    }

    public async Task<FabricWorkloadPublicApi> ListFabricWorkloadPublicApis(string workload)
    {
        if (workload.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return new (string.Empty, string.Empty, await ListFabricWorkloadsAsync());
        }

        var url = string.Format(GithubRepoContentUrlFormated, PublicAPISpecRepo, workload);

        _logger.LogInformation("Getting public API specifications for workload {workload}", workload);

        var content = await GetGithubContentAsync(url);

        var swaggerUrl = content.SingleOrDefault(item => item["name"] != null && item["name"]!.Value<string>() == "swagger.json")?["download_url"]?.Value<string>();
        var modelDefinitionsUrl = content.SingleOrDefault(item => item["name"] != null && item["name"]!.Value<string>() == "definitions.json")?["download_url"]?.Value<string>() ??
                                  content.SingleOrDefault(item => item["name"] != null && item["name"]!.Value<string>() == "definitions")?["url"]?.Value<string>();
        var examplesLocation = content.SingleOrDefault(item => item["name"] != null && item["name"]!.Value<string>() == "examples")?["url"]?.Value<string>();

        content = await GetGithubContentAsync(examplesLocation);
        var exampleUrls = content.Select(item => item["download_url"]?.Value<string>() ?? string.Empty).Where(url => !string.IsNullOrEmpty(url));

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, swaggerUrl);
        requestMessage.Headers.Add("User-Agent", "request");

        var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();

        var swaggerContent = await httpResponse.Content.ReadAsStringAsync();

        return new (swaggerContent ?? string.Empty, modelDefinitionsUrl ?? string.Empty, exampleUrls ?? []);
    }

    public async Task<IEnumerable<string>> ListFabricWorkloadsAsync()
    {
        var url = string.Format(GithubRepoContentUrlFormated, PublicAPISpecRepo, string.Empty);

        _logger.LogInformation("Listing available Fabric workloads");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        requestMessage.Headers.Add("User-Agent", "request");

        var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();

        var content = JArray.Parse(await httpResponse.Content.ReadAsStringAsync());

        return content.Where(item => item["type"] != null && item["type"]!.Value<string>() == "dir" && item["name"] != null)
                      .Select(item => item["name"]!.ToString());
    }
}
