// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Caller identity resolver for HTTP On-Behalf-Of mode.
/// Extracts tenant and principal identity from the authenticated user's claims.
/// </summary>
public sealed class HttpOboCallerIdentityResolver(IHttpContextAccessor httpContextAccessor) : ICallerIdentityResolver
{
    public ValueTask<CallerBinding> ResolveAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.User.Identity?.IsAuthenticated != true)
        {
            throw new InvalidOperationException(
                "Cannot resolve OBO caller binding: the current HTTP request is not authenticated.");
        }

        var tenantId = httpContext.User.FindFirst("tid")?.Value;
        var principalId = httpContext.User.FindFirst("oid")?.Value
                       ?? httpContext.User.FindFirst("sub")?.Value;

        string? principalIdHash = null;
        if (principalId is not null)
        {
            principalIdHash = HashPrincipalId(principalId);
        }

        var binding = new CallerBinding
        {
            Mode = "obo",
            TenantId = tenantId,
            PrincipalIdHash = principalIdHash,
        };

        return new ValueTask<CallerBinding>(binding);
    }

    internal static string HashPrincipalId(string principalId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(principalId));
        return string.Concat("sha256:", Convert.ToHexStringLower(hash));
    }
}
