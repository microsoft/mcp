// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands;

/// <summary>
/// POC: how long one wait tool call keeps waiting. One long call makes the host show a single wait step rather than
/// "(N times)". Set the AZURE_MIGRATE_WAIT_MAX_SECONDS environment variable (10-600) to tune it without a redeploy.
/// </summary>
internal static class WaitLimits
{
    internal const string MaxWaitSecondsVariable = "AZURE_MIGRATE_WAIT_MAX_SECONDS";

    internal const int DefaultMaxWaitSeconds = 150;

    internal static TimeSpan MaxWait =>
        TimeSpan.FromSeconds(int.TryParse(Environment.GetEnvironmentVariable(MaxWaitSecondsVariable), out int seconds) && seconds is >= 10 and <= 600
            ? seconds
            : DefaultMaxWaitSeconds);
}
