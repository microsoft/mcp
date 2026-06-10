// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Catalog entry returned from the Advisor metadata API. Mirrors the
/// MetadataSupportedValueDetail definition in the REST API (id + displayName).
/// Property names and null-handling are governed by JsonSourceGenerationOptions on
/// AdvisorJsonContext (camelCase naming + WhenWritingNull default).
/// </summary>
public record RecommendationType(
    string Id,
    string DisplayName
);
