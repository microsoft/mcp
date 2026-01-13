// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmSizeInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("numberOfCores")] int? NumberOfCores,
    [property: JsonPropertyName("memoryInMB")] int? MemoryInMB,
    [property: JsonPropertyName("maxDataDiskCount")] int? MaxDataDiskCount,
    [property: JsonPropertyName("osDiskSizeInMB")] int? OsDiskSizeInMB,
    [property: JsonPropertyName("resourceDiskSizeInMB")] int? ResourceDiskSizeInMB);
