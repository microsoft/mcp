// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class FabricItemJsonInput
{
    internal const int MaxObjectBytes = 1024 * 1024;
    internal const int MaxObjectDepth = 32;

    internal static JsonElement? ParseObject(string? json, string option, int maxBytes = MaxObjectBytes)
    {
        if (json is null)
        {
            return null;
        }
        if (json.Length > maxBytes || Encoding.UTF8.GetByteCount(json) > maxBytes)
        {
            throw new FabricItemInputException(FormattableString.Invariant(
                $"{option} must not exceed {maxBytes} UTF-8 bytes."));
        }
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                MaxDepth = MaxObjectDepth,
                AllowDuplicateProperties = false
            });
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new FabricItemInputException($"{option} must be a JSON object, not null, an array, or a scalar.");
            }
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new FabricItemInputException(FormattableString.Invariant(
                $"{option} must contain valid JSON with no duplicate properties and at most {MaxObjectDepth} nesting levels."));
        }
    }
}
