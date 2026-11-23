// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

internal readonly record struct UnusedAgeObservation(
    DateTimeOffset? LastActivityAt = null,
    int? QuietWindowDays = null,
    bool IsIncomplete = false)
{
    internal bool IsUnknown => IsIncomplete || (LastActivityAt is null && QuietWindowDays is null);
}
