// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Models.Paging;

/// <summary>
/// Carries the requested page size and continuation from a command to its service.
/// </summary>
/// <param name="PageSize">
/// The maximum number of items requested per page, or <see langword="null"/> to use the service
/// default or retain the previous page size when continuing.
/// </param>
/// <param name="ContinuationToken">
/// The opaque token from a previous page, or <see langword="null"/> to start a new iteration.
/// </param>
/// <remarks>
/// This request contains no connection-local state. Services validate the token against the
/// current request scope and authenticate each page independently.
/// </remarks>
public sealed record PageRequest(int? PageSize = null, string? ContinuationToken = null)
{
    /// <summary>
    /// The default number of items requested on the first page when no size is supplied.
    /// </summary>
    public const int DefaultPageSize = 50;

    /// <summary>
    /// The largest supported number of items requested per page.
    /// </summary>
    public const int MaxPageSize = 1000;

    /// <summary>
    /// The maximum supported continuation-token length in characters.
    /// </summary>
    public const int MaxContinuationTokenLength = 65536;
}
