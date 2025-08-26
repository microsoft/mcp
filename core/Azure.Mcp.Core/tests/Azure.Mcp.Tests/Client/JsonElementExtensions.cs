// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tests.Client;

public static class JsonElementExtensions
{
    public static bool IsError(this JsonElement? element)
    {
        if (element == null || element.Value.ValueKind != JsonValueKind.Object)
        {
            return true;
        }

        return element.Value.TryGetProperty("error", out var errorProperty) && 
               errorProperty.ValueKind != JsonValueKind.Null;
    }

    public static bool IsSuccess(this JsonElement? element)
    {
        return !element.IsError();
    }

    public static JsonElement? Content(this JsonElement? element)
    {
        if (element == null || element.Value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (element.Value.TryGetProperty("content", out var contentProperty) && 
            contentProperty.ValueKind != JsonValueKind.Null)
        {
            return contentProperty;
        }

        return element;
    }
}
