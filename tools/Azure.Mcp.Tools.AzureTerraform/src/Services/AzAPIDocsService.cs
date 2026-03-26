// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.BicepSchema.Services;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public sealed class AzApiDocsService : IAzApiDocsService
{
    private static readonly Lazy<IServiceProvider> s_schemaServiceProvider = new(() =>
    {
        var services = new ServiceCollection();
        SchemaGenerator.ConfigureServices(services);
        return services.BuildServiceProvider();
    });

    public AzApiDocsResult GetDocumentation(string resourceTypeName, string? apiVersion = null)
    {
        var serviceProvider = s_schemaServiceProvider.Value;
        TypesDefinitionResult typesResult = SchemaGenerator.GetResourceTypeDefinitions(serviceProvider, resourceTypeName, apiVersion);
        List<ComplexType> complexTypes = SchemaGenerator.GetResponse(typesResult);

        string resolvedApiVersion = typesResult.ApiVersion;
        string fullName = $"{resourceTypeName}@{resolvedApiVersion}";
        string parentResourceType = GetParentResourceType(resourceTypeName);
        string writableScopes = GetWritableScopes(typesResult);
        string hclSchema = FormatAsHcl(resourceTypeName, resolvedApiVersion, parentResourceType, writableScopes, complexTypes);

        return new AzApiDocsResult
        {
            ResourceType = resourceTypeName,
            ApiVersion = resolvedApiVersion,
            Schema = hclSchema,
            ParentResourceType = parentResourceType,
            WritableScopes = writableScopes,
            Summary = $"AzAPI resource schema for {fullName}"
        };
    }

    private static string GetParentResourceType(string resourceTypeName)
    {
        string[] parts = resourceTypeName.Split('/');
        if (parts.Length > 2)
        {
            return string.Join("/", parts[..^1]);
        }
        return "Microsoft.Resources/resourceGroups";
    }

    private static string GetWritableScopes(TypesDefinitionResult typesResult)
    {
        if (typesResult.ResourceTypeEntities.Count > 0)
        {
            return typesResult.ResourceTypeEntities[0].WritableScopes;
        }
        return "Unknown";
    }

    internal static string FormatAsHcl(
        string resourceTypeName,
        string apiVersion,
        string parentResourceType,
        string writableScopes,
        List<ComplexType> complexTypes)
    {
        var sb = new StringBuilder();
        string fullName = $"{resourceTypeName}@{apiVersion}";

        sb.AppendLine($"# Resource Type: {fullName}");
        sb.AppendLine($"API Version: {apiVersion}");
        sb.AppendLine($"Parent resource type: {parentResourceType}");
        sb.AppendLine();
        sb.AppendLine("A json-like Resource Schema reference:");
        sb.AppendLine();

        // Get the resource label from the last part of resource type
        string[] resourceParts = resourceTypeName.Split('/');
        string lastPart = resourceParts[^1];
        string label = lastPart.EndsWith('s') ? lastPart[..^1] : lastPart;

        sb.AppendLine("```hcl");
        sb.AppendLine($"resource \"azapi_resource\" \"{label}\" {{");
        sb.AppendLine($"  type = \"{fullName}\"");
        sb.AppendLine($"  parent_id = {GetParentIdDescription(resourceTypeName, writableScopes)}");

        // Check if location should be added
        if (writableScopes.Contains("ResourceGroup", StringComparison.OrdinalIgnoreCase))
        {
            sb.AppendLine("  location = \"(Required) String. The geo-location where the resource lives\"");
        }

        // Find the main resource type entity and its body
        ResourceTypeEntity? resourceEntity = null;
        foreach (var ct in complexTypes)
        {
            if (ct is ResourceTypeEntity rte)
            {
                resourceEntity = rte;
                break;
            }
        }

        if (resourceEntity?.BodyType is ObjectTypeEntity bodyType)
        {
            // Build a lookup of all named complex types
            var typeIndex = new Dictionary<string, ComplexType>(StringComparer.OrdinalIgnoreCase);
            foreach (var ct in complexTypes)
            {
                if (ct is not ResourceTypeEntity)
                {
                    typeIndex[ct.Name] = ct;
                }
            }

            // Find the "properties" property in the body
            PropertyInfo? propertiesProp = null;
            foreach (var prop in bodyType.Properties)
            {
                if (prop.Name.Equals("properties", StringComparison.OrdinalIgnoreCase))
                {
                    propertiesProp = prop;
                }
            }

            if (propertiesProp != null && typeIndex.TryGetValue(propertiesProp.Type, out var propertiesType)
                && propertiesType is ObjectTypeEntity propertiesObj)
            {
                sb.AppendLine("  body = {");
                sb.AppendLine("    properties = {");
                FormatObjectProperties(sb, propertiesObj, typeIndex, 6);
                sb.AppendLine("    }");
                sb.AppendLine("  }");
            }

            // Add identity, sku, tags if present at top level
            foreach (var prop in bodyType.Properties)
            {
                if (prop.Name is "identity" or "sku" or "tags")
                {
                    if (typeIndex.TryGetValue(prop.Type, out var propType) && propType is ObjectTypeEntity objType)
                    {
                        sb.AppendLine($"  {prop.Name} = {{");
                        FormatObjectProperties(sb, objType, typeIndex, 4);
                        sb.AppendLine("  }");
                    }
                    else
                    {
                        sb.AppendLine($"  {prop.Name} = \"{FormatPropertyDescription(prop)}\"");
                    }
                }
            }
        }

        sb.AppendLine("}");
        sb.AppendLine("```");

        return sb.ToString();
    }

    private static void FormatObjectProperties(
        StringBuilder sb,
        ObjectTypeEntity objType,
        Dictionary<string, ComplexType> typeIndex,
        int indent)
    {
        string pad = new(' ', indent);

        foreach (var prop in objType.Properties)
        {
            // Skip read-only properties
            if (prop.Flags != null && prop.Flags.Contains("ReadOnly", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string required = prop.Flags != null && prop.Flags.Contains("Required", StringComparison.OrdinalIgnoreCase)
                ? "(Required)"
                : "(Optional)";

            // Check if property is a complex type
            if (typeIndex.TryGetValue(prop.Type, out var nestedType))
            {
                if (nestedType is ObjectTypeEntity nestedObj)
                {
                    string desc = string.IsNullOrEmpty(prop.Description) ? "" : $" // {prop.Description}";
                    sb.AppendLine($"{pad}{prop.Name} = {{ {required}{desc}");
                    FormatObjectProperties(sb, nestedObj, typeIndex, indent + 2);
                    sb.AppendLine($"{pad}}}");
                }
                else if (nestedType is DiscriminatedObjectTypeEntity discObj)
                {
                    string desc = string.IsNullOrEmpty(prop.Description) ? "" : $" // {prop.Description}";
                    sb.AppendLine($"{pad}{prop.Name} = \"{required} Discriminated by '{discObj.Discriminator}'.{desc}\"");
                }
                else
                {
                    sb.AppendLine($"{pad}{prop.Name} = \"{FormatPropertyDescriptionWithRequired(required, prop)}\"");
                }
            }
            else if (prop.Type.EndsWith("[]", StringComparison.Ordinal))
            {
                // Array type
                string elementType = prop.Type[..^2];
                if (typeIndex.TryGetValue(elementType, out var elementComplexType) && elementComplexType is ObjectTypeEntity elementObj)
                {
                    string desc = string.IsNullOrEmpty(prop.Description) ? "" : $" // {prop.Description}";
                    sb.AppendLine($"{pad}{prop.Name} = [ {{ {required} Array.{desc}");
                    FormatObjectProperties(sb, elementObj, typeIndex, indent + 2);
                    sb.AppendLine($"{pad}}} ]");
                }
                else
                {
                    sb.AppendLine($"{pad}{prop.Name} = \"{required} Array of {elementType}. {prop.Description ?? ""}\"");
                }
            }
            else
            {
                sb.AppendLine($"{pad}{prop.Name} = \"{FormatPropertyDescriptionWithRequired(required, prop)}\"");
            }
        }
    }

    private static string FormatPropertyDescriptionWithRequired(string required, PropertyInfo prop)
    {
        string typeDesc = MapTypeToDescription(prop.Type);
        string desc = prop.Description ?? "";
        string modifiers = prop.Modifiers != null ? $" [{prop.Modifiers}]" : "";
        return $"{required} {typeDesc}. {desc}{modifiers}".Trim();
    }

    private static string FormatPropertyDescription(PropertyInfo prop)
    {
        string required = prop.Flags != null && prop.Flags.Contains("Required", StringComparison.OrdinalIgnoreCase)
            ? "(Required)"
            : "(Optional)";
        return FormatPropertyDescriptionWithRequired(required, prop);
    }

    private static string MapTypeToDescription(string typeName)
    {
        return typeName.ToLowerInvariant() switch
        {
            "string" => "String",
            "int" => "Integer",
            "bool" => "Boolean",
            _ when typeName.Contains('|') => $"One of: {typeName}",
            _ => typeName
        };
    }

    private static string GetParentIdDescription(string resourceTypeName, string writableScopes)
    {
        string[] parts = resourceTypeName.Split('/');
        if (parts.Length > 2)
        {
            string parentType = string.Join("/", parts[..^1]);
            return $"\"Reference to the `id` property of resource of type: `{parentType}`\"";
        }

        if (writableScopes.Contains("Tenant", StringComparison.OrdinalIgnoreCase))
        {
            return "\"A tenant id in format /tenants/{tenantId}\"";
        }
        if (writableScopes.Contains("ManagementGroup", StringComparison.OrdinalIgnoreCase))
        {
            return "\"A management group id in format /providers/Microsoft.Management/managementGroups/{managementGroupId}\"";
        }
        if (writableScopes.Contains("Subscription", StringComparison.OrdinalIgnoreCase))
        {
            return "\"A subscription id in format /subscriptions/{subscriptionId}\"";
        }
        if (writableScopes.Contains("ResourceGroup", StringComparison.OrdinalIgnoreCase))
        {
            return "\"Reference to the `id` property of a `Microsoft.Resources/resourceGroups`\"";
        }

        return "\"Unknown scope\"";
    }
}
