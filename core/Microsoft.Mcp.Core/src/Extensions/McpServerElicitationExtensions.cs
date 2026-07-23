// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Elicitation;

namespace Microsoft.Mcp.Core.Extensions;

/// <summary>
/// Extension methods for MCP server to support elicitation functionality.
/// </summary>
public static class McpServerElicitationExtensions
{
    /// <summary>
    /// Sends an elicitation request to gather user input with validation and error handling.
    /// </summary>
    /// <remarks>
    /// This extension wraps <see cref="McpServer.ElicitAsync"/> to translate between our internal
    /// <see cref="ElicitationRequestParams"/>/<see cref="ElicitationAction"/> types and the SDK's
    /// protocol types. The underlying SDK API is unchanged between SDK <c>1.1.0</c> and
    /// <c>2.0.0-preview.1</c> — both versions expose the same <c>ElicitAsync</c> overload with
    /// the same <c>ElicitRequestParams</c>/<c>ElicitResult</c> shapes.
    /// </remarks>
    /// <param name="server">The MCP server instance.</param>
    /// <param name="request">The elicitation request parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous elicitation operation.</returns>
    public static async Task<ElicitationResponse> RequestElicitationAsync(
        this McpServer server,
        ElicitationRequestParams request,
        CancellationToken cancellationToken = default)
    {
        if (server == null)
            throw new ArgumentNullException(nameof(server));
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(request));

        // Check if client supports elicitation
        if (server.ClientCapabilities?.Elicitation == null)
        {
            throw new NotSupportedException("Client does not support elicitation. Elicitation capability is required.");
        }

        // Create the proper MCP protocol elicitation request
        var protocolRequest = new ModelContextProtocol.Protocol.ElicitRequestParams
        {
            Message = request.Message
        };

        if (request.RequestedSchema != null)
        {
            protocolRequest.RequestedSchema = CreateRequestedSchema(request.RequestedSchema);
        }

        // Send the real elicitation request through the MCP SDK
        var protocolResponse = await server.ElicitAsync(protocolRequest, cancellationToken);

        // Convert protocol response to our ElicitationResponse
        var elicitationResponse = new ElicitationResponse
        {
            Action = protocolResponse.Action switch
            {
                "accept" => ElicitationAction.Accept,
                "cancel" => ElicitationAction.Cancel,
                _ => ElicitationAction.Decline,
            }
        };

