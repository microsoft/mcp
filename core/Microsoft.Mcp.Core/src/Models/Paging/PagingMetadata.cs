// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Models.Paging;

/// <summary>
/// Describes a result page and, when available, the invocation needed to retrieve its successor.
/// </summary>
/// <param name="PageSize">The maximum number of items requested for the page, not the returned item count.</param>
/// <param name="ContinuationToken">
/// The opaque next-page token, or <see langword="null"/> when the iteration is complete.
/// </param>
/// <param name="CommandId">
/// The stable identity of the logical command to invoke, independent of transport or routing aliases.
/// </param>
/// <param name="ContinuationArguments">
/// The complete command arguments to replay unchanged on this or another compatible server instance,
/// including resolved scope, page size, and continuation token.
/// </param>
/// <remarks>
/// An empty page can still have continuation. Tokens and arguments do not grant access; the
/// receiving instance must authenticate and authorize the request independently.
/// </remarks>
public sealed record PagingMetadata(
    int PageSize,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ContinuationToken = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? CommandId = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] JsonObject? ContinuationArguments = null);
