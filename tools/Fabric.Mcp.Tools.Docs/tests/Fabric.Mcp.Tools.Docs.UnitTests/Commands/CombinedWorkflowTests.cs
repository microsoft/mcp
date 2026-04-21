// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.CommandLine;
using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Docs.Commands.BestPractices;
using Fabric.Mcp.Tools.Docs.Commands.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Tests.Commands;

/// <summary>
/// Shared fixture for combined workflow tests. Creates the service provider
/// and command instances once, shared across all tests in the class.
/// </summary>
public sealed class CombinedWorkflowFixture : IDisposable
{
    public ServiceProvider ServiceProvider { get; }
    public ListWorkloadsCommand ListWorkloadsCommand { get; }
    public GetWorkloadApisCommand GetWorkloadApisCommand { get; }
    public GetWorkloadDefinitionCommand GetWorkloadDefinitionCommand { get; }
    public GetExamplesCommand GetExamplesCommand { get; }

    private readonly LoggerFactory _loggerFactory;

    public CombinedWorkflowFixture()
    {
        _loggerFactory = new LoggerFactory();
        ListWorkloadsCommand = new ListWorkloadsCommand(_loggerFactory.CreateLogger<ListWorkloadsCommand>());
        GetWorkloadApisCommand = new GetWorkloadApisCommand(_loggerFactory.CreateLogger<GetWorkloadApisCommand>());
        GetWorkloadDefinitionCommand = new GetWorkloadDefinitionCommand(_loggerFactory.CreateLogger<GetWorkloadDefinitionCommand>());
        GetExamplesCommand = new GetExamplesCommand(_loggerFactory.CreateLogger<GetExamplesCommand>());

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IResourceProviderService, EmbeddedResourceProviderService>();
        services.AddSingleton<IFabricPublicApiService, FabricPublicApiService>();
        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
        _loggerFactory.Dispose();
    }
}

/// <summary>
/// Combined workflow tests that exercise real service implementations (no mocks).
/// Commands use the real EmbeddedResourceProviderService and FabricPublicApiService
/// backed by assembly-embedded resources.
/// </summary>
public class CombinedWorkflowTests(CombinedWorkflowFixture fixture) : IClassFixture<CombinedWorkflowFixture>
{
    private readonly ServiceProvider _serviceProvider = fixture.ServiceProvider;
    private readonly ListWorkloadsCommand _listWorkloadsCommand = fixture.ListWorkloadsCommand;
    private readonly GetWorkloadApisCommand _getWorkloadApisCommand = fixture.GetWorkloadApisCommand;
    private readonly GetWorkloadDefinitionCommand _getWorkloadDefinitionCommand = fixture.GetWorkloadDefinitionCommand;
    private readonly GetExamplesCommand _getExamplesCommand = fixture.GetExamplesCommand;

    [Fact]
    public async Task ListWorkloads_ThenGetApisForEach_ReturnsApiSpecsForAllWorkloads()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // Step 1: List all workloads using the real service
        var listContext = new CommandContext(_serviceProvider);
        var listResult = await _listWorkloadsCommand.ExecuteAsync(
            listContext, _listWorkloadsCommand.GetCommand().Parse([]), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, listResult.Status);
        Assert.NotNull(listResult.Results);

        var workloads = DeserializeWorkloads(listResult);
        Assert.NotEmpty(workloads);

