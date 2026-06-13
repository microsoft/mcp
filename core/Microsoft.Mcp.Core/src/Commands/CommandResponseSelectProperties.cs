// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Mcp.Core.Commands;

public sealed class CommandResponseSelectProperties(string[]? selectedFields, IJsonTypeInfoResolver originalResolver, Type typeToTrim) : IJsonTypeInfoResolver
{
    private readonly IJsonTypeInfoResolver _trimmingResolver = originalResolver.WithAddedModifier(typeInfo =>
        {
            if (typeInfo.Type == typeToTrim)
            {
                foreach (var property in typeInfo.Properties)
                {
                    property.ShouldSerialize = (obj, value) =>
                        selectedFields != null && selectedFields.Any(field => property.Name.Equals(field, StringComparison.OrdinalIgnoreCase));
                }
            }
        });

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) => _trimmingResolver.GetTypeInfo(type, options);
}
