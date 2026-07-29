// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Mcp.Core.Options;

public class OptionDescriptor
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string[] Aliases { get; init; }
    public bool Required { get; init; }
    public bool Hidden { get; init; }
    public object? DefaultValue { get; init; }
    public bool AllowEmptyOrWhiteSpaceString { get; init; }
    public required PropertyInfo TargetProperty { get; init; }
    public required Type Type { get; init; }
    public PropertyInfo? ParentProperty { get; init; }

    public static OptionDescriptor[] FromType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() where T : class
    {
        List<OptionDescriptor> optionDescriptors = [];
        CollectDescriptors(typeof(T), null, optionDescriptors, new(), true, null);
        return [.. optionDescriptors];
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070:UnrecognizedReflectionPattern",
        Justification = "Nested option types are rooted by the application.")]
    private static void CollectDescriptors(
        Type type,
        string? prefix,
        List<OptionDescriptor> descriptors,
        NullabilityInfoContext nullabilityContext,
        bool parentRequired,
        PropertyInfo? parentProperty)
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

            var optionAttribute = property.GetCustomAttribute<OptionAttribute>();
            var optionContainerAttribute = property.GetCustomAttribute<OptionContainerAttribute>();
            // Only include properties with [Option] or [OptionContainer]
            if (optionAttribute == null && optionContainerAttribute == null)
            {
                continue;
            }

            if (optionAttribute != null && optionContainerAttribute != null)
            {
                throw new InvalidOperationException("Properties can only be attributed with [Option] or [OptionContainer], not both.");
            }

            var required = Attribute.IsDefined(property, typeof(RequiredMemberAttribute));
            var complex = IsComplexType(property.PropertyType);
            if (optionAttribute != null)
            {
                if (complex)
                {
                    throw new InvalidOperationException("Complex properties cannot use [Option] attribute. Use [OptionContainer] instead.");
                }
                if (!parentRequired && required)
                {
                    throw new InvalidOperationException(
                        $"Optional group contains required member '{property.Name}'. " +
                        "All properties within a non-required complex type must be non-required. " +
                        "Either make the parent property required or make all child properties non-required.");
                }


                descriptors.Add(new OptionDescriptor
                {
                    Name = GetNameOrPrefix(optionAttribute.Name, prefix, property.Name),
                    Aliases = optionAttribute.Aliases ?? [],
                    Description = optionAttribute.Description,
                    Type = property.PropertyType,
                    Required = required,
                    Hidden = optionAttribute.Hidden,
                    DefaultValue = optionAttribute.DefaultValue,
                    AllowEmptyOrWhiteSpaceString = optionAttribute.AllowEmptyOrWhiteSpaceString,
                    TargetProperty = property,
                    ParentProperty = parentProperty
                });
            }

            if (optionContainerAttribute != null)
            {
                if (!complex)
                {
                    throw new InvalidOperationException("Non-complex properties cannot use [OptionContainer] attribute. Use [Option] instead.");
                }
                // Flatten nested complex types with a prefix.
                CollectDescriptors(property.PropertyType, GetNameOrPrefix(optionContainerAttribute.Prefix, prefix, property.Name), descriptors, nullabilityContext, required, property);
            }
        }
    }

    private static string GetNameOrPrefix(string? attributeNameOrPrefix, string? prefix, string propertyName)
    {
        string name = attributeNameOrPrefix ?? OptionNameConvention.ToKebabCase(propertyName);
        if (!string.IsNullOrEmpty(prefix))
        {
            name = $"{prefix}-{name}";
        }
        return name;
    }

    private static bool IsComplexType(Type type)
    {
        // Unwrap Nullable<T>
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (IsScalarType(underlying))
        {
            return false;
        }

        // Arrays and collections are leaf types, but only when their element type is scalar
        // String is an IEnumerable<char>, but we treat it as a scalar type, so it will be handled above.
        if (underlying.IsArray || underlying.IsAssignableTo(typeof(System.Collections.IEnumerable)))
        {
            var elementType = GetCollectionElementType(underlying);
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
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
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
}
