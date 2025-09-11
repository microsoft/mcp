// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;

namespace Azure.Mcp.Core.Models.Elicitation;

/// <summary>
/// Utility class for creating elicitation schema objects.
/// </summary>
public static class ElicitationSchema
{
    /// <summary>
    /// Creates a simple string input schema for elicitation.
    /// </summary>
    /// <param name="propertyName">The name of the property to request.</param>
    /// <param name="title">The display title for the property.</param>
    /// <param name="description">The description of what the property represents.</param>
    /// <param name="isRequired">Whether the property is required.</param>
    /// <returns>A JSON schema object for the elicitation request.</returns>
    public static JsonObject CreateStringSchema(string propertyName, string title, string description, bool isRequired = true)
    {
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                [propertyName] = new JsonObject
                {
                    ["type"] = "string",
                    ["title"] = title,
                    ["description"] = description
                }
            }
        };

        if (isRequired)
        {
            schema["required"] = JsonNode.Parse($"[\"{propertyName}\"]");
        }

        return schema;
    }

    /// <summary>
    /// Creates a password input schema for elicitation.
    /// </summary>
    /// <param name="propertyName">The name of the property to request.</param>
    /// <param name="title">The display title for the property.</param>
    /// <param name="description">The description of what the property represents.</param>
    /// <param name="isRequired">Whether the property is required.</param>
    /// <returns>A JSON schema object for the elicitation request.</returns>
    public static JsonObject CreatePasswordSchema(string propertyName, string title, string description, bool isRequired = true)
    {
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                [propertyName] = new JsonObject
                {
                    ["type"] = "string",
                    ["title"] = title,
                    ["description"] = description,
                    ["format"] = "password"
                }
            }
        };

        if (isRequired)
        {
            schema["required"] = JsonNode.Parse($"[\"{propertyName}\"]");
        }

        return schema;
    }

    /// <summary>
    /// Creates a secret value input schema for elicitation.
    /// </summary>
    /// <param name="propertyName">The name of the property to request.</param>
    /// <param name="title">The display title for the property.</param>
    /// <param name="description">The description of what the property represents.</param>
    /// <param name="isRequired">Whether the property is required.</param>
    /// <returns>A JSON schema object for the elicitation request.</returns>
    public static JsonObject CreateSecretSchema(string propertyName, string title, string description, bool isRequired = true)
    {
        return CreatePasswordSchema(propertyName, title, description, isRequired);
    }
}
