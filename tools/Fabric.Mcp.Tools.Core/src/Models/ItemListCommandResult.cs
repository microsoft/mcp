// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>One page of workspace or folder item metadata.</summary>
/// <param name="Items">The items in this page; an empty array is a valid page.</param>
/// <param name="ContinuationToken">The optional token to pass to the next invocation.</param>
/// <param name="ContinuationUri">The optional next-page URI, which this tool never follows.</param>
public sealed record ItemListCommandResult(
    List<FabricItemSummary> Items,
    string? ContinuationToken,
    string? ContinuationUri);
