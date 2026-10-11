// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands;

internal static class ResilienceManagementOperationIdValidation
{
    public static void Validate(string? operationId, ValidationResult validationResult)
    {
        if (operationId is not null && !Guid.TryParseExact(operationId, "D", out _))
        {
            validationResult.Errors.Add("The operation ID must be a GUID in canonical D format, such as 11111111-1111-1111-1111-111111111111.");
        }
    }

    public static string? Normalize(string? operationId) =>
        operationId is null ? null : Guid.ParseExact(operationId, "D").ToString("D");
}
