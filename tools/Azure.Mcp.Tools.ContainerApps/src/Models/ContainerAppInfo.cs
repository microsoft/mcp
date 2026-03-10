// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ContainerApps.Models;

public sealed record ContainerAppInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("managedEnvironmentId")] string? ManagedEnvironmentId,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState);
