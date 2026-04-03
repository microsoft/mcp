// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Caller identity resolver for stdio and hosting-environment-identity modes.
/// Returns a static binding with no per-user differentiation.
/// </summary>
public sealed class HostIdentityCallerIdentityResolver : ICallerIdentityResolver
{
    private static readonly CallerBinding s_binding = new() { Mode = "hostIdentity" };

    public ValueTask<CallerBinding> ResolveAsync(CancellationToken cancellationToken = default) =>
        new(s_binding);
}
