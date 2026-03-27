// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Commands.Descriptors;

[JsonSerializable(typeof(CommandDescriptor))]
[JsonSerializable(typeof(CommandDescriptor[]))]
[JsonSerializable(typeof(CommandGroupDescriptor))]
[JsonSerializable(typeof(CommandGroupDescriptor[]))]
[JsonSerializable(typeof(OptionDescriptor))]
[JsonSerializable(typeof(OptionDescriptor[]))]
[JsonSerializable(typeof(ToolAnnotations))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true
)]
public sealed partial class DescriptorJsonContext : JsonSerializerContext
{
}
