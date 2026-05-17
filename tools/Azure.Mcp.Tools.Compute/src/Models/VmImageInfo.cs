// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

/// <summary>
/// Describes a VM image candidate — a built-in alias, a marketplace URN, or a shared-gallery reference.
/// </summary>
public sealed record VmImageInfo(
    [property: JsonPropertyName("alias")] string? Alias,
    [property: JsonPropertyName("publisher")] string? Publisher,
    [property: JsonPropertyName("offer")] string? Offer,
    [property: JsonPropertyName("sku")] string? Sku,
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("urn")] string? Urn,
    [property: JsonPropertyName("osType")] string? OsType,
    [property: JsonPropertyName("hyperVGeneration")] string? HyperVGeneration,
    [property: JsonPropertyName("source")] string? Source);
