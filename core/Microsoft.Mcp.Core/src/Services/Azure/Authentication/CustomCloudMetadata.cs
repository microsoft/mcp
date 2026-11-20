// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Metadata required to connect to a custom Azure cloud.
/// </summary>
public sealed record CustomCloudMetadata(
    string? AuthorityHost,
    CustomCloudServiceMetadata? Arm,
    CustomCloudServiceMetadata? LogAnalytics,
    CustomCloudKustoMetadata? Kusto);
