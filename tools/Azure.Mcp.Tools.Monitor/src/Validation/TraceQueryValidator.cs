// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Monitor.Validation;

internal static class TraceQueryValidator
{
    public static void ValidateWindow(DateTimeOffset start, DateTimeOffset end, ValidationResult result)
    {
        if (start >= end || end - start > TimeSpan.FromDays(1))
        {
            result.AddError("The time range must be positive and no longer than one day.", "Invalid time range.");
        }
    }
}