        return elicitationResponse;
    }

    // Translate our JsonObject-based schema into the SDK's typed PrimitiveSchemaDefinition
    // hierarchy. The SDK does not accept a raw JsonObject — it requires each property to be
    // represented as a concrete schema type (string, boolean, integer, enum, etc.) so that
    // the MCP client can render an appropriate form widget for each field.
    private static ModelContextProtocol.Protocol.ElicitRequestParams.RequestSchema CreateRequestedSchema(JsonObject requestedSchema)
    {
        var protocolSchema = new ModelContextProtocol.Protocol.ElicitRequestParams.RequestSchema();

        if (requestedSchema.TryGetPropertyValue("properties", out var propertiesNode) && propertiesNode is JsonObject propertiesObject)
        {
            var properties = new Dictionary<string, ModelContextProtocol.Protocol.ElicitRequestParams.PrimitiveSchemaDefinition>(StringComparer.Ordinal);
            foreach (var property in propertiesObject)
            {
                if (property.Value == null)
                {
                    continue;
                }

                var schemaDefinition = CreatePrimitiveSchemaDefinition(property.Value);

                if (schemaDefinition != null)
                {
                    properties[property.Key] = schemaDefinition;
                }
            }

            protocolSchema.Properties = properties;
        }

        if (requestedSchema.TryGetPropertyValue("required", out var requiredNode) && requiredNode is JsonArray requiredArray)
        {
            protocolSchema.Required = requiredArray
                .Select(node => node?.GetValue<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Cast<string>()
                .ToList();
        }

        return protocolSchema;
    }

    private static ModelContextProtocol.Protocol.ElicitRequestParams.PrimitiveSchemaDefinition? CreatePrimitiveSchemaDefinition(JsonNode schemaNode)
    {
        if (schemaNode is not JsonObject schemaObject)
        {
            return null;
        }

        string? title = GetString(schemaObject, "title");
        string? description = GetString(schemaObject, "description");

        if (schemaObject.TryGetPropertyValue("oneOf", out var oneOfNode) && oneOfNode is JsonArray oneOfArray)
        {
            return new ModelContextProtocol.Protocol.ElicitRequestParams.TitledSingleSelectEnumSchema
            {
                Title = title,
                Description = description,
                OneOf = oneOfArray
                    .OfType<JsonObject>()
                    .Select(option => new ModelContextProtocol.Protocol.ElicitRequestParams.EnumSchemaOption
                    {
                        Title = GetString(option, "title") ?? string.Empty,
                        Const = GetString(option, "const") ?? string.Empty,
                    })
                    .ToList(),
            };
        }

        return GetString(schemaObject, "type") switch
        {
            "string" => new ModelContextProtocol.Protocol.ElicitRequestParams.StringSchema
            {
                Title = title,
                Description = description,
                MinLength = GetInt(schemaObject, "minLength"),
                MaxLength = GetInt(schemaObject, "maxLength"),
                Format = GetString(schemaObject, "format"),
                Default = GetString(schemaObject, "default"),
            },
            "number" or "integer" => new ModelContextProtocol.Protocol.ElicitRequestParams.NumberSchema
            {
                Title = title,
                Description = description,
                Minimum = GetDouble(schemaObject, "minimum"),
                Maximum = GetDouble(schemaObject, "maximum"),
                Default = GetDouble(schemaObject, "default"),
            },
            "boolean" => new ModelContextProtocol.Protocol.ElicitRequestParams.BooleanSchema
            {
                Title = title,
                Description = description,
                Default = GetBool(schemaObject, "default"),
            },
            _ => null,
        };
    }

    private static string? GetString(JsonObject obj, string propertyName)
        => obj.TryGetPropertyValue(propertyName, out var node) && node is JsonValue value && value.TryGetValue(out string? result)
            ? result
            : null;

    private static int? GetInt(JsonObject obj, string propertyName)
        => obj.TryGetPropertyValue(propertyName, out var node) && node is JsonValue value && value.TryGetValue(out int result)
            ? result
            : null;

    private static double? GetDouble(JsonObject obj, string propertyName)
        => obj.TryGetPropertyValue(propertyName, out var node) && node is JsonValue value && value.TryGetValue(out double result)
            ? result
            : null;

    private static bool? GetBool(JsonObject obj, string propertyName)
        => obj.TryGetPropertyValue(propertyName, out var node) && node is JsonValue value && value.TryGetValue(out bool result)
            ? result
            : null;

    /// <summary>
    /// Checks if the client supports elicitation.
    /// </summary>
    /// <param name="server">The MCP server instance.</param>
    /// <returns>True if the client supports elicitation, false otherwise.</returns>
    public static bool SupportsElicitation(this McpServer server)
    {
        return server?.ClientCapabilities?.Elicitation != null;
    }

    /// <summary>
    /// Checks if elicitation should be triggered for a tool based on its metadata.
    /// </summary>
    /// <param name="server">The MCP server instance.</param>
    /// <param name="toolMetadata">The tool metadata to check.</param>
    /// <returns>True if elicitation should be triggered, false otherwise.</returns>
    public static bool ShouldTriggerElicitation(this McpServer server, object? toolMetadata)
    {
        if (!server.SupportsElicitation())
        {
            return false;
        }

        if (toolMetadata is JsonObject jsonMetadata)
        {
            // tool.Meta uses "SecretHint" (set in CommandFactoryToolLoader/NamespaceToolLoader)
            if (jsonMetadata.TryGetPropertyValue(McpHelper.SecretHintMetaKey, out var secretValue) &&
                secretValue is JsonValue secretJsonValue &&
                secretJsonValue.TryGetValue(out bool isSecret) &&
                isSecret)
            {
                return true;
            }

            // tool.Meta uses "DestructiveHint" when present; also check ToolAnnotations-style keys
            if (jsonMetadata.TryGetPropertyValue("DestructiveHint", out var destructiveValue) &&
                destructiveValue is JsonValue destructiveJsonValue &&
                destructiveJsonValue.TryGetValue(out bool isDestructive) &&
                isDestructive)
            {
                return true;
            }
        }

        return false;
    }
}