        // Step 2: For each workload, get its API spec
        foreach (var workload in workloads)
        {
            var apiContext = new CommandContext(_serviceProvider);
            var apiResult = await _getWorkloadApisCommand.ExecuteAsync(
                apiContext,
                _getWorkloadApisCommand.GetCommand().Parse(["--workload-type", workload]),
                cancellationToken);

            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);
        }
    }

    [Fact]
    public async Task ListWorkloads_ThenGetDefinitionForEach_ReturnsOrReportsNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // Step 1: List all workloads
        var listContext = new CommandContext(_serviceProvider);
        var listResult = await _listWorkloadsCommand.ExecuteAsync(
            listContext, _listWorkloadsCommand.GetCommand().Parse([]), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, listResult.Status);

        var workloads = DeserializeWorkloads(listResult);
        Assert.NotEmpty(workloads);

        // Step 2: For each workload, get its item definition.
        // Not all workloads have embedded item definitions, so we accept OK or NotFound.
        var successCount = 0;
        foreach (var workload in workloads)
        {
            var defContext = new CommandContext(_serviceProvider);
            var defResult = await _getWorkloadDefinitionCommand.ExecuteAsync(
                defContext,
                _getWorkloadDefinitionCommand.GetCommand().Parse(["--workload-type", workload]),
                cancellationToken);

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
        var cancellationToken = TestContext.Current.CancellationToken;

        // Step 1: List all workloads
        var listContext = new CommandContext(_serviceProvider);
        var listResult = await _listWorkloadsCommand.ExecuteAsync(
            listContext, _listWorkloadsCommand.GetCommand().Parse([]), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, listResult.Status);

        var workloads = DeserializeWorkloads(listResult);
        Assert.NotEmpty(workloads);

        // Step 2: For each workload, get both API spec and item definition
        foreach (var workload in workloads)
        {
            var apiContext = new CommandContext(_serviceProvider);
            var apiResult = await _getWorkloadApisCommand.ExecuteAsync(
                apiContext,
                _getWorkloadApisCommand.GetCommand().Parse(["--workload-type", workload]),
                cancellationToken);

            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);

            var defContext = new CommandContext(_serviceProvider);
            var defResult = await _getWorkloadDefinitionCommand.ExecuteAsync(
                defContext,
                _getWorkloadDefinitionCommand.GetCommand().Parse(["--workload-type", workload]),
                cancellationToken);

            // Item definitions may not exist for every workload
            Assert.True(
                defResult.Status == HttpStatusCode.OK || defResult.Status == HttpStatusCode.NotFound,
                $"Unexpected status {defResult.Status} for workload '{workload}'");
        }
    }

    [Fact]
    public async Task ListWorkloads_ThenGetApisDefinitionsAndExamples_FullWorkflow()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // Step 1: List workloads
        var listContext = new CommandContext(_serviceProvider);
        var listResult = await _listWorkloadsCommand.ExecuteAsync(
            listContext, _listWorkloadsCommand.GetCommand().Parse([]), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, listResult.Status);

        var workloads = DeserializeWorkloads(listResult);
        Assert.NotEmpty(workloads);

        // Step 2: For each workload, get APIs, definitions, and examples
        foreach (var workload in workloads)
        {
            // API spec - should always succeed for listed workloads
            var apiContext = new CommandContext(_serviceProvider);
            var apiResult = await _getWorkloadApisCommand.ExecuteAsync(
                apiContext,
                _getWorkloadApisCommand.GetCommand().Parse(["--workload-type", workload]),
                cancellationToken);
            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);

            // Item definition - may or may not exist
            var defContext = new CommandContext(_serviceProvider);
            var defResult = await _getWorkloadDefinitionCommand.ExecuteAsync(
                defContext,
                _getWorkloadDefinitionCommand.GetCommand().Parse(["--workload-type", workload]),
                cancellationToken);
            Assert.True(
                defResult.Status == HttpStatusCode.OK || defResult.Status == HttpStatusCode.NotFound,
                $"Unexpected definition status {defResult.Status} for workload '{workload}'");

            // Examples - should always succeed (returns empty dict if none exist)
            var exContext = new CommandContext(_serviceProvider);
            var exResult = await _getExamplesCommand.ExecuteAsync(
                exContext,
                _getExamplesCommand.GetCommand().Parse(["--workload-type", workload]),
                cancellationToken);
            Assert.Equal(HttpStatusCode.OK, exResult.Status);
            Assert.NotNull(exResult.Results);
        }
    }

    [Fact]
    public async Task ListWorkloads_DoesNotReturnCommon()
    {
        // The service filters out the "common" pseudo-workload
        var cancellationToken = TestContext.Current.CancellationToken;

        var listContext = new CommandContext(_serviceProvider);
        var listResult = await _listWorkloadsCommand.ExecuteAsync(
            listContext, _listWorkloadsCommand.GetCommand().Parse([]), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, listResult.Status);

        var workloads = DeserializeWorkloads(listResult);
        Assert.DoesNotContain("common", workloads, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetApis_WithCommonWorkloadType_ReturnsNotFound()
    {
        // "common" is explicitly rejected with a helpful message
        var cancellationToken = TestContext.Current.CancellationToken;

        var apiContext = new CommandContext(_serviceProvider);
        var apiResult = await _getWorkloadApisCommand.ExecuteAsync(
            apiContext,
            _getWorkloadApisCommand.GetCommand().Parse(["--workload-type", "common"]),
            cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, apiResult.Status);
        Assert.Contains("common", apiResult.Message);
        Assert.Contains("platform", apiResult.Message);
    }

    [Fact]
    public async Task GetApis_WithNonexistentWorkload_ReturnsError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var apiContext = new CommandContext(_serviceProvider);
        var apiResult = await _getWorkloadApisCommand.ExecuteAsync(
            apiContext,
            _getWorkloadApisCommand.GetCommand().Parse(["--workload-type", "this-workload-does-not-exist"]),
            cancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, apiResult.Status);
    }

    [Fact]
    public async Task ListWorkloads_ThenGetApisForEach_ApiSpecContainsValidJson()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // Step 1: List workloads
        var listContext = new CommandContext(_serviceProvider);
        var listResult = await _listWorkloadsCommand.ExecuteAsync(
            listContext, _listWorkloadsCommand.GetCommand().Parse([]), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, listResult.Status);

        var workloads = DeserializeWorkloads(listResult);
        Assert.NotEmpty(workloads);

        // Step 2: For each workload, verify the API spec result contains parseable JSON content
        foreach (var workload in workloads)
        {
            var apiContext = new CommandContext(_serviceProvider);
            var apiResult = await _getWorkloadApisCommand.ExecuteAsync(
                apiContext,
                _getWorkloadApisCommand.GetCommand().Parse(["--workload-type", workload]),
                cancellationToken);

            Assert.Equal(HttpStatusCode.OK, apiResult.Status);
            Assert.NotNull(apiResult.Results);

            // Serialize the result to JSON and verify it contains an apiSpecification field
            var json = SerializeResults(apiResult);
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
        var cancellationToken = TestContext.Current.CancellationToken;

        var context1 = new CommandContext(_serviceProvider);
        var result1 = await _listWorkloadsCommand.ExecuteAsync(
            context1, _listWorkloadsCommand.GetCommand().Parse([]), cancellationToken);

        var context2 = new CommandContext(_serviceProvider);
        var result2 = await _listWorkloadsCommand.ExecuteAsync(
            context2, _listWorkloadsCommand.GetCommand().Parse([]), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, result1.Status);
        Assert.Equal(HttpStatusCode.OK, result2.Status);

        var workloads1 = DeserializeWorkloads(result1);
        var workloads2 = DeserializeWorkloads(result2);

        Assert.Equal(workloads1.OrderBy(w => w), workloads2.OrderBy(w => w));
    }

    private static string SerializeResults(CommandResponse response)
    {
        Assert.NotNull(response.Results);
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        response.Results.Write(writer);
        writer.Flush();
        return System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    private static IEnumerable<string> DeserializeWorkloads(CommandResponse response)
    {
        var json = SerializeResults(response);
        using var doc = JsonDocument.Parse(json);
        var workloadsArray = doc.RootElement.GetProperty("Workloads");
        return workloadsArray.EnumerateArray().Select(e => e.GetString()!).ToArray();
    }
}
