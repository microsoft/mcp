// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectedItemDppDataSourceReference(
    string? ResourceId,
    string? ResourceName,
    string? DataSourceType,
    string? ResourceType,
    string? ResourceLocation,
    string? ObjectType,
    string? ResourceUriString,
    string? ResourceProperties);
