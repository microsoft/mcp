// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Models.HealthModels;

namespace Azure.Mcp.Tools.Monitor.Services;

/// <summary>
/// The calculations of an inspection over CloudHealth API payloads: which entities it selects, and which of their
/// fields it keeps. Payloads are never rearranged, renamed or merged. A projection only leaves fields out. No I/O.
/// </summary>
internal static class HealthModelInspection
{
    internal static readonly TimeSpan DefaultWindow = TimeSpan.FromHours(24);

    // A path segment that stands for any property at its level.
    private const string AnyProperty = "*";

    // A path segment that stands for any number of levels, none included.
    private const string AnyDepth = "**";

    internal static string? Name(JsonElement entity) => Text(entity, "name");

    internal static string? HealthState(JsonElement entity) => Text(Property(entity, "properties"), "healthState");

    internal static bool Admits(HealthModelHealthFilter? filter, string? state) => filter switch
    {
        null => true,
        HealthModelHealthFilter.NotHealthy => Admits(HealthModelHealthFilter.Degraded, state)
            || Admits(HealthModelHealthFilter.Unhealthy, state)
            || Admits(HealthModelHealthFilter.Unknown, state),
        _ => string.Equals(state, filter.ToString(), StringComparison.OrdinalIgnoreCase),
    };

    /// <summary>
    /// The window to send for history and annotations: it ends at <paramref name="endAt"/>, else <paramref name="timestamp"/>,
    /// and starts a day before its end unless given. With neither, the service's own default applies.
    /// For example, an endAt of 2026-11-01T12:00Z alone gives the window from 2026-10-31T12:00Z to then.
    /// </summary>
    internal static (DateTimeOffset? StartAt, DateTimeOffset? EndAt) Window(DateTimeOffset? startAt, DateTimeOffset? endAt, DateTimeOffset? timestamp)
    {
        var end = endAt ?? timestamp;
        return (startAt ?? end - DefaultWindow, end);
    }

    /// <summary>The page of <paramref name="items"/> after the first <paramref name="skip"/>, and where the next page starts while more remain.</summary>
    internal static (List<T> Page, int? NextSkip) Page<T>(IReadOnlyList<T> items, int skip, int max) =>
        ([.. items.Skip(skip).Take(max)], max < items.Count - skip ? skip + max : null);

    /// <summary>
    /// Walks the dependency graph breadth-first from <paramref name="start"/> and returns it and each entity reached,
    /// once, with the path it was first reached by. <see cref="HealthModelWalk.Causes"/> follows only children that drive
    /// their parent's state. A walk reaches each listed entity at most once, so it needs no cap of its own.
    /// </summary>
    internal static List<(string Path, JsonElement Entity)> Walk(
        IReadOnlyDictionary<string, JsonElement> entities,
        IEnumerable<JsonElement> relationships,
        string start,
        HealthModelWalk walk)
    {
        var next = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var properties in relationships.Select(edge => Property(edge, "properties")))
        {
            if (Text(properties, "parentEntityName") is { } parent && Text(properties, "childEntityName") is { } child)
            {
                var (from, to) = walk == HealthModelWalk.Dependents ? (child, parent) : (parent, child);
                (next.TryGetValue(from, out var list) ? list : next[from] = []).Add(to);
            }
        }

