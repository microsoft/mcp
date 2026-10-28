// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Models.Ingestion;

namespace Azure.Mcp.Tools.Monitor.Validation;

/// <summary>
/// Validates log data against Logs Ingestion API payload requirements and, optionally, a declared stream schema.
/// </summary>
internal static class IngestionLogDataValidator
{
    /// <summary>The maximum size of a single Logs Ingestion API request body.</summary>
    internal const int MaxPayloadBytes = 1024 * 1024;

    private const int MaxIssues = 50;

    /// <summary>
    /// Validates the payload shape and, when <paramref name="schema"/> is provided, each record against the declared columns.
    /// </summary>
    /// <param name="logData">The JSON array of log records.</param>
    /// <param name="schema">Declared column names mapped to their data collection rule column types, or null to check shape only.</param>
    internal static IngestionValidationResult Validate(string? logData, IReadOnlyDictionary<string, string>? schema)
    {
        var errors = new List<IngestionValidationIssue>();
        var warnings = new List<IngestionValidationIssue>();

        if (string.IsNullOrWhiteSpace(logData))
        {
            errors.Add(new(null, null, "Log data is empty. Provide a JSON array of log records."));
            return Build(0, schema is not null, errors, warnings);
        }

        if (Encoding.UTF8.GetByteCount(logData) > MaxPayloadBytes)
        {
            errors.Add(new(null, null, $"Log data exceeds the maximum request size of {MaxPayloadBytes} bytes. Split it into smaller batches."));
            return Build(0, schema is not null, errors, warnings);
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(logData);
        }
        catch (JsonException ex)
        {
            errors.Add(new(null, null, $"Log data is not valid JSON: {ex.Message}"));
            return Build(0, schema is not null, errors, warnings);
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array)
            {
                errors.Add(new(null, null, "Log data must be a JSON array of objects, for example [{\"TimeGenerated\":\"2024-01-01T00:00:00Z\",\"Message\":\"hello\"}]."));
                return Build(0, schema is not null, errors, warnings);
            }

            int count = root.GetArrayLength();
            if (count == 0)
            {
                errors.Add(new(null, null, "Log data contains no records."));
                return Build(0, schema is not null, errors, warnings);
            }

            var seenColumns = new HashSet<string>(StringComparer.Ordinal);
            var unknownColumns = new HashSet<string>(StringComparer.Ordinal);
            int index = 0;
            foreach (var record in root.EnumerateArray())
            {
                if (record.ValueKind != JsonValueKind.Object)
                {
                    AddIssue(errors, new(index, null, $"Record is a JSON {record.ValueKind}, but each record must be a JSON object."));
                }
                else if (schema is not null)
                {
                    ValidateRecord(record, index, schema, seenColumns, unknownColumns, errors, warnings);
                }

                index++;
            }

            if (schema is not null)
            {
                foreach (var column in schema.Keys.Where(c => !seenColumns.Contains(c)).Order(StringComparer.Ordinal))
                {
                    AddIssue(warnings, new(null, column, $"Declared column '{column}' is not present in any record and will be null."));
                }
            }

            return Build(count, schema is not null, errors, warnings);
        }
    }

    private static void ValidateRecord(
        JsonElement record,
        int index,
        IReadOnlyDictionary<string, string> schema,
        HashSet<string> seenColumns,
        HashSet<string> unknownColumns,
        List<IngestionValidationIssue> errors,
        List<IngestionValidationIssue> warnings)
    {
        foreach (var property in record.EnumerateObject())
        {
            if (!schema.TryGetValue(property.Name, out var columnType))
            {
                if (unknownColumns.Add(property.Name))
                {
                    AddIssue(warnings, new(index, property.Name, $"Column '{property.Name}' is not declared in the stream schema and is dropped unless a transformation uses it."));
                }

                continue;
            }

            seenColumns.Add(property.Name);
            var typeError = GetTypeError(property.Value, columnType);
            if (typeError is not null)
            {
                AddIssue(errors, new(index, property.Name, typeError));
            }
        }
    }

    private static string? GetTypeError(JsonElement value, string columnType)
    {
        if (value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        bool valid = columnType.ToLowerInvariant() switch
        {
            "string" => value.ValueKind == JsonValueKind.String,
            "int" => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out _),
            "long" => value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out _),
            "real" or "double" => value.ValueKind == JsonValueKind.Number,
            "boolean" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "datetime" => value.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _),
            "guid" => value.ValueKind == JsonValueKind.String && Guid.TryParse(value.GetString(), out _),
            _ => true
        };

        return valid
            ? null
            : $"Value of kind {value.ValueKind} is not compatible with column type '{columnType}'.";
    }

    private static void AddIssue(List<IngestionValidationIssue> issues, IngestionValidationIssue issue)
    {
        if (issues.Count < MaxIssues)
        {
            issues.Add(issue);
        }
    }

    private static IngestionValidationResult Build(
        int recordCount,
        bool schemaChecked,
        List<IngestionValidationIssue> errors,
        List<IngestionValidationIssue> warnings) =>
        new(errors.Count == 0, null, null, recordCount, schemaChecked, errors, warnings);
}
