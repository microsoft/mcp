// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Jobs;

internal static class RecoveryJobValidation
{
    public static void ValidateName(string recoveryJob, ValidationResult validationResult)
    {
        if (!Guid.TryParseExact(recoveryJob, "D", out _))
        {
            validationResult.Errors.Add("The recovery job name must be a GUID in D format, such as 11111111-1111-1111-1111-111111111111.");
        }
    }
}
