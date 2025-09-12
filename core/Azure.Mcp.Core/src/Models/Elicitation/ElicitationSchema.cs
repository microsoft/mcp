// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Models;

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
        var schema = new ElicitationSchemaRoot
        {
            Properties = new Dictionary<string, ElicitationSchemaProperty>
            {
                [propertyName] = new ElicitationSchemaProperty
                {
                    Title = title,
                    Description = description
                }
            },
            Required = isRequired ? [propertyName] : null
        };

        return JsonSerializer.SerializeToNode(schema, ModelsJsonContext.Default.ElicitationSchemaRoot)!.AsObject();
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
        var schema = new ElicitationSchemaRoot
        {
            Properties = new Dictionary<string, ElicitationSchemaProperty>
            {
                [propertyName] = new ElicitationSchemaProperty
                {
                    Title = title,
                    Description = description,
                    Format = "password"
                }
            },
            Required = isRequired ? [propertyName] : null
        };

        return JsonSerializer.SerializeToNode(schema, ModelsJsonContext.Default.ElicitationSchemaRoot)!.AsObject();
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
