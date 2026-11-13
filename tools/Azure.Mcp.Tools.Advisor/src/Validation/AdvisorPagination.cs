// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Validation;

internal static class AdvisorPagination
{
    internal const string Description =
        "Continuation token returned by the preceding page. Pass it unchanged with the same scope, tenant, filters, ordering and page size. Omit for the first page.";
    internal const string Error = "--continuation-token must be a non-empty token of at most 16384 characters without control characters.";

    internal static bool IsValid(string? token) =>
        token is null || token.Length is > 0 and <= 16384 && !token.Any(char.IsControl) && !string.IsNullOrWhiteSpace(token);

    internal static void Validate(string? token)
    {
        if (!IsValid(token))
        {
            throw new ArgumentException(Error);
        }
    }
}
