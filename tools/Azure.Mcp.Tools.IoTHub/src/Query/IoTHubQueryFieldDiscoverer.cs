// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Query;

public static class IoTHubQueryFieldDiscoverer
{
    private const int MaxExamplesPerField = 3;

    public static QueryDiscoveredFields Discover(IEnumerable<JsonElement> items)
    {
        var deviceFields = new FieldAccumulator();
        var tagFields = new FieldAccumulator();
        var desiredFields = new FieldAccumulator();
        var reportedFields = new FieldAccumulator();

        foreach (var item in items)
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            AddTopLevelDeviceFields(item, deviceFields);

            if (item.TryGetProperty("tags", out var tags))
            {
                AddFields(tags, string.Empty, tagFields);
            }

            if (item.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object)
            {
                if (properties.TryGetProperty("desired", out var desired))
                {
                    AddFields(desired, string.Empty, desiredFields);
                }

                if (properties.TryGetProperty("reported", out var reported))
                {
                    AddFields(reported, string.Empty, reportedFields);
                }
            }
        }

        return new QueryDiscoveredFields
        {
            Device = deviceFields.ToFields(),
            Tags = tagFields.ToFields(),
            Desired = desiredFields.ToFields(),
            Reported = reportedFields.ToFields()
        };
    }

    private static void AddTopLevelDeviceFields(JsonElement item, FieldAccumulator accumulator)
    {
        foreach (var property in item.EnumerateObject())
        {
            if (string.Equals(property.Name, "tags", StringComparison.OrdinalIgnoreCase)
                || string.Equals(property.Name, "properties", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            AddFields(property.Value, property.Name, accumulator);
        }
    }

    private static void AddFields(JsonElement element, string path, FieldAccumulator accumulator)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Name.StartsWith('$'))
                    {
                        continue;
                    }

                    var childPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";
                    AddFields(property.Value, childPath, accumulator);
                }
                break;
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                accumulator.Add(path, GetTypeName(element), element);
                break;
        }
    }

    private static string GetTypeName(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => "string",
        JsonValueKind.Number => "number",
        JsonValueKind.True or JsonValueKind.False => "boolean",
        _ => element.ValueKind.ToString().ToLowerInvariant()
    };

    private sealed class FieldAccumulator
    {
        private readonly SortedDictionary<string, FieldState> _fields = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string field, string type, JsonElement example)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                return;
            }

            if (!_fields.TryGetValue(field, out var state))
            {
                state = new FieldState(field);
                _fields.Add(field, state);
            }

            state.Add(type, example);
        }

        public List<QueryDiscoveredField> ToFields() => _fields.Values
            .Select(state => new QueryDiscoveredField(state.Field, state.Type, state.Examples))
            .ToList();
    }

    private sealed class FieldState(string field)
    {
        private readonly SortedSet<string> _types = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _exampleKeys = new(StringComparer.Ordinal);

        public string Field { get; } = field;
        public List<JsonElement> Examples { get; } = [];
        public string Type => _types.Count == 1 ? _types.First() : "mixed";

        public void Add(string type, JsonElement example)
        {
            _types.Add(type);

            var exampleKey = example.GetRawText();
            if (Examples.Count >= MaxExamplesPerField || !_exampleKeys.Add(exampleKey))
            {
                return;
            }

            Examples.Add(example.Clone());
        }
    }
}
