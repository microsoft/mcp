// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// Identifies flat command options that support bounded result pages and continuation.
/// </summary>
/// <remarks>
/// Implementing option classes declare these properties with <see cref="OptionAttribute"/> so they
/// are exposed to callers. The command base validates their bounds; services implement continuation.
/// </remarks>
public interface IPagingOption
{
    /// <summary>
    /// Gets or sets the maximum number of items requested per page, or <see langword="null"/> to
    /// use the service default or retain the size associated with a continuation.
    /// </summary>
    int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets the opaque token returned by a previous page, or <see langword="null"/> to
    /// start a new iteration.
    /// </summary>
    string? ContinuationToken { get; set; }
}
