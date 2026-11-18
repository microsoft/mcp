// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.ResourceManager.CloudHealth.Models;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.HealthModels;

// The fixtures are in test-resources.healthmodels.module.bicep and their health states in test-resources-post.ps1.
// Model B's root has an enabled Resource Health signal and reported Degraded, then Unhealthy, states. Model C is a root
// with a WorstOf rollup over two leaves: frontend reported Degraded and backend reported Healthy.
public sealed class HealthModelInspectRecordedTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private static readonly string[] s_validHealthStates =
    [
        EntityHealthState.Healthy.ToString(),
        EntityHealthState.Degraded.ToString(),
        EntityHealthState.Unhealthy.ToString(),
        EntityHealthState.Unknown.ToString(),
    ];

    private static readonly string[] s_sanitizedHeaders =
    [
        "x-ms-correlation-request-id",
        "x-ms-operation-identifier",
        "x-ms-routing-request-id",
        "x-ms-served-by",
        "X-MSEdge-Ref"
    ];

    private string? _modelB;
    private string? _modelC;

    // The default AZSDK3493 replaces every $..name with "Sanitized". Entity names stay base-name-sanitized instead,
    // because the inspect tests read entity names and walk paths built from them.
    public override List<string> DisabledDefaultSanitizers =>
        [.. base.DisabledDefaultSanitizers, "AZSDK3493"];

    private List<GeneralRegexSanitizer>? _generalRegexSanitizers;

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers =>
        _generalRegexSanitizers ??=
        [
            .. base.GeneralRegexSanitizers,
            new(new GeneralRegexSanitizerBody
            {
                Regex = @"ResourceHealth-[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}",
                Value = "ResourceHealth-Sanitized"
            }),
            new(new GeneralRegexSanitizerBody
            {
                Regex = Regex.Escape(Settings.ResourceGroupName),
                Value = "Sanitized"
            })
        ];

    public override List<UriRegexSanitizer> UriRegexSanitizers { get; } =
    [
        new(new UriRegexSanitizerBody
        {
            Regex = "resource[Gg]roups/([^?\\/]+)",
            Value = "Sanitized",
            GroupForReplace = "1"
        })
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..resourceGroup")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..displayName")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..Owners")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..id")
        {
            Regex = "resource[Gg]roups/([^?\\/]+)",
            Value = "Sanitized",
            GroupForReplace = "1"
        })
    ];

    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
    [
        .. base.HeaderRegexSanitizers,
        .. s_sanitizedHeaders.Select(h => new HeaderRegexSanitizer(new HeaderRegexSanitizerBody(h)))
    ];

    // History requests carry time windows in their bodies.
    public override CustomDefaultMatcher? TestMatcher => new()
    {
        CompareBodies = false
    };

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _modelB = $"{Settings.ResourceBaseName}-hm-b";
        _modelC = $"{Settings.ResourceBaseName}-hm-c";
    }

    [Fact]
    public async Task Should_Inspect_HealthModel_WalksDependencies()
    {
        var model = _modelC!;

        var result = await InspectAsync(model, ("walk", "Dependencies"));

        var entities = result.AssertProperty("entities").EnumerateArray().ToList();
        Assert.Equal([model, $"{model}/{model}-leaf-backend", $"{model}/{model}-leaf-frontend"], Paths(entities).Order(StringComparer.Ordinal));
        Assert.All(entities, entity => Assert.Contains(HealthState(entity), s_validHealthStates));
        Assert.False(result.TryGetProperty("truncated", out _));
        Assert.Equal(3, result.AssertProperty("total").GetInt32());
        Assert.EndsWith($"/providers/Microsoft.CloudHealth/healthmodels/{model}", result.AssertProperty("id").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.True(result.AssertProperty("readAt").TryGetDateTimeOffset(out _));
    }

    // The root's WorstOf rollup takes its Degraded state from the frontend, so the walk does not follow the healthy backend.
    [Fact]
    public async Task Should_Inspect_HealthModel_WalksCauses()
    {
        var model = _modelC!;

        var result = await InspectAsync(model, ("walk", "Causes"));

        var entities = result.AssertProperty("entities").EnumerateArray().ToList();
        Assert.Equal([model, $"{model}/{model}-leaf-frontend"], Paths(entities));
        Assert.All(entities, entity => Assert.Equal("Degraded", HealthState(entity)));
    }

    [Fact]
    public async Task Should_Inspect_HealthModel_WalksDependents()
    {
        var model = _modelC!;

        var result = await InspectAsync(model, ("entity", $"{model}-leaf-backend"), ("walk", "Dependents"));

        Assert.Equal([$"{model}-leaf-backend", $"{model}-leaf-backend/{model}"], Paths(result.AssertProperty("entities").EnumerateArray()));
    }

    // NotHealthy keeps the Degraded root and frontend, one page at a time.
    [Fact]
    public async Task Should_Inspect_HealthModel_FiltersByHealthAndPages()
    {
        var model = _modelC!;

        var first = await InspectAsync(model, ("where-health", "NotHealthy"), ("top", 1));
        var second = await InspectAsync(model, ("where-health", "NotHealthy"), ("top", 1), ("next-marker", first.AssertProperty("nextMarker").GetString()!));

        Assert.Equal(2, first.AssertProperty("total").GetInt32());
        Assert.False(second.TryGetProperty("nextMarker", out _));
        var entities = first.AssertProperty("entities").EnumerateArray().Concat(second.AssertProperty("entities").EnumerateArray()).ToList();
        Assert.Equal([model, $"{model}-leaf-frontend"], entities.Select(entity => entity.AssertProperty("entity").AssertProperty("name").GetString()!).Order(StringComparer.Ordinal));
    }

    // * as the first segment reaches into every payload, and the result lists every path the payloads hold.
    [Fact]
    public async Task Should_Inspect_HealthModel_KeepsOnlyTheChosenFields()
    {
        var model = _modelC!;

        var result = await InspectAsync(model, ("fields", "*.properties.healthState"));

        var entities = result.AssertProperty("entities").EnumerateArray().ToList();
        Assert.Equal(3, entities.Count);
        Assert.All(entities, entity =>
        {
            var properties = Assert.Single(entity.AssertProperty("entity").EnumerateObject());
            Assert.Equal("properties", properties.Name);
            Assert.Equal("healthState", Assert.Single(properties.Value.EnumerateObject()).Name);
        });
        var fields = result.AssertProperty("fields").EnumerateArray().Select(field => field.GetString()).ToList();
        Assert.Contains("entity.name", fields);
        Assert.Contains("entity.properties.healthState", fields);
    }

    // The topology exists well before the tests run, so the model a minute ago has the same walk.
    [Fact]
    public async Task Should_Inspect_HealthModel_AtATimestamp()
    {
        var model = _modelC!;
        var timestamp = RegisterOrRetrieveVariable("timestamp", DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O", CultureInfo.InvariantCulture));

        var result = await InspectAsync(model, ("walk", "Dependencies"), ("timestamp", timestamp));

        var entities = result.AssertProperty("entities").EnumerateArray().ToList();
        Assert.Equal([model, $"{model}/{model}-leaf-backend", $"{model}/{model}-leaf-frontend"], Paths(entities).Order(StringComparer.Ordinal));
        Assert.All(entities, entity => Assert.Contains(HealthState(entity), s_validHealthStates));
    }

    // Model B's root has seeded health state transitions. They age out of the default 24 h window, so the history window
    // covers the last 29 days (the service reach is 30).
    [Fact]
    public async Task Should_Inspect_HealthModel_EntityWithSignalsAndHistory()
    {
        var model = _modelB!;
        var historyFrom = RegisterOrRetrieveVariable("historyFrom", DateTime.UtcNow.AddDays(-29).ToString("o"));

        var result = await CallToolAsync(
            "monitor_healthmodels_inspect",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "health-model", model },
                { "entity", model },
                { "include", new[] { "Signals", "History", "Annotations" } },
                { "start-at", historyFrom }
            });

        // Each payload is the CloudHealth API object, passed through.
        var item = Assert.Single(result.AssertProperty("entities").EnumerateArray());
        Assert.False(item.TryGetProperty("errors", out var errors), errors.ToString());
        var entity = item.AssertProperty("entity");
        Assert.Equal(model, entity.AssertProperty("name").GetString());
        var properties = entity.AssertProperty("properties");
        Assert.Contains(properties.AssertProperty("healthState").GetString(), s_validHealthStates);
        Assert.Equal(JsonValueKind.Object, properties.AssertProperty("signalGroups").ValueKind);

        var history = item.AssertProperty("history").AssertProperty("history").EnumerateArray().ToList();
        Assert.NotEmpty(history);
        Assert.All(history, transition => Assert.Contains(transition.AssertProperty("newState").GetString(), s_validHealthStates));
        Assert.Equal(JsonValueKind.Object, item.AssertProperty("annotations").ValueKind);
    }

    // Model B's root has an enabled Resource Health signal, which reports every few minutes, so its history in the
    // default 24 h window is not empty.
    [Fact]
    public async Task Should_Inspect_HealthModel_EntitySignalHistory()
    {
        var model = _modelB!;

        var result = await CallToolAsync(
            "monitor_healthmodels_inspect",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "health-model", model },
                { "entity", model },
                { "include", new[] { "SignalHistory" } }
            });

        // One getSignalHistory response per signal of the entity, each passed through.
        var item = Assert.Single(result.AssertProperty("entities").EnumerateArray());
        Assert.False(item.TryGetProperty("errors", out var errors), errors.ToString());
        var reads = item.AssertProperty("signalHistory").EnumerateArray().ToList();
        Assert.NotEmpty(reads);
        Assert.All(reads, read => Assert.False(string.IsNullOrEmpty(read.AssertProperty("signalName").GetString())));
        Assert.Contains(reads, read => read.AssertProperty("history").GetArrayLength() > 0);
    }

    private async Task<JsonElement> InspectAsync(string model, params (string Name, object Value)[] options)
    {
        var arguments = new Dictionary<string, object?>
        {
            { "subscription", Settings.SubscriptionId },
            { "resource-group", Settings.ResourceGroupName },
            { "health-model", model },
        };
        foreach (var (name, value) in options)
        {
            arguments[name] = value;
        }
        return (await CallToolAsync("monitor_healthmodels_inspect", arguments))!.Value;
    }

    private static IEnumerable<string> Paths(IEnumerable<JsonElement> entities) =>
        entities.Select(entity => entity.AssertProperty("path").GetString()!);

    private static string? HealthState(JsonElement entity) =>
        entity.AssertProperty("entity").AssertProperty("properties").AssertProperty("healthState").GetString();
}
