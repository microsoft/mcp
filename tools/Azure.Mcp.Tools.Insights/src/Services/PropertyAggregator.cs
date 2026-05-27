// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tools.Insights.Services.Models;

namespace Azure.Mcp.Tools.Insights.Services;

/// <summary>
/// Aggregates the top-N most-common observed values for each property leaf across a
/// collection of Azure Resource Graph rows.
/// </summary>
internal static class PropertyAggregator
{
    internal const int MaxPropertyDepth = 5;

    internal const int TopValuesPerLeaf = 3;

    public static SubscriptionAggregation Aggregate(IEnumerable<JsonElement> rows, int subscriptionCount = 1)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var byType = new Dictionary<string, ResourceState>(StringComparer.Ordinal);
        var resourceGroups = new HashSet<string>(StringComparer.Ordinal);

        foreach (var row in rows)
        {
            if (row.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var typeStr = TryGetString(row, "type");
            if (string.IsNullOrEmpty(typeStr))
            {
                continue;
            }

            var typeKey = typeStr.ToLowerInvariant();

            var rg = TryGetString(row, "resourceGroup");
            if (!string.IsNullOrEmpty(rg))
            {
                resourceGroups.Add(rg.ToLowerInvariant());
            }

            if (!byType.TryGetValue(typeKey, out var state))
            {
                state = new ResourceState();
                byType[typeKey] = state;
            }

            state.Count++;

            foreach (var (path, value) in WalkRow(row))
            {
                Insert(state.Tree, path, value);
            }
        }

        var result = new Dictionary<string, ResourceTypeAggregation>(StringComparer.Ordinal);
        foreach (var (typeKey, state) in byType)
        {
            result[typeKey] = new ResourceTypeAggregation(
                typeKey,
                state.Count,
                Emit(state.Tree));
        }

        return new SubscriptionAggregation(result, subscriptionCount, resourceGroups.Count);
    }

    private static IEnumerable<(IReadOnlyList<string> Path, string Value)> WalkRow(JsonElement row)
    {
        var location = TryGetString(row, "location");
        if (!string.IsNullOrEmpty(location))
        {
            yield return (new[] { "location" }, location.ToLowerInvariant());
        }

        var kind = TryGetString(row, "kind");
        if (!string.IsNullOrEmpty(kind))
        {
            yield return (new[] { "kind" }, kind.ToLowerInvariant());
        }

        // name: emit raw so the naming convention can be inferred downstream.
        var name = TryGetString(row, "name");
        if (!string.IsNullOrEmpty(name))
        {
            yield return (new[] { "name" }, name);
        }

        if (row.TryGetProperty("tags", out var tags) && tags.ValueKind == JsonValueKind.Object)
        {
            foreach (var pair in WalkObject(tags, new List<string> { "tags" }, depth: 1))
            {
                yield return pair;
            }
        }

        if (row.TryGetProperty("sku", out var sku) && sku.ValueKind == JsonValueKind.Object)
        {
            foreach (var pair in WalkObject(sku, new List<string> { "sku" }, depth: 1))
            {
                yield return pair;
            }
        }

        if (row.TryGetProperty("identity", out var identity) && identity.ValueKind == JsonValueKind.Object)
        {
            var idType = TryGetString(identity, "type");
            if (!string.IsNullOrEmpty(idType))
            {
                yield return (new[] { "identity" }, idType.ToLowerInvariant().Replace(" ", ""));
            }
        }

        if (row.TryGetProperty("properties", out var props) && props.ValueKind == JsonValueKind.Object)
        {
            foreach (var pair in WalkObject(props, new List<string> { "properties" }, depth: 1))
            {
                yield return pair;
            }
        }
    }

    private static IEnumerable<(IReadOnlyList<string> Path, string Value)> WalkObject(
        JsonElement node,
        List<string> path,
        int depth)
    {
        if (depth > MaxPropertyDepth)
        {
            yield break;
        }

        foreach (var prop in node.EnumerateObject())
        {
            var newPath = new List<string>(path) { prop.Name.ToLowerInvariant() };
            foreach (var pair in WalkValue(prop.Value, newPath, depth + 1))
            {
                yield return pair;
            }
        }
    }

