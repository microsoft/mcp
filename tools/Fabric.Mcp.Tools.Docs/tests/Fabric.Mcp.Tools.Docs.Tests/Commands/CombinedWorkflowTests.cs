// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Docs.Commands;
using Fabric.Mcp.Tools.Docs.Commands.BestPractices;
using Fabric.Mcp.Tools.Docs.Commands.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Xunit;

namespace Fabric.Mcp.Tools.Docs.Tests.Commands;

/// <summary>
/// Combined workflow tests that exercise real service implementations (no mocks).
/// Commands use the real EmbeddedResourceProviderService and FabricPublicApiService
/// backed by assembly-embedded resources.
/// </summary>
public class CombinedWorkflowTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ListItemTypesCommand _listItemTypesCommand;
    private readonly GetItemApisCommand _getItemApisCommand;
    private readonly GetItemDefinitionCommand _getItemDefinitionCommand;
    private readonly GetExamplesCommand _getExamplesCommand;

    public CombinedWorkflowTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IResourceProviderService, EmbeddedResourceProviderService>();
        services.AddSingleton<IFabricPublicApiService, FabricPublicApiService>();
        services.AddSingleton<ListItemTypesCommand>();
        services.AddSingleton<GetItemApisCommand>();
        services.AddSingleton<GetItemDefinitionCommand>();
        services.AddSingleton<GetExamplesCommand>();
        _serviceProvider = services.BuildServiceProvider();

        _listItemTypesCommand = _serviceProvider.GetRequiredService<ListItemTypesCommand>();
        _getItemApisCommand = _serviceProvider.GetRequiredService<GetItemApisCommand>();
        _getItemDefinitionCommand = _serviceProvider.GetRequiredService<GetItemDefinitionCommand>();
        _getExamplesCommand = _serviceProvider.GetRequiredService<GetExamplesCommand>();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ListItemTypes_ThenGetApisForEach_ReturnsApiSpecsForAllItemTypes()
    {
        // Step 1: List all item types using the real service
        var listResult = await ExecuteCommandAsync(_listItemTypesCommand);

        var itemTypes = ValidateAndDeserializeItemTypesResponse(listResult);
        Assert.NotEmpty(itemTypes);

        // Step 2: For each item type, get its API spec
        foreach (var itemType in itemTypes)
        {
            var apiResult = await ExecuteCommandAsync(_getItemApisCommand, "--item-type", itemType);

            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);
        }
    }

    [Fact]
    public async Task ListItemTypes_ThenGetDefinitionForEach_ReturnsOrReportsNotFound()
    {
        // Step 1: List all item types
        var listResult = await ExecuteCommandAsync(_listItemTypesCommand);

        var itemTypes = ValidateAndDeserializeItemTypesResponse(listResult);

        // Step 2: For each item type, get its item definition.
        // Not all item types have embedded item definitions, so we accept OK or NotFound.
        var successCount = 0;
        foreach (var itemType in itemTypes)
        {
            var defResult = await ExecuteCommandAsync(_getItemDefinitionCommand, "--item-type", itemType);

            Assert.True(
                defResult.Status == HttpStatusCode.OK || defResult.Status == HttpStatusCode.NotFound,
                $"Unexpected status {defResult.Status} for item type '{itemType}': {defResult.Message}");

            if (defResult.Status == HttpStatusCode.OK)
            {
                Assert.NotNull(defResult.Results);
                successCount++;
            }
        }

        // The number of item types that successfully returned a definition should
        // match the number of *-definition.md files in Resources/item-definitions
        // that are reachable from the listed item types.
        // When a new definition file is added, the corresponding item type must
        // also be present in the API specs for the definition to be discoverable.
        var assembly = typeof(EmbeddedResourceProviderService).Assembly;
        var allDefinitionResources = assembly.GetManifestResourceNames()
            .Where(name => name.Contains("item-definitions/") && name.EndsWith("-definition.md", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.True(allDefinitionResources.Length > 0, "Expected embedded item-definition resources to exist");

        // Count how many definition files are reachable from listed item types
        // via the same pattern used by GetItemDefinition
        var matchedDefinitions = new HashSet<string>();
        foreach (var itemType in itemTypes)
        {
            var pattern = FabricPublicApiService.BuildItemDefinitionPattern(itemType);
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            foreach (var resource in allDefinitionResources)
            {
                if (regex.IsMatch(resource))
                {
                    matchedDefinitions.Add(resource);
                }
            }
        }

        // Every reachable definition should have been successfully retrieved
        Assert.Equal(matchedDefinitions.Count, successCount);

        // Flag any orphaned definition files that no item type can reach.
        // dbtjob, hlscohort, orgapp, and orgappaudience have no corresponding item type API spec directories.
        var knownOrphans = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "dbtjob-definition.md", "hlscohort-definition.md", "orgapp-definition.md", "orgappaudience-definition.md" };
        var orphaned = allDefinitionResources
            .Except(matchedDefinitions)
            .Where(r => !knownOrphans.Contains(Path.GetFileName(r)!))
            .ToArray();
        Assert.True(orphaned.Length == 0,
            $"The following {orphaned.Length} item-definition file(s) have no matching item type in the API specs: " +
            string.Join(", ", orphaned.Select(Path.GetFileName)));
    }

    [Fact]
    public async Task ListItemTypes_ThenGetApisAndDefinitionsForEach_ReturnsAllData()
    {
        // Step 1: List all item types
        var listResult = await ExecuteCommandAsync(_listItemTypesCommand);

        var itemTypes = ValidateAndDeserializeItemTypesResponse(listResult);

        // Step 2: For each item type, get both API spec and item definition
        foreach (var itemType in itemTypes)
        {
            var apiResult = await ExecuteCommandAsync(_getItemApisCommand, "--item-type", itemType);

            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);

            var defResult = await ExecuteCommandAsync(_getItemDefinitionCommand, "--item-type", itemType);

            // Item definitions may not exist for every item type
            Assert.True(
                defResult.Status == HttpStatusCode.OK || defResult.Status == HttpStatusCode.NotFound,
                $"Unexpected status {defResult.Status} for item type '{itemType}'");
        }
    }

    [Fact]
    public async Task ListItemTypes_ThenGetApisDefinitionsAndExamples_FullWorkflow()
    {
        // Step 1: List item types
        var listResult = await ExecuteCommandAsync(_listItemTypesCommand);

        var itemTypes = ValidateAndDeserializeItemTypesResponse(listResult);

        // Step 2: For each item type, get APIs, definitions, and examples
        foreach (var itemType in itemTypes)
        {
            // API spec - should always succeed for listed item types
            var apiResult = await ExecuteCommandAsync(_getItemApisCommand, "--item-type", itemType);

            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);

            // Item definition - may or may not exist
            var defResult = await ExecuteCommandAsync(_getItemDefinitionCommand, "--item-type", itemType);

            Assert.True(
                defResult.Status == HttpStatusCode.OK || defResult.Status == HttpStatusCode.NotFound,
                $"Unexpected definition status {defResult.Status} for item type '{itemType}'");

            // Examples - should always succeed (returns empty dict if none exist)
            var exResult = await ExecuteCommandAsync(_getExamplesCommand, "--item-type", itemType);

            Assert.Equal(HttpStatusCode.OK, exResult.Status);
            Assert.NotNull(exResult.Results);
        }
    }

    [Fact]
    public async Task ListItemTypes_DoesNotReturnCommon()
    {
        // The service filters out the "common" pseudo-item-type
        var listResult = await ExecuteCommandAsync(_listItemTypesCommand);

        var itemTypes = ValidateAndDeserializeItemTypesResponse(listResult);
        Assert.DoesNotContain("common", itemTypes, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetApis_WithCommonItemType_ReturnsNotFound()
    {
        // "common" is explicitly rejected with a helpful message
        var apiResult = await ExecuteCommandAsync(_getItemApisCommand, "--item-type", "common");

        Assert.Equal(HttpStatusCode.NotFound, apiResult.Status);
        Assert.Contains("common", apiResult.Message);
        Assert.Contains("platform", apiResult.Message);
    }

    [Fact]
    public async Task GetApis_WithNonexistentItemType_ReturnsError()
    {
        var apiResult = await ExecuteCommandAsync(_getItemApisCommand, "--item-type", "this-item-type-does-not-exist");

        Assert.NotEqual(HttpStatusCode.OK, apiResult.Status);
    }

    [Fact]
    public async Task ListItemTypes_ThenGetApisForEach_ApiSpecContainsValidJson()
    {
        // Step 1: List item types
        var listResult = await ExecuteCommandAsync(_listItemTypesCommand);

        var itemTypes = ValidateAndDeserializeItemTypesResponse(listResult);

        // Step 2: For each item type, verify the API spec result contains parseable JSON content
        foreach (var itemType in itemTypes)
        {
            var apiResult = await ExecuteCommandAsync(_getItemApisCommand, "--item-type", itemType);

            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);

            // Deserialize the command wrapper and verify its API specification contains valid JSON.
            var json = JsonSerializer.Serialize(apiResult.Results);
            var result = JsonSerializer.Deserialize(json, FabricJsonContext.Default.GetItemApisCommandResult);
            Assert.NotNull(result);
            var apiSpecJson = result.PublicApi.apiSpecification;
            Assert.False(string.IsNullOrEmpty(apiSpecJson),
                $"API specification for item type '{itemType}' should not be empty");

            // Verify the swagger spec inside apiSpecification is valid JSON
            using var swaggerDoc = JsonDocument.Parse(apiSpecJson!);
            Assert.NotEqual(JsonValueKind.Undefined, swaggerDoc.RootElement.ValueKind);
        }
    }

    [Fact]
    public async Task ListItemTypes_ResultsAreConsistentAcrossMultipleInvocations()
    {
        // Verifies that calling the same tool twice returns consistent results
        var result1 = await ExecuteCommandAsync(_listItemTypesCommand);
        var result2 = await ExecuteCommandAsync(_listItemTypesCommand);

        var itemTypes1 = ValidateAndDeserializeItemTypesResponse(result1);
        var itemTypes2 = ValidateAndDeserializeItemTypesResponse(result2);

        Assert.Equal(itemTypes1.OrderBy(w => w), itemTypes2.OrderBy(w => w));
    }

    private static Task<CommandResponse> ExecuteCommandAsync(IBaseCommand command, params string[] args)
        => command.ExecuteAsync(new(), command.GetCommand().Parse(args), TestContext.Current.CancellationToken);

    private static IEnumerable<string> ValidateAndDeserializeItemTypesResponse(CommandResponse response)
    {
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FabricJsonContext.Default.ItemTypeListCommandResult);
        Assert.NotNull(result);
        var itemTypes = result.ItemTypes;
        Assert.NotEmpty(itemTypes);
        return itemTypes;
    }
}
