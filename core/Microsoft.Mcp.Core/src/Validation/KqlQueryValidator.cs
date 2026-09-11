// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Validation;

/// <summary>
/// Validates user-supplied KQL queries for structural validity.
/// Authorization and permission enforcement are handled by Azure RBAC and the target service.
/// </summary>
public static class KqlQueryValidator
{
    private const int MaxQueryLength = 10000;

    /// <summary>
    /// Validates the KQL query for structural validity. Throws <see cref="CommandValidationException"/>
    /// when the query is empty or exceeds the maximum length.
    /// </summary>
    public static void ValidateQuerySafety(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new CommandValidationException("Query cannot be empty.");
        }

        if (query.Length > MaxQueryLength)
        {
            throw new CommandValidationException(
                $"Query length exceeds the maximum allowed limit of {MaxQueryLength:N0} characters.");
        }
    }
}
