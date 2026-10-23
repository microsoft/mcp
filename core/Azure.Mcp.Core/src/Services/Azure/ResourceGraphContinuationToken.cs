// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Azure;

/// <summary>
/// Stores the versioned Resource Graph continuation payload needed to resume on another server instance.
/// </summary>
/// <param name="Version">The payload format version checked before continuation is accepted.</param>
/// <param name="PageSize">The page size that must be preserved throughout the iteration.</param>
/// <param name="CloudEndpoint">The configured ARM endpoint identifying the Azure cloud.</param>
/// <param name="Subscription">The resolved subscription ID used by the resource query.</param>
/// <param name="Tenant">The resolved tenant ID used to authenticate the resource query.</param>
/// <param name="Query">The ordered KQL query, including the resource type and filters.</param>
/// <param name="SkipToken">The opaque next-page token returned by Azure Resource Graph.</param>
/// <remarks>
/// The serialized payload is encoded, not encrypted or signed, and is not an authorization credential.
/// The service verifies its version and request scope and authenticates each page independently.
/// </remarks>
internal sealed record ResourceGraphContinuationToken(
    int Version,
    int PageSize,
    string CloudEndpoint,
    string Subscription,
    string Tenant,
    string Query,
    string SkipToken);
