// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Resolves the caller identity for the current request context.
/// The implementation varies by transport mode (stdio vs. HTTP).
/// </summary>
public interface ICallerIdentityResolver
{
    /// <summary>
    /// Resolves the <see cref="CallerBinding"/> for the current caller.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The resolved caller binding.</returns>
    ValueTask<CallerBinding> ResolveAsync(CancellationToken cancellationToken = default);
}
