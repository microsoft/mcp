// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Bicep.Types;
using Azure.Bicep.Types.Concrete;
using Azure.Bicep.Types.Index;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;
using Azure.Mcp.Tools.BicepSchema.Services.Support;


// This is mostly from
// https://msazure.visualstudio.com/One/_git/AzureUX-Deployments-Tooling?path=%2FBicepTypesDefinitions%2FResourcePropertiesApp%2FProgram.cs, which
// is based off of the code in https://github.com/Azure/bicep-types/blob/main/src/bicep-types/src/writers/markdown.ts

namespace Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties;

public class ResourceVisitor
{
    private readonly ITypeLoader _azTypeLoader;
    private readonly Lazy<TypeIndex> _typeIndex;

    // Sample key of a resource: "Microsoft.App/containerApps/authConfigs@2024-03-01"
    private readonly Lazy<IReadOnlyDictionary<string, CrossFileTypeReference>> _resources;

    // Sample key of a resource function: "microsoft.app/containerapps"
    // Sample value of a resource function: { "2024-03-01": [ function1, function2 ], "2024-08-02-preview": [ function1, function2] }
    private readonly Lazy<InsensitiveDictionary<IReadOnlyDictionary<string, IReadOnlyList<CrossFileTypeReference>>>> _resourceFunctions;

    // TODO: Consider adding k8s types
    public ResourceVisitor(ITypeLoader azTypeLoader)
    {
        _azTypeLoader = azTypeLoader;
        _typeIndex = new Lazy<TypeIndex>(azTypeLoader.LoadTypeIndex);
        _resources = new Lazy<IReadOnlyDictionary<string, CrossFileTypeReference>>(() => new InsensitiveDictionary<CrossFileTypeReference>(_typeIndex.Value.Resources.ToDictionary()));
        _resourceFunctions = new Lazy<InsensitiveDictionary<IReadOnlyDictionary<string, IReadOnlyList<CrossFileTypeReference>>>>(
            () => new InsensitiveDictionary<IReadOnlyDictionary<string, IReadOnlyList<CrossFileTypeReference>>>(
                _typeIndex.Value.ResourceFunctions.ToDictionary()));
    }

    public InsensitiveDictionary<ProviderResourceTypes> GetAllResourceTypesAndVersionsByProvider()
    {
        var providers = new InsensitiveDictionary<ProviderResourceTypes>();

        foreach (KeyValuePair<string, CrossFileTypeReference> resource in _resources.Value)
        {
            string[] typeAndVersion = resource.Key.Split('@');
            string fqResourceType = typeAndVersion[0];
            string apiVersion = typeAndVersion[1];
            string provider = fqResourceType.Split("/").First();
            string providerKey = provider.ToLowerInvariant();
            string resourceTypeKey = fqResourceType.ToLowerInvariant();

            if (!providers.TryGetValue(providerKey, out ProviderResourceTypes? providerResourceTypes))
            {
                providerResourceTypes = new ProviderResourceTypes(provider);
                providers[providerKey] = providerResourceTypes;
            }

            if (!providerResourceTypes.ResourceTypes.TryGetValue(resourceTypeKey, out UniqueResourceType? uniqueResourceType))
            {
                uniqueResourceType = new UniqueResourceType(fqResourceType);
                providerResourceTypes.ResourceTypes[resourceTypeKey] = uniqueResourceType;
            }

            uniqueResourceType.ApiVersions.Add(apiVersion);
        }

        return providers;
    }

    public UniqueResourceType? FindResourceTypeAndVersions(string resourceTypeName)
    {
        (string providerName, _, _) = ResourceParser.ParseResourceType(resourceTypeName);
        if (GetAllResourceTypesAndVersionsByProvider().TryGetValue(providerName, out ProviderResourceTypes? provider)
            && provider.ResourceTypes.TryGetValue(resourceTypeName.ToLowerInvariant(), out UniqueResourceType? uniqueResourceType))
        {
            return uniqueResourceType;
        }
        else
        {
            return null;
        }
    }

