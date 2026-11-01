// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Models.HealthModels;
using Azure.Mcp.Tools.Monitor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.HealthModels;

public class HealthModelInspectionTests
{
    [Theory]
    [InlineData(null, "Healthy", true)]
    [InlineData(HealthModelHealthFilter.Healthy, "Healthy", true)]
    [InlineData(HealthModelHealthFilter.Healthy, "healthy", true)]
    [InlineData(HealthModelHealthFilter.Degraded, "Unhealthy", false)]
    [InlineData(HealthModelHealthFilter.NotHealthy, "Unknown", true)]
    [InlineData(HealthModelHealthFilter.NotHealthy, "Healthy", false)]
    [InlineData(HealthModelHealthFilter.NotHealthy, "Deleted", false)]
    [InlineData(HealthModelHealthFilter.Unknown, null, false)]
    public void Admits_KeepsEntitiesInTheFilteredState(HealthModelHealthFilter? filter, string? state, bool expected) =>
        Assert.Equal(expected, HealthModelInspection.Admits(filter, state));

    [Theory]
    // No bounds: the service's own default window.
    [InlineData(null, null, null, null, null)]
    // An end alone starts a day before it, not a day before now.
    [InlineData(null, "2026-10-20T12:00:00Z", null, "2026-10-19T12:00:00Z", "2026-10-20T12:00:00Z")]
    // At a timestamp, the window ends there.
    [InlineData(null, null, "2026-10-20T12:00:00Z", "2026-10-19T12:00:00Z", "2026-10-20T12:00:00Z")]
    [InlineData("2026-10-01T00:00:00Z", null, null, "2026-10-01T00:00:00Z", null)]
    [InlineData("2026-10-01T00:00:00Z", "2026-10-02T00:00:00Z", "2026-10-20T12:00:00Z", "2026-10-01T00:00:00Z", "2026-10-02T00:00:00Z")]
    public void Window_EndsAtEndAtOrTimestamp_AndStartsADayEarlier(string? startAt, string? endAt, string? timestamp, string? start, string? end) =>
        Assert.Equal((Time(start), Time(end)), HealthModelInspection.Window(Time(startAt), Time(endAt), Time(timestamp)));

    [Theory]
    [InlineData(120, 0, "0..49", 50)]
    [InlineData(120, 50, "50..99", 100)]
    [InlineData(120, 100, "100..119", null)]
    [InlineData(120, 150, "", null)]
    [InlineData(50, 0, "0..49", null)]
    // A marker past the end overflows nothing.
    [InlineData(3, int.MaxValue, "", null)]
    public void Page_ReturnsTheItemsAfterSkip_AndWhereTheNextPageStarts(int count, int skip, string expected, int? nextSkip)
    {
        var (page, next) = HealthModelInspection.Page(Enumerable.Range(0, count).ToList(), skip, max: 50);

        Assert.Equal((expected, nextSkip), (page.Count == 0 ? string.Empty : $"{page[0]}..{page[^1]}", next));
    }

    [Theory]
    [InlineData(HealthModelWalk.Dependencies, "root", "root;root/a;root/b;root/a/c")]
    [InlineData(HealthModelWalk.Dependents, "c", "c;c/a;c/b;c/a/root")]
    // Causes skips the healthy child b, the suppressed child d and the Unknown child e, which the rollup leaves out.
    [InlineData(HealthModelWalk.Causes, "root", "root;root/a;root/a/c")]
    public void Walk_ReturnsTheStartAndEachEntityOnceWithItsPath(HealthModelWalk walk, string start, string expected)
    {
        var entities = new[] { Entity("root", "Degraded"), Entity("a", "Degraded"), Entity("b", "Healthy"), Entity("c", "Unhealthy"), Entity("d", "Degraded", "Suppressed"), Entity("e", "Unknown") }
            .ToDictionary(entity => HealthModelInspection.Name(entity)!, StringComparer.OrdinalIgnoreCase);
        var relationships = new[] { ("root", "a"), ("root", "b"), ("a", "c"), ("b", "c"), ("c", "root"), ("root", "d"), ("root", "e") }
            .Select(Relationship);
        if (walk != HealthModelWalk.Causes)
        {
            entities.Remove("d");
            entities.Remove("e");
        }

        var found = HealthModelInspection.Walk(entities, relationships, start, walk);

        Assert.Equal(expected, string.Join(';', found.Select(step => step.Path)));
    }

    [Theory]
    // An Unknown child drives an Unknown parent, or a MinHealthy or MaxNotHealthy rollup that keeps Unknown children.
    [InlineData("Unknown", null, null, true)]
    [InlineData("Degraded", null, null, false)]
    [InlineData("Degraded", "MinHealthy", true, false)]
    [InlineData("Degraded", "MinHealthy", false, true)]
    [InlineData("Degraded", "MaxNotHealthy", false, true)]
    [InlineData("Degraded", "WorstOf", false, false)]
    [InlineData("Degraded", null, false, false)]
    public void Walk_Causes_FollowsUnknownChildrenOnlyWhereTheRollupCountsThem(string parentState, string? aggregationType, bool? ignoreUnknown, bool followed)
    {
        var settings = new[]
        {
            aggregationType is null ? null : "\"aggregationType\":\"" + aggregationType + "\"",
            ignoreUnknown is { } ignore ? "\"ignoreUnknown\":" + (ignore ? "true" : "false") : null,
        }.OfType<string>().ToList();
        var rollup = settings.Count > 0 ? ",\"signalGroups\":{\"dependencies\":{" + string.Join(',', settings) + "}}" : string.Empty;
        var entities = new[] { Json("{\"name\":\"root\",\"properties\":{\"healthState\":\"" + parentState + "\"" + rollup + "}}"), Entity("child", "Unknown") }
            .ToDictionary(entity => HealthModelInspection.Name(entity)!, StringComparer.OrdinalIgnoreCase);

        var found = HealthModelInspection.Walk(entities, [Relationship(("root", "child"))], "root", HealthModelWalk.Causes);

        Assert.Equal(followed ? "root;root/child" : "root", string.Join(';', found.Select(step => step.Path)));
    }

