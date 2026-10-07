// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class CatalogSearchInputValidator
{
    internal static void Validate(
        string? search,
        string? filter,
        int? pageSize,
        string? continuationToken,
        ValidationResult validationResult)
    {
        if (pageSize is < 1 or > 1000)
        {
            validationResult.Errors.Add("Page size must be between 1 and 1000.");
        }

        if (continuationToken is not null && (search is not null || filter is not null))
        {
            validationResult.Errors.Add("--continuation-token must not be combined with --search or --filter. Use the returned token without search or filter.");
        }
    }
}