    private static IEnumerable<(IReadOnlyList<string> Path, string Value)> WalkValue(
        JsonElement value,
        List<string> path,
        int depth)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var pair in WalkObject(value, path, depth))
                {
                    yield return pair;
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in value.EnumerateArray())
                {
                    foreach (var pair in WalkValue(item, path, depth + 1))
                    {
                        yield return pair;
                    }
                }
                break;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                yield break;
            default:
                var scalar = ScalarToString(value).Trim();
                if (!string.IsNullOrEmpty(scalar))
                {
                    yield return (path, scalar);
                }
                break;
        }
    }

    private static void Insert(JsonObject tree, IReadOnlyList<string> path, string value)
    {
        if (path.Count == 0)
        {
            return;
        }

        var cursor = tree;
        for (int i = 0; i < path.Count - 1; i++)
        {
            var key = path[i];
            if (!cursor.TryGetPropertyValue(key, out var existing) || existing is null)
            {
                var next = new JsonObject();
                cursor[key] = next;
                cursor = next;
            }
            else if (existing is JsonObject obj)
            {
                if (IsCounter(obj))
                {
                    return;
                }
                cursor = obj;
            }
            else
            {
                return;
            }
        }

        var leafName = path[^1];
        if (!cursor.TryGetPropertyValue(leafName, out var leaf) || leaf is null)
        {
            cursor[leafName] = new JsonObject { [value] = JsonValue.Create(1) };
            return;
        }

        if (leaf is JsonObject leafCounter)
        {
            if (!IsCounter(leafCounter))
            {
                return;
            }

            if (leafCounter.TryGetPropertyValue(value, out var cur) && cur is JsonValue jv && jv.TryGetValue<int>(out var n))
            {
                leafCounter[value] = JsonValue.Create(n + 1);
            }
            else
            {
                leafCounter[value] = JsonValue.Create(1);
            }
        }
    }

    // Distinguishes a leaf counter ({value:int, ...}) from an intermediate node.
    private static bool IsCounter(JsonObject obj)
    {
        if (obj.Count == 0)
        {
            return false;
        }
        foreach (var kvp in obj)
        {
            if (kvp.Value is not JsonValue v || !v.TryGetValue<int>(out _))
            {
                return false;
            }
        }
        return true;
    }

    private static JsonObject Emit(JsonObject tree)
    {
        var result = new JsonObject();
        foreach (var kvp in tree)
        {
            if (kvp.Value is JsonObject child)
            {
                if (IsCounter(child))
                {
                    var top = TopNFractions(child, TopValuesPerLeaf);
                    if (top.Count > 0)
                    {
                        result[kvp.Key] = top;
                    }
                }
                else
                {
                    var sub = Emit(child);
                    if (sub.Count > 0)
                    {
                        result[kvp.Key] = sub;
                    }
                }
            }
        }
        return result;
    }

    private static JsonObject TopNFractions(JsonObject counter, int n)
    {
        long total = 0;
        foreach (var kvp in counter)
        {
            if (kvp.Value is JsonValue jv && jv.TryGetValue<int>(out var c))
            {
                total += c;
            }
        }

        var result = new JsonObject();
        if (total <= 0)
        {
            return result;
        }

        var ordered = counter
            .Where(kvp => kvp.Value is JsonValue jv && jv.TryGetValue<int>(out _))
            .Select(kvp => (Key: kvp.Key, Count: kvp.Value!.GetValue<int>()))
            .OrderByDescending(t => t.Count)
            .ThenBy(t => t.Key, StringComparer.Ordinal)
            .Take(n);

        foreach (var (key, count) in ordered)
        {
            var fraction = Math.Round((double)count / total, 3, MidpointRounding.ToEven);
            result[key] = JsonValue.Create(fraction);
        }
        return result;
    }

    private static string ScalarToString(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.True => "True",
        JsonValueKind.False => "False",
        JsonValueKind.Number => value.GetRawText(),
        _ => value.GetRawText(),
    };

    private static string? TryGetString(JsonElement obj, string name)
    {
        if (!obj.TryGetProperty(name, out var prop))
        {
            return null;
        }
        return prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;
    }

    private sealed record ResourceState
    {
        public int Count { get; set; }
        public JsonObject Tree { get; } = new JsonObject();
    }
}