    [Theory]
    // An enabled Resource Health signal is read like the group's other signals; a disabled one is not.
    [InlineData("""{"signalName":"RH"}""", "RH;cpu;lat;push")]
    [InlineData("""{"signalName":"RH","enabled":"Enabled"}""", "RH;cpu;lat;push")]
    [InlineData("""{"signalName":"RH","enabled":"Disabled"}""", "cpu;lat;push")]
    public void SignalNames_ReadsEveryGroupAndTheDefinitionsTheSignalsName(string resourceHealth, string expected)
    {
        var entity = Json("""{"name":"api","properties":{"signalGroups":{"azureResource":{"signals":[{"name":"cpu"},{"name":"lat","signalDefinitionName":"lat-def"}],"resourceHealth":"""
            + resourceHealth + """},"externalSignals":[{"name":"push"}],"dependencies":{"aggregationType":"WorstOf"}}}}""");

        var (signals, definitions) = HealthModelInspection.SignalNames(entity);

        Assert.Equal(expected, string.Join(';', signals));
        Assert.Equal(["lat-def"], definitions);
    }

    [Theory]
    // A path keeps its value in place, nested as it was.
    [InlineData("properties.healthState", """{"properties":{"healthState":"Degraded"}}""")]
    // Array elements are walked through and keep their order.
    [InlineData("properties.signalGroups.azureResource.signals.name", """{"properties":{"signalGroups":{"azureResource":{"signals":[{"name":"cpu"},{"name":"mem"}]}}}}""")]
    // A path keeps everything below it, and wins over a deeper path under it.
    [InlineData("properties.signalGroups.azureResource.signals.status;properties.signalGroups", """{"properties":{"signalGroups":{"azureResource":{"signals":[{"name":"cpu","status":{"value":1}},{"name":"mem"}]}}}}""")]
    // A path that matches nothing leaves nothing behind.
    [InlineData("name;properties.missing", """{"name":"api"}""")]
    // * stands for any property at its level.
    [InlineData("properties.signalGroups.*.signals.name", """{"properties":{"signalGroups":{"azureResource":{"signals":[{"name":"cpu"},{"name":"mem"}]}}}}""")]
    // A named path and a * path that meet keep what either names.
    [InlineData("properties.signalGroups.*.signals.name;properties.signalGroups.azureResource.signals.status", """{"properties":{"signalGroups":{"azureResource":{"signals":[{"name":"cpu","status":{"value":1}},{"name":"mem"}]}}}}""")]
    // A ** segment stands for any number of levels.
    [InlineData("properties.**.value", """{"properties":{"signalGroups":{"azureResource":{"signals":[{"status":{"value":1}},{}]}}}}""")]
    // A ** at the end keeps everything below.
    [InlineData("properties.signalGroups.**", """{"properties":{"signalGroups":{"azureResource":{"signals":[{"name":"cpu","status":{"value":1}},{"name":"mem"}]}}}}""")]
    // A list none of whose elements match leaves nothing behind either.
    [InlineData("name;**.missing", """{"name":"api"}""")]
    // A trailing ** stands for no level too, so it keeps a plain value.
    [InlineData("properties.healthState.**", """{"properties":{"healthState":"Degraded"}}""")]
    // ** alone keeps everything.
    [InlineData("**", """{"name":"api","properties":{"healthState":"Degraded","signalGroups":{"azureResource":{"signals":[{"name":"cpu","status":{"value":1}},{"name":"mem"}]}}}}""")]
    public void Project_KeepsOnlyTheNamedPathsInPlace(string paths, string expected)
    {
        var entity = Json("""{"name":"api","properties":{"healthState":"Degraded","signalGroups":{"azureResource":{"signals":[{"name":"cpu","status":{"value":1}},{"name":"mem"}]}}}}""");

        var projected = HealthModelInspection.Project(entity, paths.Split(';'));

        Assert.Equal(expected, projected.GetRawText());
    }

    [Fact]
    public void Paths_ListsEveryFieldPathOnce()
    {
        // A tag key with a dot cannot be named in a path, so the list stops at tags.
        var entity = Json("""{"name":"api","properties":{"signals":[{"name":"cpu","status":{"value":1}},{"name":"mem"}],"tags":{"app.tier":"web"}}}""");

        Assert.Equal(
            ["entity.name", "entity.properties", "entity.properties.signals", "entity.properties.signals.name", "entity.properties.signals.status", "entity.properties.signals.status.value", "entity.properties.tags"],
            HealthModelInspection.Paths(entity, "entity"));
    }

    private static JsonElement Entity(string name, string state, string impact = "Standard") =>
        Json($$$"""{"name":"{{{name}}}","properties":{"healthState":"{{{state}}}","impact":"{{{impact}}}"}}""");

    private static JsonElement Relationship((string Parent, string Child) edge) =>
        Json($$$"""{"name":"r","properties":{"parentEntityName":"{{{edge.Parent}}}","childEntityName":"{{{edge.Child}}}"}}""");

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static DateTimeOffset? Time(string? text) => text is null ? null : DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);
}
