// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Services.Azure;

/// <summary>
/// Provides source-generated JSON metadata for Resource Graph continuation payloads.
/// </summary>
/// <remarks>
/// Serialization is compatible with native AOT, and deserialization rejects unknown payload members.
/// </remarks>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(ResourceGraphContinuationToken))]
internal sealed partial class ResourceGraphPagingJsonContext : JsonSerializerContext;
