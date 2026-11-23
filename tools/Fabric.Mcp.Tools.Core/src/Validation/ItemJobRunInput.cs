// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class ItemJobRunInput
{
    internal const int MaxJobTypeBytes = 1024;
    internal const int MaxRequestBytes = FabricItemJsonInput.MaxObjectBytes;

    internal static void ValidateContext(string workspaceId, string itemId, string jobType, ValidationResult validation)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            validation.Errors.Add("--workspace-id must be a nonempty UUID.");
        }
        if (!Guid.TryParse(itemId, out var item) || item == Guid.Empty)
        {
            validation.Errors.Add("--item-id must be a nonempty UUID.");
        }
        // Uri.EscapeDataString leaves dot segments unchanged, and Uri would normalize them.
        // Reject those representations rather than silently changing the requested route.
        if (string.IsNullOrWhiteSpace(jobType) || jobType is "." or ".." ||
            jobType.Length > MaxJobTypeBytes || Encoding.UTF8.GetByteCount(jobType) > MaxJobTypeBytes ||
            jobType.Any(char.IsControl) || !IsWellFormedUnicode(jobType))
        {
            validation.Errors.Add("--job-type must be nonempty and not whitespace, valid Unicode without control characters or dot segments, within the 1,024 UTF-8 byte tool budget.");
        }
    }

    internal static string? CreateBody(string? executionData, string? jobParameters, CancellationToken cancellationToken)
    {
        try
        {
            return SerializeBody(executionData, jobParameters, cancellationToken);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or ArgumentException { InnerException: EncoderFallbackException })
        {
            // JsonDocument decodes escapes lazily; malformed Unicode can surface during
            // inspection or serialization, including wrapped decoder failures.
            throw new FabricItemInputException("Job JSON input must contain valid Unicode strings and property names.");
        }
    }

    private static string? SerializeBody(string? executionData, string? jobParameters, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (executionData is null && jobParameters is null)
        {
            return null;
        }

        var execution = FabricItemJsonInput.ParseObject(executionData, "--execution-data");
        var parameters = FabricItemJsonInput.ParseArray(jobParameters, "--job-parameters");
        if (parameters is { } array)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var parameter in array.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (parameter.ValueKind != JsonValueKind.Object ||
                    !parameter.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String ||
                    !parameter.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String ||
                    !parameter.TryGetProperty("value", out _))
                {
                    throw new FabricItemInputException("--job-parameters requires objects with string name and type fields and a value field.");
                }
                var parameterName = name.GetString()!;
                _ = type.GetString();
                if (parameterName.Length > 256 || !names.Add(parameterName))
                {
                    throw new FabricItemInputException("--job-parameters names must be case-insensitively unique and at most 256 characters.");
                }
                // The schema's value is open-ended and its examples include scalars. Do not
                // coerce values, close the extensible type set, or discard extension properties.
            }
        }

        var body = JsonSerializer.Serialize(new ItemJobRunRequest(execution, parameters), CoreJsonContext.Default.ItemJobRunRequest);
        if (Encoding.UTF8.GetByteCount(body) > MaxRequestBytes)
        {
            throw new FabricItemInputException("The serialized job request must not exceed the 1 MiB (1,048,576 byte) tool budget.");
        }
        cancellationToken.ThrowIfCancellationRequested();
        return body;
    }

    private static bool IsWellFormedUnicode(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (!char.IsSurrogate(value[index]))
            {
                continue;
            }
            if (!char.IsHighSurrogate(value[index]) || index + 1 == value.Length || !char.IsLowSurrogate(value[++index]))
            {
                return false;
            }
        }
        return true;
    }
}