        var found = new List<(string Path, JsonElement Entity)> { (start, entities[start]) };
        var seen = new HashSet<string>([start], StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < found.Count; index++)
        {
            var (path, entity) = found[index];
            foreach (var name in next.GetValueOrDefault(Name(entity) ?? string.Empty) ?? [])
            {
                if (entities.TryGetValue(name, out var reached) && (walk != HealthModelWalk.Causes || Drives(entity, reached)) && seen.Add(name))
                {
                    found.Add(($"{path}/{name}", reached));
                }
            }
        }
        return found;
    }

    // A child that is not suppressed drives its parent when it is Degraded or Unhealthy. An Unknown child never beats a
    // known state under WorstOf (the default) or BestOf, and MinHealthy and MaxNotHealthy count it only when the
    // parent's dependencies set ignoreUnknown to false. Otherwise it drives only a parent that is Unknown itself.
    private static bool Drives(JsonElement parent, JsonElement child) =>
        Text(Property(child, "properties"), "impact") != "Suppressed"
        && HealthState(child) switch
        {
            "Degraded" or "Unhealthy" => true,
            "Unknown" => HealthState(parent) == "Unknown" || CountsUnknown(Property(Property(Property(parent, "properties"), "signalGroups"), "dependencies")),
            _ => false,
        };

    private static bool CountsUnknown(JsonElement dependencies) =>
        Text(dependencies, "aggregationType") is "MinHealthy" or "MaxNotHealthy"
        && Property(dependencies, "ignoreUnknown").ValueKind == JsonValueKind.False;

    /// <summary>The names of the entity's signals and of the signal definitions they name, across every signal group.</summary>
    internal static (List<string> Signals, List<string> Definitions) SignalNames(JsonElement entity)
    {
        var signals = new List<string>();
        var definitions = new List<string>();
        if (Property(Property(entity, "properties"), "signalGroups") is { ValueKind: JsonValueKind.Object } groups)
        {
            foreach (var group in groups.EnumerateObject())
            {
                // A group's Resource Health signal has a history of its own while it is enabled.
                if (Property(group.Value, "resourceHealth") is { ValueKind: JsonValueKind.Object } resourceHealth
                    && Text(resourceHealth, "enabled") is null or "Enabled"
                    && Text(resourceHealth, "signalName") is { } resourceHealthSignal)
                {
                    signals.Add(resourceHealthSignal);
                }
                // A group lists its signals under "signals", as in azureResource.signals.
                // externalSignals is the list itself.
                var held = Property(group.Value, "signals") is { ValueKind: JsonValueKind.Array } list ? list : group.Value;
                if (held.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }
                foreach (var signal in held.EnumerateArray())
                {
                    if (Text(signal, "name") is { } name)
                    {
                        signals.Add(name);
                    }
                    if (Text(signal, "signalDefinitionName") is { } definition)
                    {
                        definitions.Add(definition);
                    }
                }
            }
        }
        return (signals, definitions);
    }

    /// <summary>
    /// Keeps only the fields of <paramref name="source"/> named by dotted <paramref name="paths"/>, in place: names,
    /// nesting and array order stay as they are. A path keeps everything below it, and array elements are walked
    /// through, so <c>properties.signalGroups.azureResource.signals.status</c> keeps the status of every signal.
    /// </summary>
    internal static JsonElement Project(JsonElement source, IEnumerable<string> paths)
    {
        var keep = new Fields();
        foreach (var path in paths)
        {
            // ** stands for no level too, so a trailing ** keeps the value itself, whatever it holds.
            var segments = path.Split('.');
            var count = segments.Length;
            while (count > 0 && segments[count - 1] == AnyDepth)
            {
                count--;
            }
            if (count == 0)
            {
                return source;
            }

            var node = keep;
            for (var index = 0; index < count && node is not null; index++)
            {
                var last = index == count - 1;
                if (!node.TryGetValue(segments[index], out var child) || (last && child is not null))
                {
                    node[segments[index]] = child = last ? null : new Fields();
                }
                node = child;
            }
        }

        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            Write(writer, source, keep);
        }
        using var document = JsonDocument.Parse(buffer.WrittenMemory);
        return document.RootElement.Clone();
    }

    /// <summary>
    /// Every dotted path that occurs in <paramref name="source"/>, array elements walked through, in first-seen order.
    /// A property whose name holds a dot, such as a tag key, cannot be named in a path, so the list stops at its parent.
    /// </summary>
    internal static IEnumerable<string> Paths(JsonElement source, string prefix = "")
    {
        var paths = new List<string>();
        Collect(source, prefix, paths, new HashSet<string>(StringComparer.Ordinal));
        return paths;
    }

    private static void Collect(JsonElement element, string path, List<string> paths, HashSet<string> seen)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject().Where(property => !property.Name.Contains('.')))
            {
                var child = path.Length == 0 ? property.Name : $"{path}.{property.Name}";
                if (seen.Add(child))
                {
                    paths.Add(child);
                }
                Collect(property.Value, child, paths, seen);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                Collect(item, path, paths, seen);
            }
        }
    }

    private static void Write(Utf8JsonWriter writer, JsonElement element, Fields? keep)
    {
        if (keep is null)
        {
            element.WriteTo(writer);
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();
            foreach (var property in element.EnumerateObject())
            {
                if (Match(keep, property.Name, out var child) && Keeps(property.Value, child))
                {
                    writer.WritePropertyName(property.Name);
                    Write(writer, property.Value, child);
                }
            }
            writer.WriteEndObject();
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            writer.WriteStartArray();
            foreach (var item in element.EnumerateArray())
            {
                Write(writer, item, keep);
            }
            writer.WriteEndArray();
        }
        else
        {
            element.WriteTo(writer);
        }
    }

    // Whether a projection keeps any field of the value. A value that no path matches is left out.
    // For example, properties.signalGroups.*.signals.status drops a signal group that holds no signals.
    private static bool Keeps(JsonElement element, Fields? keep) =>
        keep is null || element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().Any(property => Match(keep, property.Name, out var child) && Keeps(property.Value, child)),
            JsonValueKind.Array => element.EnumerateArray().Any(item => Keeps(item, keep)),
            _ => false,
        };

    // The fields kept below a property are its named entry, the * entry and the ** entry together. An entry with no
    // children keeps everything. A ** entry stays in force below, and what follows it also applies at this level.
    // For example, properties.healthState with **.status keeps healthState and every status at any depth below
    // properties.
    private static bool Match(Fields keep, string name, out Fields? child)
    {
        var found = new List<Fields?>();
        if (keep.TryGetValue(name, out var byName))
        {
            found.Add(byName);
        }
        if (keep.TryGetValue(AnyProperty, out var byAny))
        {
            found.Add(byAny);
        }
        if (keep.TryGetValue(AnyDepth, out var rest))
        {
            found.Add(rest is null ? null : new Fields { [AnyDepth] = rest });
            if (rest is not null && Match(rest, name, out var here))
            {
                found.Add(here);
            }
        }
        child = found.Count == 0 ? null : found.Aggregate(Union);
        return found.Count > 0;
    }

    private static Fields? Union(Fields? first, Fields? second)
    {
        if (first is null || second is null)
        {
            return null;
        }
        var union = new Fields();
        foreach (var key in first.Keys.Union(second.Keys))
        {
            var inFirst = first.TryGetValue(key, out var fromFirst);
            var inSecond = second.TryGetValue(key, out var fromSecond);
            union[key] = !inFirst ? fromSecond : !inSecond ? fromFirst : Union(fromFirst, fromSecond);
        }
        return union;
    }

    private static JsonElement Property(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) ? value : default;

    private static string? Text(JsonElement element, string name) =>
        Property(element, name) is { ValueKind: JsonValueKind.String } value ? value.GetString() : null;

    // The fields a projection keeps below one object: a name with no children keeps that whole value.
    private sealed class Fields() : Dictionary<string, Fields?>(StringComparer.Ordinal);
}
