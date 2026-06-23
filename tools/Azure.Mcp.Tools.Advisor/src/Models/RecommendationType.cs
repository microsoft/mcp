// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Catalog entry for an Azure Advisor recommendation type. Sourced from the
/// `recommendationType` entity's `supportedValues[]` in the Advisor metadata API,
/// which surfaces the linkage between a recommendation type and its category,
/// impact, and the resource type it applies to.
///
/// Useful both for greenfield environments (no actual recommendations exist yet)
/// and for brownfield environments where new resource types are being onboarded
/// and the caller wants to know what Advisor would recommend for them.
///
/// Property names and null-handling are governed by JsonSourceGenerationOptions on
/// AdvisorJsonContext (camelCase naming + WhenWritingNull default).
/// </summary>
public record RecommendationType(
    string Id,
    string DisplayName,
    string? Category,
    string? Impact,
    string? ResourceType,
    string? SubCategory
);
