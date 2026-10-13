// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Fabric.Mcp.Tools.Docs.Models;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.Docs.Services;

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

    public async Task<FabricPublicApi> GetPublicApis(string itemType, CancellationToken cancellationToken)
    {
        var apiSpecPath = string.Format(FormattedSpecPath, itemType);

        _logger.LogInformation("Getting public API specifications for item type {itemType}", itemType);

        var spec = await _resourceProviderService.GetResource(apiSpecPath, cancellationToken);

        return new(spec ?? string.Empty, await GetSpecDefinitionsAsync(itemType, cancellationToken));
    }

    public async Task<IEnumerable<string>> ListItemTypesAsync(CancellationToken cancellationToken)
    {
        var contentDirPath = BaseResourcePath;

        _logger.LogInformation("Listing available Fabric item types");

        var itemTypes = await _resourceProviderService.ListResourcesInPath(contentDirPath, ResourceType.Directory, cancellationToken);
        return itemTypes.Where(w => !string.Equals(w, "common", StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    public async Task<IDictionary<string, string>> GetExamplesAsync(string itemType, CancellationToken cancellationToken)
    {
        // Construct the full path: itemType/examples
        var examplesDirPath = BaseResourcePath + itemType + "/" + APISpecExamplesDirName;

        _logger.LogInformation("Getting example files for item type {itemType}", itemType);

        return await GetExamplesFromDirectoryAsync(examplesDirPath, cancellationToken);
    }

    public string GetItemDefinition(string itemType)
    {
        var itemDefinitionPattern = BuildItemDefinitionPattern(itemType);

        _logger.LogInformation("Getting item definition for item type {itemType}", itemType);

        return EmbeddedResourceProviderService.GetEmbeddedResource(itemDefinitionPattern);
    }

    public IEnumerable<string> GetTopicBestPractices(string topic)
    {
        _logger.LogInformation("Getting best practice resources on {topic}", topic);

        return [EmbeddedResourceProviderService.GetEmbeddedResource(topic)];
    }

    #endregion IFabricPublicApiService

    /// <summary>
    /// Builds a regex pattern for matching item definition resources.
    /// Item type names from directory listings are camelCase (e.g., "kqlDatabase"),
    /// while item definition files use kebab-case (e.g., "kql-database-definition.md").
    /// This method handles three kinds of naming mismatches:
    /// 1. camelCase boundaries → optional hyphens plus extra kebab-case segments
    ///    (e.g., "mirroredAzureDatabricksCatalog" matches "mirrored-azuredatabricks-unitycatalog")
    /// 2. All-lowercase item type names → optional hyphens between every character
    ///    (e.g., "sparkjob" matches "spark-job")
    /// 3. Trailing "definition" in item type names is stripped since the file suffix
    ///    already includes "-definition.md" (e.g., "sparkjobdefinition" → "sparkjob")
    /// </summary>
    internal static string BuildItemDefinitionPattern(string itemType)
    {
        // Strip trailing "definition" since the file naming convention already includes "-definition.md"
        if (itemType.EndsWith("definition", StringComparison.OrdinalIgnoreCase))
        {
            itemType = itemType[..^10];
        }

        var sb = new StringBuilder("item-definitions/");
        for (int i = 0; i < itemType.Length; i++)
        {
            if (i > 0)
            {
                if (char.IsUpper(itemType[i]))
                {
                    // At camelCase boundaries, allow extra lowercase chars and hyphens
                    // to handle naming inconsistencies (e.g., "AzureDatabricksCatalog"
                    // matching "azuredatabricks-unitycatalog")
                    sb.Append("[-a-z]*?");
                }
                else
                {
                    // Between consecutive lowercase chars, allow optional hyphen
                    // to handle kebab-case variants (e.g., "sparkjob" matching "spark-job")
                    sb.Append("-?");
                }
            }

            sb.Append(char.ToLowerInvariant(itemType[i]));
        }

        sb.Append("-definition\\.md");
        return sb.ToString();
    }

    private async Task<IDictionary<string, string>> GetSpecDefinitionsAsync(string itemType, CancellationToken cancellationToken)
    {
        var specDirPath = BaseResourcePath + itemType + '/';

        var content = await _resourceProviderService.ListResourcesInPath(specDirPath, cancellationToken: cancellationToken);

        var res = new Dictionary<string, string>();

        if (content.Contains(APISpecDefinitionsFileName))
        {
            res[APISpecDefinitionsFileName] = await _resourceProviderService.GetResource($"{specDirPath}{APISpecDefinitionsFileName}", cancellationToken);
        }

        if (content.Contains(APISpecDefinitionsDirName))
        {
            var definitions = await _resourceProviderService.ListResourcesInPath($"{specDirPath}{APISpecDefinitionsDirName}", cancellationToken: cancellationToken);
            foreach (var definition in definitions)
            {

                res[$"{APISpecDefinitionsDirName}{definition}"] = await _resourceProviderService.GetResource($"{APISpecDefinitionsDirName}{definition}", cancellationToken);
            }
        }

        return res;
    }

    private async Task<IDictionary<string, string>> GetExamplesFromDirectoryAsync(string directory, CancellationToken cancellationToken)
    {
        var res = new Dictionary<string, string>();

        // Check if this is a file (not a directory)
        foreach (var example in await _resourceProviderService.ListResourcesInPath(directory, ResourceType.File, cancellationToken))
        {

            res[example] = await _resourceProviderService.GetResource($"{directory}{example}", cancellationToken);
        }

        foreach (var subDir in await _resourceProviderService.ListResourcesInPath(directory, ResourceType.Directory, cancellationToken))
        {
            var subDirExamples = await GetExamplesFromDirectoryAsync($"{directory}{subDir}/", cancellationToken);
            foreach (var (exampleFile, exampleContent) in subDirExamples)
            {
                res[$"{subDir}{exampleFile}"] = exampleContent;
            }
        }
        return res;
    }
}
