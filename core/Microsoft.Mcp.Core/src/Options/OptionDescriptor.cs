// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Options;

public class OptionDescriptor
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool Required { get; init; }
    public bool Hidden { get; init; }
    public required PropertyInfo TargetProperty { get; init; }
    public required Type Type { get; init; }
    public PropertyInfo? ParentProperty { get; init; }

    public static OptionDescriptor[] FromType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() where T : class
    {
        List<OptionDescriptor> optionDescriptors = [];
        NullabilityInfoContext nullabilityContext = new();
        CollectDescriptors(typeof(T), prefix: null, optionDescriptors, nullabilityContext);
        return [.. optionDescriptors];
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:UnrecognizedReflectionPattern",
        Justification = "Nested option types are rooted by the application.")]
    private static void CollectDescriptors(Type type, string? prefix, List<OptionDescriptor> descriptors, NullabilityInfoContext nullabilityContext, bool parentOptional = false, PropertyInfo? parentProperty = null)
    {
        PropertyInfo[] allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // When a derived class hides a base property with 'new', GetProperties returns both.
        // Keep only the most-derived version (the one whose DeclaringType is closest to 'type').
        Dictionary<string, PropertyInfo> deduped = new(StringComparer.Ordinal);
        foreach (PropertyInfo p in allProperties)
        {
            if (!deduped.TryGetValue(p.Name, out PropertyInfo? existing) ||
                (existing.DeclaringType is not null && p.DeclaringType is not null && existing.DeclaringType.IsAssignableFrom(p.DeclaringType)))
            {
                deduped[p.Name] = p;
            }
        }

        foreach (PropertyInfo property in deduped.Values)
        {
            // Skip read-only properties (no setter) — they can't be bound from command-line args
            if (!property.CanWrite)
            {
                continue;
            }

            OptionAttribute? optionAttribute = property.GetCustomAttribute<OptionAttribute>();
            string name = optionAttribute?.Name ?? OptionNameConvention.ToKebabCase(property.Name);

            if (!string.IsNullOrEmpty(prefix))
            {
                name = $"{prefix}-{name}";
            }

            if (IsComplexType(property.PropertyType))
            {
                bool isOptionalGroup = IsNullable(property, nullabilityContext);

                // Flatten nested complex types with a prefix.
                CollectDescriptors(property.PropertyType, name, descriptors, nullabilityContext, parentOptional: isOptionalGroup, parentProperty: property);
            }
            else
            {
                bool isNullable = IsNullable(property, nullabilityContext);

                if (parentOptional && !isNullable)
                {
                    throw new InvalidOperationException(
                        $"Optional group contains required member '{property.Name}'. " +
                        "All properties within a nullable complex type must be nullable. " +
                        "Either make the parent property required or make all child properties nullable.");
                }


                descriptors.Add(new OptionDescriptor
                {
                    Name = name,
                    Description = optionAttribute?.Description,
                    Type = property.PropertyType,
                    Required = !isNullable,
                    Hidden = optionAttribute?.Hidden ?? false,
                    TargetProperty = property,
                    ParentProperty = parentProperty
                });
            }
        }
    }

    private static bool IsComplexType(Type type)
    {
        // Unwrap Nullable<T>
        Type underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (IsScalarType(underlying))
        {
            return false;
        }

        // Arrays and collections are leaf types, but only when their element type is scalar
        // String is an IEnumerable<char>, but we treat it as a scalar type, so it will be handled above.
        if (underlying.IsArray || underlying.IsAssignableTo(typeof(System.Collections.IEnumerable)))
        {
            Type? elementType = GetCollectionElementType(underlying);
            if (elementType is not null && !IsScalarType(elementType))
            {
                throw new InvalidOperationException(
                    $"Collection property with non-scalar element type '{elementType.Name}' is not supported. " +
                    "Only collections of scalar types (string, int, enum, etc.) can be used as command options.");
            }
            return false;
        }

        // Everything else (classes, complex structs) is considered nested/complex
        return true;
    }

    private static bool IsScalarType(Type type)
    {
        Type underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsPrimitive ||
               underlying.IsEnum ||
               underlying == typeof(string) ||
               underlying == typeof(decimal) ||
               underlying == typeof(DateTime) ||
               underlying == typeof(DateTimeOffset) ||
               underlying == typeof(TimeSpan) ||
               underlying == typeof(Guid);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:UnrecognizedReflectionPattern",
        Justification = "Collection types used in option properties are rooted by the application.")]
    private static Type? GetCollectionElementType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        // Look for IEnumerable<T>
        return type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(i => i.GetGenericArguments()[0])
            .FirstOrDefault();
    }

    private static bool IsNullable(PropertyInfo property, NullabilityInfoContext nullabilityContext)
    {
        // Nullable value types (e.g., TimeSpan?)
        if (Nullable.GetUnderlyingType(property.PropertyType) is not null)
        {
            return true;
        }

        // Nullable reference types (e.g., string?)
        NullabilityInfo nullabilityInfo = nullabilityContext.Create(property);
        return nullabilityInfo.WriteState == NullabilityState.Nullable;
    }
}
