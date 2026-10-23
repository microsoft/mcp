// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Shared per-call tenant handling for tools proxied from tenant-scoped registry servers, used by
/// every proxy mode's tool loader.
/// </summary>
public static class RegistryTenantScope
{
    public const string TenantPropertyName = "tenant";

    private const string TenantPropertyDescription =
        "Optional Microsoft Entra tenant ID (GUID) to run this call against. Defaults to the tenant of " +
        "the identity the server runs as, and applies only when the server runs under its own identity.";

    /// <summary>
    /// Adds an optional <c>tenant</c> property to a proxied tool's schema. A tool that declares its
    /// own <c>tenant</c> is left untouched, since the upstream server owns that argument.
    /// </summary>
    public static JsonElement WithTenantProperty(JsonElement inputSchema)
    {
        if (SchemaDeclaresTenant(inputSchema) || JsonNode.Parse(inputSchema.GetRawText()) is not JsonObject schema)
        {
            return inputSchema;
        }

        if (schema["properties"] is not JsonObject properties)
        {
            properties = [];
            schema["properties"] = properties;
        }

        properties[TenantPropertyName] = new JsonObject
        {
            ["type"] = "string",
            ["description"] = TenantPropertyDescription
        };

        // Parse rather than serialize: keeps the result a self-contained JsonElement and needs no
        // serializer context under trimming or AOT.
        using var document = JsonDocument.Parse(schema.ToJsonString());
        return document.RootElement.Clone();
    }

    /// <summary>
    /// Returns <paramref name="tool"/> with the <c>tenant</c> property added, or the same instance
    /// when its schema already declares one.
    /// </summary>
    public static Tool WithTenantProperty(Tool tool)
    {
        if (SchemaDeclaresTenant(tool.InputSchema))
        {
            return tool;
        }

        return new Tool
        {
            Name = tool.Name,
            Title = tool.Title,
            Description = tool.Description,
            InputSchema = WithTenantProperty(tool.InputSchema),
            OutputSchema = tool.OutputSchema,
            Annotations = tool.Annotations,
            Icons = tool.Icons,
            Meta = tool.Meta
        };
    }

    public static bool SchemaDeclaresTenant(JsonElement inputSchema) =>
        inputSchema.ValueKind == JsonValueKind.Object
        && inputSchema.TryGetProperty("properties", out var properties)
        && properties.ValueKind == JsonValueKind.Object
        && properties.TryGetProperty(TenantPropertyName, out _);

    /// <summary>
    /// Removes the <c>tenant</c> argument and reports it. A present but unusable value is an error
    /// rather than a fall back to the hosting tenant, which would be the wrong tenant's data.
    /// </summary>
    public static bool TryConsumeTenantArgument(
        Dictionary<string, object?> parameters,
        out string? tenantId,
        out string? error)
    {
        tenantId = null;
        error = null;

        if (!parameters.Remove(TenantPropertyName, out var value) || value is null)
        {
            return true;
        }

        var text = value switch
        {
            JsonElement { ValueKind: JsonValueKind.Null } => null,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            JsonElement element => element.GetRawText(),
            string s => s,
            _ => value.ToString()
        };

        if (text is null)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(text) || !Guid.TryParse(text.Trim(), out _))
        {
            error = $"'{TenantPropertyName}' must be a Microsoft Entra tenant ID (GUID); got '{text}'.";
            return false;
        }

        tenantId = text.Trim();
        return true;
    }

    /// <summary>
    /// Publishes <paramref name="tenantId"/> as the ambient tenant until disposed, restoring the
    /// previous value so a nested call cannot strand an outer one on the hosting tenant.
    /// </summary>
    public static Scope Enter(string? tenantId) => new(tenantId);

    public readonly struct Scope : IDisposable
    {
        private readonly string? _previous;

        internal Scope(string? tenantId)
        {
            _previous = RegistryTenantContext.CurrentTenantId;
            RegistryTenantContext.CurrentTenantId = tenantId;
        }

        public void Dispose() => RegistryTenantContext.CurrentTenantId = _previous;
    }
}
