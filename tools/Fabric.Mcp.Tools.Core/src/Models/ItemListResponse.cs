// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>One page returned by the Fabric List Items REST API.</summary>
/// <param name="Value">The items in this page.</param>
/// <param name="ContinuationToken">The optional token for the next page.</param>
/// <param name="ContinuationUri">The optional next-page URI, returned as metadata only.</param>
public sealed record ItemListResponse(
    List<FabricItemSummary> Value,
    string? ContinuationToken = null,
    string? ContinuationUri = null);
