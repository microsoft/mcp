// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Core.Services.ProcessExecution;
using Fabric.Mcp.Tools.PublicApi.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Fabric.Mcp.Tools.PublicApi.Services;

public class FabricPublicApiService(ILogger<FabricPublicApiService> logger, IHttpClientService httpClientService) : IFabricPublicApiService
{
    private readonly ILogger<FabricPublicApiService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));

    private const string PublicAPISpecRepo = "fabric-rest-api-specs";
    private const string APISpecFileName = "swagger.json";
    private const string APISpecDefinitionsFileName = "definitions.json";

    private const string APISpecDefinitionsDirName = "definitions";
    private const string APISpecExamplesDirName = "examples";

    private const string BaseGithubUrl = "https://api.github.com/repos/microsoft/";
    private const string BaseRepoUrl = BaseGithubUrl + PublicAPISpecRepo + "/contents/";

    private async Task<JsonElement[]> GetGithubContentArrayAsync(string? requestUrl)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        requestMessage.Headers.Add("User-Agent", "request");

        var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();

        var jsonDoc = JsonDocument.Parse(await httpResponse.Content.ReadAsStringAsync());
        return [.. jsonDoc.RootElement.EnumerateArray()];
    }

    private async Task<string> GetGithubContentAsync(string? requestUrl)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        requestMessage.Headers.Add("User-Agent", "request");

        var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();

        return await httpResponse.Content.ReadAsStringAsync();
    }

    public async Task<FabricWorkloadPublicApi> ListFabricWorkloadPublicApis(string workload)
    {
        var url = BaseRepoUrl + workload;

        _logger.LogInformation("Getting public API specifications for workload {workload}", workload);

        var content = await GetGithubContentArrayAsync(url);

        var swaggerUrl = content
            .Where(item => item.TryGetProperty("name", out var name) && name.GetString() == "swagger.json")
            .Select(item => item.TryGetProperty("download_url", out var downloadUrl) ? downloadUrl.GetString() : null)
            .FirstOrDefault();

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, swaggerUrl);
        requestMessage.Headers.Add("User-Agent", "request");

        var httpResponse = await _httpClientService.DefaultClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();

        var swaggerContent = await httpResponse.Content.ReadAsStringAsync();

        return new(swaggerContent ?? string.Empty, await GetWorkloadSpecDefinitionsAsync(workload));
    }

    private async Task<IDictionary<string, string>> GetWorkloadSpecDefinitionsAsync(string workloadType)
    {
        var url = BaseRepoUrl + workloadType;

        var content = await GetGithubContentArrayAsync(url);

        var res = new Dictionary<string, string>();

        var definitionsJsonItem = content
            .FirstOrDefault(item => item.TryGetProperty("name", out var name) && name.GetString() == "definitions.json");
        
        if (!definitionsJsonItem.Equals(default(JsonElement)))
        {
            if (definitionsJsonItem.TryGetProperty("download_url", out var downloadUrl))
            {
                res["definitions.json"] = downloadUrl.GetString()!;
            }
        }

        var definitionsItem = content
            .FirstOrDefault(item => item.TryGetProperty("name", out var name) && name.GetString() == "definitions");
        
        if (!definitionsItem.Equals(default(JsonElement)))
        {
            if (definitionsItem.TryGetProperty("url", out var definitionsUrl))
            {
                var definitions = await GetGithubContentArrayAsync(definitionsUrl.GetString());
                foreach (var definition in definitions)
                {
                    if (definition.TryGetProperty("name", out var defName) && 
                        definition.TryGetProperty("download_url", out var defDownloadUrl))
                    {
                        res[$"{APISpecDefinitionsDirName}/{defName.GetString()!}"] = defDownloadUrl.GetString()!;
                    }
                }
            }
        }

        foreach (var (definitionPath, definitionUrl) in res)
        {
            res[definitionPath] = await GetGithubContentAsync(definitionUrl);
        }

        return res;
    }

    public async Task<IEnumerable<string>> ListFabricWorkloadsAsync()
    {
        var url = BaseRepoUrl;

        _logger.LogInformation("Listing available Fabric workloads");

        var content = await GetGithubContentArrayAsync(url);

        return content.Where(item => 
                item.TryGetProperty("type", out var type) && type.GetString() == "dir" && 
                item.TryGetProperty("name", out var _))
                      .Select(item => item.GetProperty("name").GetString() ?? string.Empty);
    }

    public async Task<IDictionary<string, string>> GetExamplesAsync(string workloadType)
    {
        
        // Construct the full path: workloadType/examples
        var fullPath = $"{workloadType}/{APISpecExamplesDirName}";
        var url = BaseRepoUrl + fullPath;

        _logger.LogInformation("Getting example files for workload {workloadType}", workloadType);

        var res = new Dictionary<string, string>();

        var content = await GetGithubContentArrayAsync(url);

        // Check if this is a file (not a directory)
        foreach (var item in content)
        {
            if (item.TryGetProperty("name", out var name))
            {
                if (item.TryGetProperty("type", out var type) && type.GetString()! == "file" &&
                    item.TryGetProperty("download_url", out var downloadUrl))
                {
                    res[name.GetString()!] = await GetGithubContentAsync(downloadUrl.GetString());
                }
                else if (type.GetString()! == "dir")
                {
                    // Recursively get examples from subdirectories
                    var subExamples = await GetExamplesAsync($"{workloadType}/{name.GetString()!}");
                    foreach (var (subPath, subContent) in subExamples)
                    {
                        res[$"{name.GetString()!}/{subPath}"] = subContent;
                    }
                }
            }
        }

        return res;
    }
}
