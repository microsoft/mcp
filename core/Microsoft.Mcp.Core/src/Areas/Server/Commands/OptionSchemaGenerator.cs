// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Helpers;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Builds the JSON Schema fragment that describes a command's input parameters
/// (the MCP <c>inputSchema</c>) by delegating per-option type mapping to
/// <see cref="JsonSchemaExporter"/>.
/// </summary>
/// <remarks>
/// AOT note: <c>Microsoft.Mcp.Core</c> sets <c>IsAotCompatible=true</c>, which
/// promotes <c>RequiresUnreferencedCode</c>/<c>RequiresDynamicCode</c> warnings
/// into build errors. Option value types come from <c>System.CommandLine</c> as
/// runtime <see cref="Type"/> values, so this helper has to use
/// <see cref="DefaultJsonTypeInfoResolver"/> and the non-generic
/// <see cref="JsonStringEnumConverter"/> — both annotated. The
/// <see cref="UnconditionalSuppressMessageAttribute"/> attributes below are
/// load-bearing: removing them breaks <c>dotnet build</c>. They are sound
/// because the helper uses the exporter only to read schema metadata (no
/// (de)serialization, no enum materialization), and because the actual option
/// types in use are primitives, enums, <see cref="Guid"/>, nullables of those,
/// and arrays of those — shapes that the default resolver handles without
/// trimming or dynamic-codegen concerns.
/// </remarks>
internal static class OptionSchemaGenerator
{
    private static readonly JsonSerializerOptions s_schemaOptions = CreateSchemaOptions();

    private static readonly JsonSchemaExporterOptions s_exporterOptions = new()
    {
        TreatNullObliviousAsNonNullable = true,
    };

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification = "Option value types are primitives, enums, and arrays of primitives that the default resolver handles without trim concerns.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "Option value types are primitives, enums, and arrays of primitives that the default resolver handles without dynamic code generation.")]
    private static JsonSerializerOptions CreateSchemaOptions()
    {
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters = { new JsonStringEnumConverter() },
        };
        options.MakeReadOnly();
        return options;
    }

    /// <summary>
    /// Returns a JSON Schema fragment for the supplied option value type, with the
    /// option description applied at the root.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification = "Option value types are primitives, enums, and arrays of primitives that the default resolver handles without trim concerns.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "Option value types are primitives, enums, and arrays of primitives that the default resolver handles without dynamic code generation.")]
    public static JsonNode CreatePropertySchema(Type optionType, string? description)
    {
        ArgumentNullException.ThrowIfNull(optionType);

        var schema = JsonSchemaExporter.GetJsonSchemaAsNode(s_schemaOptions, optionType, s_exporterOptions);

        if (schema is JsonObject schemaObject && !string.IsNullOrWhiteSpace(description))
        {
            schemaObject["description"] = description;
        }

        return schema;
    }

    /// <summary>
    /// Builds the <c>inputSchema</c> root <see cref="JsonObject"/> for the supplied options.
    /// Always emits an <c>"object"</c> schema with <c>additionalProperties: false</c> for OpenAI strict-mode compatibility.
    /// </summary>
    public static JsonObject CreateInputSchema(IReadOnlyList<Option> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var properties = new JsonObject();
        var required = new JsonArray();

        foreach (var option in options)
        {
            var propName = NameNormalization.NormalizeOptionName(option.Name);
            properties[propName] = CreatePropertySchema(option.ValueType, option.Description);

            if (option.Required)
            {
                required.Add((JsonNode)propName);
            }
        }

        var root = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties,
        };

        if (required.Count > 0)
        {
            root["required"] = required;
        }

        root["additionalProperties"] = false;

        return root;
    }

    /// <summary>
    /// Builds the <c>outputSchema</c> root <see cref="JsonObject"/> for a command's success payload using
    /// the supplied source-generated <see cref="JsonTypeInfo"/>. Strips schema metadata that the MCP SDK's
    /// <c>IsValidMcpToolSchema</c> validator rejects (<c>$schema</c>, <c>title</c>) and ensures the root carries
    /// <c>"type": "object"</c>. <c>additionalProperties</c> is intentionally left unset so server-side fields
    /// added later don't break clients.
    /// </summary>
    /// <remarks>
    /// AOT note: the supplied <paramref name="typeInfo"/> is source-generated by each toolset's
    /// <c>*JsonContext</c>, so this code path does not need the <see cref="UnconditionalSuppressMessageAttribute"/>
    /// attributes that <see cref="CreatePropertySchema"/> carries.
    /// </remarks>
    public static JsonObject CreateOutputSchema(JsonTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);

        var schema = JsonSchemaExporter.GetJsonSchemaAsNode(typeInfo.Options, typeInfo.Type, s_exporterOptions);

        // The exporter may emit a non-object root for primitives/arrays. The MCP SDK's IsValidMcpToolSchema
        // requires an object schema at the root, so wrap such payloads.
        JsonObject root = schema as JsonObject ?? new JsonObject { ["type"] = "object", ["properties"] = new JsonObject { ["value"] = schema } };

        StripUnsupportedKeywords(root);

        // Ensure top-level "type": "object" is present even when the exporter emits only "$ref" or "anyOf".
        if (root["type"] is null)
        {
            root["type"] = "object";
        }

        return root;
    }

    private static void StripUnsupportedKeywords(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            obj.Remove("$schema");
            obj.Remove("title");
            foreach (var kvp in obj.ToList())
            {
                StripUnsupportedKeywords(kvp.Value);
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                StripUnsupportedKeywords(item);
            }
        }
    }
}
