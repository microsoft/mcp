// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;

namespace Azure.Mcp.Tools.IoTHub.Services;

internal readonly record struct ObservationWindow(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime)
{
    public string Timespan => $"{ToIsoString(StartTime)}/{ToIsoString(EndTime)}";

    private static string ToIsoString(DateTimeOffset value) =>
        value.ToUniversalTime().ToString(
            "yyyy-MM-ddTHH:mm:ss.fffffffZ",
            CultureInfo.InvariantCulture);
}