    public string[] GetResourceApiVersions(string resourceTypeName)
    {
        (string providerName, _, _) = ResourceParser.ParseResourceType(resourceTypeName);

        if (!GetAllResourceTypesAndVersionsByProvider().TryGetValue(providerName, out ProviderResourceTypes? provider))
        {
            throw new Exception($"Resource type {resourceTypeName} not found.");
        }

        return [.. provider.ResourceTypes[resourceTypeName.ToLowerInvariant()].ApiVersions];
    }

    public TypesDefinitionResult LoadSingleResource(string resourceTypeName, string apiVersion)
    {
        (string provider, _, _) = ResourceParser.ParseResourceType(resourceTypeName);

        var typesToWrite = new List<TypeBase>();

        string fullResourceTypeName = $"{resourceTypeName}@{apiVersion}";
        if (!_resources.Value.TryGetValue(fullResourceTypeName, out CrossFileTypeReference? resource))
        {
            // ITypeLoader.LoadResourceType() doesn't tell us whether the type name of apiVersion is incorrect, so
            //   figure it out here.
            if (FindResourceTypeAndVersions(resourceTypeName) is UniqueResourceType uniqueResourceType)
            {
                throw new InvalidDataException(
                    $"Resource type {resourceTypeName} does not have an apiVersion \"{apiVersion}\". "
                    + $"Available versions are: {uniqueResourceType.ApiVersions.JoinWithComma()}");
            }
            else
            {
                throw new InvalidDataException($"Resource type {resourceTypeName} not found.");
            }
        }

        var selectedResourceFunctions = _resourceFunctions.Value.Where(r => r.Key.ContainsOrdinalInsensitively(provider)).ToList();

        var result = new TypesDefinitionResult
        {
            ResourceProvider = provider,
            ApiVersion = apiVersion
        };
        try
        {
            ResourceType resourceType = _azTypeLoader.LoadResourceType(resource);

            FindTypesToWrite(typesToWrite, resourceType.Body);

            if (WriteComplexType(resourceType) is ResourceTypeEntity resourceTypeEntity)
            {
                result.ResourceTypeEntities.Add(resourceTypeEntity);
            }
            else
            {
                throw new InvalidDataException($"Resource type {resourceType.Name} failed to be converted to ResourceTypeEntity.");
            }

            foreach (KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyList<CrossFileTypeReference>>> resourceFunction in selectedResourceFunctions)
            {
                var functions = resourceFunction.Value.Where(r => r.Key.Equals(apiVersion)).SelectMany(r => r.Value).ToList();

                foreach (CrossFileTypeReference? function in functions)
                {
                    ResourceFunctionType resourceFunctionType = _azTypeLoader.LoadResourceFunctionType(function);
                    if (resourceFunctionType.Input != null)
                    {
                        typesToWrite.Add(resourceFunctionType.Input.Type);
                        FindTypesToWrite(typesToWrite, resourceFunctionType.Input);
                    }

                    typesToWrite.Add(resourceFunctionType.Output.Type);
                    FindTypesToWrite(typesToWrite, resourceFunctionType.Output);

                    if (WriteComplexType(resourceFunctionType) is ResourceFunctionTypeEntity resourceFunctionTypeEntity)
                    {
                        result.ResourceFunctionTypeEntities.Add(resourceFunctionTypeEntity);
                    }
                    else
                    {
                        throw new InvalidDataException($"Resource function type {resourceFunctionType.Name} failed to be converted to ResourceFunctionTypeEntity.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Resource type {resourceTypeName} with apiVersion {apiVersion} not supported", ex);
        }


        // Sort by name first (e.g. listSecrets), then by resource type (e.g. Microsoft.ApiManagement/service/authorizationServers)
        result.ResourceFunctionTypeEntities.Sort((a, b) =>
        {
            int nameComparison = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            return nameComparison != 0 ? nameComparison : string.Compare(a.ResourceType, b.ResourceType, StringComparison.OrdinalIgnoreCase);
        });

        foreach (TypeBase type in typesToWrite)
        {
            if (IsComplexType(type))
            {
                result.OtherComplexTypeEntities.Add(WriteComplexType(type));
            }
        }

        // Note(ligar): Dedupe here because OtherComplexTypeEntities can contain duplicates. This is because instances of the same type (TypeBase) can have different hash codes (Refer to ProcessTypeLinks() method).
        result.OtherComplexTypeEntities = [.. result.OtherComplexTypeEntities
            .GroupBy(e => e.Name)
            .Select(g => g.First())];

        result.OtherComplexTypeEntities.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

        return result;
    }

    // This finds all the types the referenced type depends on, and adds them to the typesToWrite list.
    private void FindTypesToWrite(List<TypeBase> typesToWrite, ITypeReference typeReference)
    {
        switch (typeReference.Type)
        {
            case ArrayType arrayType:
                ProcessTypeLinks(typesToWrite, arrayType.ItemType, false);
                break;
            case ObjectType objectType:
                foreach (KeyValuePair<string, ObjectTypeProperty> property in objectType.Properties.OrderByAscendingOrdinalInsensitively(kvp => kvp.Key))
                {
                    ProcessTypeLinks(typesToWrite, property.Value.Type, false);
                }
                if (objectType.AdditionalProperties != null)
                {
                    ProcessTypeLinks(typesToWrite, objectType.AdditionalProperties, false);
                }
                break;
            case DiscriminatedObjectType discriminatedObjectType:
                foreach (KeyValuePair<string, ITypeReference> property in discriminatedObjectType.Elements.OrderByAscendingOrdinalInsensitively(kvp => kvp.Key))
                {
                    // Don't display discriminated object elements as individual types
                    ProcessTypeLinks(typesToWrite, property.Value, true);
                }
                break;
            default:
                // In this method, we don't care about simple types such as IntegerType
                break;

        }
    }

    private void ProcessTypeLinks(List<TypeBase> typesToWrite, ITypeReference typeReference, bool skipParent)
    {
        if (!typesToWrite.Contains(typeReference.Type))
        {
            if (!skipParent)
            {
                typesToWrite.Add(typeReference.Type);
            }

            FindTypesToWrite(typesToWrite, typeReference);
        }
    }

    private ComplexType WriteComplexType(TypeBase typeBase)
    {
        switch (typeBase)
        {
            case ResourceType resourceType:
                var rtEntity = new ResourceTypeEntity
                {
                    Name = resourceType.Name,
                    BodyType = WriteComplexType(resourceType.Body.Type),
                    WritableScopes = resourceType.WritableScopes.ToString(),
                    ReadableScopes = resourceType.ReadableScopes.ToString(),
                };
                return rtEntity;
            case ResourceFunctionType resourceFunctionType:
                var rftEntity = new ResourceFunctionTypeEntity
                {
                    Name = resourceFunctionType.Name,
                    ResourceType = resourceFunctionType.ResourceType,
                    ApiVersion = resourceFunctionType.ApiVersion,
                    InputType = resourceFunctionType.Input != null ? GetTypeName(resourceFunctionType.Input.Type) : null,
                    OutputType = GetTypeName(resourceFunctionType.Output.Type)
                };
                return rftEntity;
            case ObjectType objectType:
                var otEntity = new ObjectTypeEntity
                {
                    Name = objectType.Name,
                    Sensitive = objectType.Sensitive,
                    AdditionalPropertiesType = objectType.AdditionalProperties != null ? GetTypeName(objectType.AdditionalProperties.Type) : null
                };
                foreach (KeyValuePair<string, ObjectTypeProperty> property in objectType.Properties.OrderByAscendingOrdinalInsensitively(kvp => kvp.Key))
                {
                    otEntity.Properties.Add(WriteTypeProperty(property.Key, property.Value));
                }
                return otEntity;
            case DiscriminatedObjectType discriminatedObjectType:
                var dotEntity = new DiscriminatedObjectTypeEntity
                {
                    Name = discriminatedObjectType.Name,
                    Discriminator = discriminatedObjectType.Discriminator
                };
                foreach (KeyValuePair<string, ObjectTypeProperty> baseProperty in discriminatedObjectType.BaseProperties.OrderByAscendingOrdinalInsensitively(kvp => kvp.Key))
                {
                    dotEntity.BaseProperties.Add(WriteTypeProperty(baseProperty.Key, baseProperty.Value));
                }
                foreach (KeyValuePair<string, ITypeReference> element in discriminatedObjectType.Elements.OrderByAscendingOrdinalInsensitively(kvp => kvp.Key))
                {
                    dotEntity.Elements.Add(WriteComplexType(element.Value.Type));
                }

                return dotEntity;
            default:
                throw new InvalidDataException("Unexpected type");
        }
    }

    private PropertyInfo WriteTypeProperty(string propertyName, ObjectTypeProperty property)
    {
        return new PropertyInfo(
            propertyName,
            GetTypeName(property.Type.Type),
            property.Description,
            GetFlags(property.Flags),
            GetModifiers(property.Type.Type));
    }

    private string GetTypeName(TypeBase typeBase)
    {
        return typeBase switch
        {
            ResourceType resourceType => resourceType.Name,
            ResourceFunctionType resourceFunctionType => $"{resourceFunctionType.Name} ({resourceFunctionType.ResourceType}@{resourceFunctionType.ApiVersion})",
            ObjectType objectType => objectType.Name,
            DiscriminatedObjectType discriminatedObjectType => discriminatedObjectType.Name,
            ArrayType arrayType => $"{GetTypeName(arrayType.ItemType.Type)}[]",
            UnionType unionType => string.Join(" | ", unionType.Elements.Select(e => GetTypeName(e.Type))),
            AnyType anyType => "any",
            NullType nullType => "null",
            BooleanType booleanType => "bool",
            IntegerType integerType => "int",
            StringType stringType => "string",
            StringLiteralType stringLiteralType => stringLiteralType.Value,
            BuiltInType builtInType => builtInType.Kind.ToString().ToLower(),
            _ => throw new InvalidDataException("Unrecognized type"),
        };
    }

    private string? GetFlags(ObjectTypePropertyFlags flags)
    {
        return flags == ObjectTypePropertyFlags.None ? null : flags.ToString();
    }

    private string? GetModifiers(TypeBase typeBase)
    {
        return typeBase switch
        {
            IntegerType integerType => GetIntegerModifiers(integerType),
            StringType stringType => GetStringModifiers(stringType),
            _ => null
        };
    }

    private string GetIntegerModifiers(IntegerType integerType)
    {
        return FormatModifiers(
            integerType.MinValue != null ? $"minValue: {integerType.MinValue}" : null,
            integerType.MaxValue != null ? $"maxValue: {integerType.MaxValue}" : null);
    }

    private string GetStringModifiers(StringType stringType)
    {
        return FormatModifiers(
            stringType.Sensitive == true ? "sensitive" : null,
            stringType.MinLength != null ? $"minLength: {stringType.MinLength}" : null,
            stringType.MaxLength != null ? $"maxLength: {stringType.MaxLength}" : null,
            stringType.Pattern != null ? $"pattern: {stringType.Pattern}" : null);
    }

    private string FormatModifiers(params string?[] modifiers)
    {
        string modifiersString = string.Join(", ", modifiers.Where(m => !string.IsNullOrEmpty(m)));
        return string.IsNullOrEmpty(modifiersString) ? string.Empty : modifiersString;
    }

    private bool IsComplexType(TypeBase typeBase)
    {
        return typeBase switch
        {
            ResourceType => true,
            ResourceFunctionType => true,
            ObjectType => true,
            DiscriminatedObjectType => true,
            _ => false,
        };
    }
}
