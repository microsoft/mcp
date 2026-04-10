// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Models;

/// <summary>
/// Result of a Migrate Project operation.
/// </summary>
public sealed record MigrateProjectResult(
    bool HasData,
    string? Id,
    string? Name,
    string? Type,
    string? Location,
    IDictionary<string, object>? Properties);
