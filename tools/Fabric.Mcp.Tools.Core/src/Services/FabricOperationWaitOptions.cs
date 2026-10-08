// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Services;

public sealed record FabricOperationWaitOptions(
    bool Sync = false, double MaxWaitSeconds = FabricOperationWaitOptions.DefaultMaxWaitSeconds, bool EarlyPoll = true)
{
    public const double DefaultMaxWaitSeconds = 120;

    public static bool IsValidBudget(double seconds) => double.IsFinite(seconds) && seconds > 0;

    internal void Validate()
    {
        if (!IsValidBudget(MaxWaitSeconds))
        {
            throw new ArgumentException("The maximum wait must be a finite positive number of seconds.", nameof(MaxWaitSeconds));
        }
    }
}
