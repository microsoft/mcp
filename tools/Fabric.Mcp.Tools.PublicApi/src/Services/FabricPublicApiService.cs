// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.PublicApi.Models;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Services;

public class FabricPublicApiService(
    ILogger<FabricPublicApiService> logger,
    IResourceProviderService resourceProviderService) : IFabricPublicApiService
{
    private readonly ILogger<FabricPublicApiService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IResourceProviderService _resourceProviderService = resourceProviderService ?? throw new ArgumentNullException(nameof(resourceProviderService));

    private const string PublicAPISpecRepo = "fabric-rest-api-specs";
    private const string APISpecFileName = "swagger.json";
    private const string APISpecDefinitionsFileName = "definitions.json";

    private const string APISpecDefinitionsDirName = "definitions/";
    private const string APISpecExamplesDirName = "examples/";

    private const string FormattedItemDefinitionPath = "item-definitions/{0}-definition.md";
    private const string BaseResourcePath = PublicAPISpecRepo + "/contents/";
    private const string FormattedSpecPath = BaseResourcePath + "{0}/" + APISpecFileName;

    #region IFabricPublicApiService

    public async Task<FabricWorkloadPublicApi> GetWorkloadPublicApis(string workload)
    {
        var workloadApiSpecPath = string.Format(FormattedSpecPath, workload);

        _logger.LogInformation("Getting public API specifications for workload {workload}", workload);

        var workloadSpec = await _resourceProviderService.GetResource(workloadApiSpecPath);

        return new(workloadSpec ?? string.Empty, await GetWorkloadSpecDefinitionsAsync(workload));
    }

    public async Task<IEnumerable<string>> ListWorkloadsAsync()
    {
        var contentDirPath = BaseResourcePath;

        _logger.LogInformation("Listing available Fabric workloads");

        return await _resourceProviderService.ListResourcesInPath(contentDirPath, ResourceType.Directory);
    }

    public async Task<IDictionary<string, string>> GetWorkloadExamplesAsync(string workloadType)
    {
        // Construct the full path: workloadType/examples
        var workloadExamplesDirPath = BaseResourcePath + workloadType + "/" + APISpecExamplesDirName;

        _logger.LogInformation("Getting example files for workload {workloadType}", workloadType);

        var res = new Dictionary<string, string>();

        return await GetExamplesFromDirectoryAsync(workloadExamplesDirPath);
    }

    public string GetWorkloadItemDefinition(string workloadType)
    {
        var workloadItemDefinitionPath = string.Format(FormattedItemDefinitionPath, workloadType);

        _logger.LogInformation("Getting item definition for workload {workloadType}", workloadType);

        return EmbeddedResourceProviderService.GetEmbeddedResource(workloadItemDefinitionPath);
    }

    public IEnumerable<string> GetTopicBestPractices(string topic)
    {
        _logger.LogInformation("Getting best practice resources on {topic}", topic);

        return [EmbeddedResourceProviderService.GetEmbeddedResource(topic)];
    }

    #endregion IFabricPublicApiService

    private async Task<IDictionary<string, string>> GetWorkloadSpecDefinitionsAsync(string workloadType)
    {
        var workloadDirPath = BaseResourcePath + workloadType + '/';

        var content = await _resourceProviderService.ListResourcesInPath(workloadDirPath);

        var res = new Dictionary<string, string>();

        if (content.Contains(APISpecDefinitionsFileName))
        {
            res[APISpecDefinitionsFileName] = await _resourceProviderService.GetResource($"{workloadDirPath}{APISpecDefinitionsFileName}");
        }

        if (content.Contains(APISpecDefinitionsDirName))
        {
            var definitions = await _resourceProviderService.ListResourcesInPath($"{workloadDirPath}{APISpecDefinitionsDirName}");
            foreach (var definition in definitions)
            {

                res[$"{APISpecDefinitionsDirName}{definition}"] = await _resourceProviderService.GetResource($"{APISpecDefinitionsDirName}{definition}");
            }
        }

        return res;
    }

    private async Task<IDictionary<string, string>> GetExamplesFromDirectoryAsync(string directory)
    {
        var res = new Dictionary<string, string>();

        // Check if this is a file (not a directory)
        foreach (var example in await _resourceProviderService.ListResourcesInPath(directory, ResourceType.File))
        {

            res[example] = await _resourceProviderService.GetResource($"{directory}{example}");
        }

        foreach (var subDir in await _resourceProviderService.ListResourcesInPath(directory, ResourceType.Directory))
        {
            var subDirExamples = await GetExamplesFromDirectoryAsync($"{directory}{subDir}/");
            foreach (var (exampleFile, exampleContent) in subDirExamples)
            {
                res[$"{subDir}{exampleFile}"] = exampleContent;
            }
        }
        return res;
    }
}
