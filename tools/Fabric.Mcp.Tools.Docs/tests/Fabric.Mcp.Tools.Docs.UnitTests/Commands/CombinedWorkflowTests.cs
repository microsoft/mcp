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
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.UnitTests.Commands;

/// <summary>
/// Combined workflow tests that exercise real service implementations (no mocks).
/// Commands use the real EmbeddedResourceProviderService and FabricPublicApiService
/// backed by assembly-embedded resources.
/// </summary>
public class CombinedWorkflowTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ListWorkloadsCommand _listWorkloadsCommand;
    private readonly GetWorkloadApisCommand _getWorkloadApisCommand;
    private readonly GetWorkloadDefinitionCommand _getWorkloadDefinitionCommand;
    private readonly GetExamplesCommand _getExamplesCommand;

    public CombinedWorkflowTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IResourceProviderService, EmbeddedResourceProviderService>();
        services.AddSingleton<IFabricPublicApiService, FabricPublicApiService>();
        services.AddSingleton<ListWorkloadsCommand>();
        services.AddSingleton<GetWorkloadApisCommand>();
        services.AddSingleton<GetWorkloadDefinitionCommand>();
        services.AddSingleton<GetExamplesCommand>();
        _serviceProvider = services.BuildServiceProvider();

        _listWorkloadsCommand = _serviceProvider.GetRequiredService<ListWorkloadsCommand>();
        _getWorkloadApisCommand = _serviceProvider.GetRequiredService<GetWorkloadApisCommand>();
        _getWorkloadDefinitionCommand = _serviceProvider.GetRequiredService<GetWorkloadDefinitionCommand>();
        _getExamplesCommand = _serviceProvider.GetRequiredService<GetExamplesCommand>();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ListWorkloads_ThenGetApisForEach_ReturnsApiSpecsForAllWorkloads()
    {
        // Step 1: List all workloads using the real service
        var listResult = await ExecuteCommandAsync(_listWorkloadsCommand);

        var workloads = ValidateAndDeserializeWorkloadsResponse(listResult);
        Assert.NotEmpty(workloads);

        // Step 2: For each workload, get its API spec
        foreach (var workload in workloads)
        {
            var apiResult = await ExecuteCommandAsync(_getWorkloadApisCommand, "--workload-type", workload);

            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);
        }
    }

    [Fact]
    public async Task ListWorkloads_ThenGetDefinitionForEach_ReturnsOrReportsNotFound()
    {
        // Step 1: List all workloads
        var listResult = await ExecuteCommandAsync(_listWorkloadsCommand);

        var workloads = ValidateAndDeserializeWorkloadsResponse(listResult);

        // Step 2: For each workload, get its item definition.
        // Not all workloads have embedded item definitions, so we accept OK or NotFound.
        var successCount = 0;
        foreach (var workload in workloads)
        {
            var defResult = await ExecuteCommandAsync(_getWorkloadDefinitionCommand, "--workload-type", workload);

            Assert.True(
                defResult.Status == HttpStatusCode.OK || defResult.Status == HttpStatusCode.NotFound,
                $"Unexpected status {defResult.Status} for workload '{workload}': {defResult.Message}");

            if (defResult.Status == HttpStatusCode.OK)
            {
                Assert.NotNull(defResult.Results);
                successCount++;
            }
        }

        // The number of workloads that successfully returned a definition should
        // match the number of *-definition.md files in Resources/item-definitions
        // that are reachable from the listed workloads.
        // When a new definition file is added, the corresponding workload must
        // also be present in the API specs for the definition to be discoverable.
        var assembly = typeof(EmbeddedResourceProviderService).Assembly;
        var allDefinitionResources = assembly.GetManifestResourceNames()
            .Where(name => name.Contains("item-definitions/") && name.EndsWith("-definition.md", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.True(allDefinitionResources.Length > 0, "Expected embedded item-definition resources to exist");

        // Count how many definition files are reachable from listed workloads
        // via the same pattern used by GetWorkloadItemDefinition
        var matchedDefinitions = new HashSet<string>();
        foreach (var workload in workloads)
        {
            var pattern = FabricPublicApiService.BuildItemDefinitionPattern(workload);
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

        // Flag any orphaned definition files that no workload can reach.
        // dbtjob and hlscohort have no corresponding workload API spec directories.
        var knownOrphans = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "dbtjob-definition.md", "hlscohort-definition.md" };
        var orphaned = allDefinitionResources
            .Except(matchedDefinitions)
            .Where(r => !knownOrphans.Contains(Path.GetFileName(r)!))
            .ToArray();
        Assert.True(orphaned.Length == 0,
            $"The following {orphaned.Length} item-definition file(s) have no matching workload in the API specs: " +
            string.Join(", ", orphaned.Select(Path.GetFileName)));
    }

    [Fact]
    public async Task ListWorkloads_ThenGetApisAndDefinitionsForEach_ReturnsAllData()
    {
        // Step 1: List all workloads
        var listResult = await ExecuteCommandAsync(_listWorkloadsCommand);

        var workloads = ValidateAndDeserializeWorkloadsResponse(listResult);

        // Step 2: For each workload, get both API spec and item definition
        foreach (var workload in workloads)
        {
            var apiResult = await ExecuteCommandAsync(_getWorkloadApisCommand, "--workload-type", workload);

            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);

            var defResult = await ExecuteCommandAsync(_getWorkloadDefinitionCommand, "--workload-type", workload);

            // Item definitions may not exist for every workload
            Assert.True(
                defResult.Status == HttpStatusCode.OK || defResult.Status == HttpStatusCode.NotFound,
                $"Unexpected status {defResult.Status} for workload '{workload}'");
        }
    }

    [Fact]
    public async Task ListWorkloads_ThenGetApisDefinitionsAndExamples_FullWorkflow()
    {
        // Step 1: List workloads
        var listResult = await ExecuteCommandAsync(_listWorkloadsCommand);

        var workloads = ValidateAndDeserializeWorkloadsResponse(listResult);

        // Step 2: For each workload, get APIs, definitions, and examples
        foreach (var workload in workloads)
        {
            // API spec - should always succeed for listed workloads
            var apiResult = await ExecuteCommandAsync(_getWorkloadApisCommand, "--workload-type", workload);

            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);

            // Item definition - may or may not exist
            var defResult = await ExecuteCommandAsync(_getWorkloadDefinitionCommand, "--workload-type", workload);

            Assert.True(
                defResult.Status == HttpStatusCode.OK || defResult.Status == HttpStatusCode.NotFound,
                $"Unexpected definition status {defResult.Status} for workload '{workload}'");

            // Examples - should always succeed (returns empty dict if none exist)
            var exResult = await ExecuteCommandAsync(_getExamplesCommand, "--workload-type", workload);

            Assert.Equal(HttpStatusCode.OK, exResult.Status);
            Assert.NotNull(exResult.Results);
        }
    }

    [Fact]
    public async Task ListWorkloads_DoesNotReturnCommon()
    {
        // The service filters out the "common" pseudo-workload
        var listResult = await ExecuteCommandAsync(_listWorkloadsCommand);

        var workloads = ValidateAndDeserializeWorkloadsResponse(listResult);
        Assert.DoesNotContain("common", workloads, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetApis_WithCommonWorkloadType_ReturnsNotFound()
    {
        // "common" is explicitly rejected with a helpful message
        var apiResult = await ExecuteCommandAsync(_getWorkloadApisCommand, "--workload-type", "common");

        Assert.Equal(HttpStatusCode.NotFound, apiResult.Status);
        Assert.Contains("common", apiResult.Message);
        Assert.Contains("platform", apiResult.Message);
    }

    [Fact]
    public async Task GetApis_WithNonexistentWorkload_ReturnsError()
    {
        var apiResult = await ExecuteCommandAsync(_getWorkloadApisCommand, "--workload-type", "this-workload-does-not-exist");

        Assert.NotEqual(HttpStatusCode.OK, apiResult.Status);
    }

    [Fact]
    public async Task ListWorkloads_ThenGetApisForEach_ApiSpecContainsValidJson()
    {
        // Step 1: List workloads
        var listResult = await ExecuteCommandAsync(_listWorkloadsCommand);

        var workloads = ValidateAndDeserializeWorkloadsResponse(listResult);

        // Step 2: For each workload, verify the API spec result contains parseable JSON content
        foreach (var workload in workloads)
        {
            var apiResult = await ExecuteCommandAsync(_getWorkloadApisCommand, "--workload-type", workload);

            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);

            // Serialize the result to JSON and verify it contains an apiSpecification field
            var json = JsonSerializer.Serialize(apiResult.Results);
            using var doc = JsonDocument.Parse(json);
            Assert.True(doc.RootElement.TryGetProperty("apiSpecification", out var apiSpecElement),
                $"API result for workload '{workload}' should contain 'apiSpecification'");
            var apiSpecJson = apiSpecElement.GetString();
            Assert.False(string.IsNullOrEmpty(apiSpecJson),
                $"API specification for workload '{workload}' should not be empty");

            // Verify the swagger spec inside apiSpecification is valid JSON
            using var swaggerDoc = JsonDocument.Parse(apiSpecJson!);
            Assert.NotEqual(JsonValueKind.Undefined, swaggerDoc.RootElement.ValueKind);
        }
    }

    [Fact]
    public async Task ListWorkloads_ResultsAreConsistentAcrossMultipleInvocations()
    {
        // Verifies that calling the same tool twice returns consistent results
        var result1 = await ExecuteCommandAsync(_listWorkloadsCommand);
        var result2 = await ExecuteCommandAsync(_listWorkloadsCommand);

        var workloads1 = ValidateAndDeserializeWorkloadsResponse(result1);
        var workloads2 = ValidateAndDeserializeWorkloadsResponse(result2);

        Assert.Equal(workloads1.OrderBy(w => w), workloads2.OrderBy(w => w));
    }

    private Task<CommandResponse> ExecuteCommandAsync(IBaseCommand command, params string[] args)
        => command.ExecuteAsync(new(_serviceProvider), command.GetCommand().Parse(args), TestContext.Current.CancellationToken);

    private static IEnumerable<string> ValidateAndDeserializeWorkloadsResponse(CommandResponse response)
    {
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FabricJsonContext.Default.ItemListCommandResult);
        Assert.NotNull(result);
        var workloads = result.Workloads;
        Assert.NotEmpty(workloads);
        return workloads;
    }
}
