// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Services;

public sealed record FabricOperationWaitOptions(
    bool Sync = false, double MaxWaitSeconds = FabricOperationWaitOptions.DefaultMaxWaitSeconds, bool EarlyPoll = true)
{
    public const double DefaultMaxWaitSeconds = 120;
    public const double MaximumWaitSeconds = 600;

    public static bool IsValidBudget(double seconds) =>
        double.IsFinite(seconds) && seconds > 0 && seconds <= MaximumWaitSeconds;

    internal void Validate()
    {
        if (!IsValidBudget(MaxWaitSeconds))
        {
            throw new ArgumentException(
                FormattableString.Invariant($"The maximum wait must be finite, greater than zero, and at most {MaximumWaitSeconds} seconds."),
                nameof(MaxWaitSeconds));
        }
    }
}
