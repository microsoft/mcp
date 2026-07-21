// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Query;

/// <summary>
/// Compiles a structured, typed <see cref="QueryCompileRequest"/> into a syntactically valid IoT Hub query string.
/// This provides a predicate layer between natural-language intent and raw IoT Hub SQL, removing ambiguity between
/// tags, desired properties, reported properties, and top-level device fields while rejecting malformed input.
/// </summary>
public static partial class IoTHubQueryCompiler
{
    // IoT Hub query sources that are supported by the read/search query surface.
    private static readonly HashSet<string> s_allowedSources = new(StringComparer.OrdinalIgnoreCase)
    {
        "devices",
        "devices.modules",
        "devices.jobs"
    };

    /// <summary>
    /// Compiles the supplied request into an IoT Hub query string.
    /// </summary>
    /// <param name="request">The typed predicate plan to compile.</param>
    /// <returns>A valid IoT Hub query string, e.g. <c>SELECT * FROM devices WHERE properties.reported.temperature > 80</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when the request is malformed or contains unsupported values.</exception>
    public static string Compile(QueryCompileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var source = string.IsNullOrWhiteSpace(request.From) ? "devices" : request.From.Trim();
        if (!s_allowedSources.Contains(source))
        {
            throw new ArgumentException(
                $"Unsupported query source '{source}'. Supported sources are: {string.Join(", ", s_allowedSources)}.");
        }

        var logicalOperator = (request.LogicalOperator ?? "AND").Trim();
        if (!string.Equals(logicalOperator, "AND", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(logicalOperator, "OR", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Unsupported logical operator '{request.LogicalOperator}'. Supported values are 'AND' and 'OR'.");
        }

        if (request.Top is { } top && top <= 0)
        {
            throw new ArgumentException($"The 'top' value '{top}' must be a positive integer.");
        }

        var builder = new StringBuilder("SELECT * FROM ").Append(source);

        if (request.Filters is { Count: > 0 })
        {
            builder.Append(" WHERE ");
            var separator = $" {logicalOperator.ToUpperInvariant()} ";
            for (var i = 0; i < request.Filters.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(separator);
                }

                builder.Append(CompilePredicate(request.Filters[i], i, request.DiscoveredFields));
            }
        }

        return builder.ToString();
    }

    private static string CompilePredicate(QueryPredicate predicate, int index, QueryDiscoveredFields? discoveredFields)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (string.IsNullOrWhiteSpace(predicate.Field))
        {
            throw new ArgumentException($"Filter at index {index} is missing a 'field' value.");
        }

        var field = predicate.Field.Trim();
        if (!FieldPathRegex().IsMatch(field))
        {
            throw new ArgumentException(
                $"Filter at index {index} has an invalid field path '{field}'. Only letters, digits, underscores, '$' and '.' are allowed.");
        }

        ValidateDiscoveredField(predicate.Scope, field, index, discoveredFields);

        var fieldPath = predicate.Scope switch
        {
            PredicateScope.Device => field,
            PredicateScope.Tags => $"tags.{field}",
            PredicateScope.Desired => $"properties.desired.{field}",
            PredicateScope.Reported => $"properties.reported.{field}",
            _ => throw new ArgumentException($"Filter at index {index} has an unsupported scope '{predicate.Scope}'.")
        };

        var op = predicate.Operator switch
        {
            PredicateOperator.Equals => "=",
            PredicateOperator.NotEquals => "!=",
            PredicateOperator.LessThan => "<",
            PredicateOperator.LessThanOrEqual => "<=",
            PredicateOperator.GreaterThan => ">",
            PredicateOperator.GreaterThanOrEqual => ">=",
            _ => throw new ArgumentException($"Filter at index {index} has an unsupported operator '{predicate.Operator}'.")
        };

        var value = FormatValue(predicate.Value, index);

        return $"{fieldPath} {op} {value}";
    }

    private static void ValidateDiscoveredField(PredicateScope scope, string field, int index, QueryDiscoveredFields? discoveredFields)
    {
        if (discoveredFields is null)
        {
            return;
        }

        var fields = GetFieldsForScope(discoveredFields, scope);
        var scopeName = GetScopeName(scope);
        if (fields.Count == 0)
        {
            throw new ArgumentException(
                $"Filter at index {index} references '{scopeName}.{field}', but no discovered fields were provided for scope '{scopeName}'. Run iothub query discover and pass its fields to query compile.");
        }

        if (fields.Any(discoveredField => string.Equals(discoveredField.Field, field, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var suggestion = fields.FirstOrDefault(discoveredField =>
            discoveredField.Field.EndsWith($".{field}", StringComparison.OrdinalIgnoreCase)
            || string.Equals(GetLastPathSegment(discoveredField.Field), field, StringComparison.OrdinalIgnoreCase));
        var suggestionText = suggestion is null ? string.Empty : $" Did you mean '{suggestion.Field}'?";
        var knownFields = string.Join(", ", fields.Select(discoveredField => discoveredField.Field).Take(10));
        var additionalFieldCount = fields.Count > 10 ? $" and {fields.Count - 10} more" : string.Empty;

        throw new ArgumentException(
            $"Filter at index {index} references unknown field '{scopeName}.{field}'.{suggestionText} Discovered fields for scope '{scopeName}' include: {knownFields}{additionalFieldCount}.");
    }

    private static List<QueryDiscoveredField> GetFieldsForScope(QueryDiscoveredFields discoveredFields, PredicateScope scope) => scope switch
    {
        PredicateScope.Device => discoveredFields.Device,
        PredicateScope.Tags => discoveredFields.Tags,
        PredicateScope.Desired => discoveredFields.Desired,
        PredicateScope.Reported => discoveredFields.Reported,
        _ => []
    };

    private static string GetScopeName(PredicateScope scope) => scope switch
    {
        PredicateScope.Device => "device",
        PredicateScope.Tags => "tags",
        PredicateScope.Desired => "desired",
        PredicateScope.Reported => "reported",
        _ => scope.ToString()
    };

    private static string GetLastPathSegment(string field)
    {
        var lastDot = field.LastIndexOf('.');
        return lastDot < 0 ? field : field[(lastDot + 1)..];
    }

    private static string FormatValue(JsonElement value, int index)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Number:
                return value.GetRawText();
            case JsonValueKind.True:
                return "true";
            case JsonValueKind.False:
                return "false";
            case JsonValueKind.String:
                var raw = value.GetString() ?? string.Empty;
                // Escape single quotes by doubling them to keep the query well-formed.
                return $"'{raw.Replace("'", "''")}'";
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                throw new ArgumentException($"Filter at index {index} is missing a 'value'.");
            default:
                throw new ArgumentException(
                    $"Filter at index {index} has an unsupported value kind '{value.ValueKind}'. Use a string, number, or boolean.");
        }
    }

    [GeneratedRegex(@"^[A-Za-z0-9_$]+(\.[A-Za-z0-9_$]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex FieldPathRegex();
}
