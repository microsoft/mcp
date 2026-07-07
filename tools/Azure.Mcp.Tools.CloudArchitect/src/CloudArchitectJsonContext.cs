// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.CloudArchitect.Models;
using Azure.Mcp.Tools.CloudArchitect.Options;

namespace Azure.Mcp.Tools.CloudArchitect;

[JsonSerializable(typeof(CloudArchitectResponseObject))]
[JsonSerializable(typeof(CloudArchitectDesignResponse))]
[JsonSerializable(typeof(ArchitectureDesignToolState))]
[JsonSerializable(typeof(ArchitectureDesignTiers))]
[JsonSerializable(typeof(ArchitectureDesignRequirements))]
[JsonSerializable(typeof(ArchitectureDesignRequirement))]
[JsonSerializable(typeof(ArchitectureDesignConfidenceFactors))]
[JsonSerializable(typeof(RequirementImportance))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true)]
public partial class CloudArchitectJsonContext : JsonSerializerContext
{
}
