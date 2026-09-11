// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

/// <summary>
/// Device-count values for an IoT Hub metric over the requested time window.
/// </summary>
/// <param name="Snapshot">The most recent point-in-time value in the window (the current exact device count).</param>
/// <param name="Peak">The maximum value observed across the window.</param>
/// <param name="Average">The mean value across the window.</param>
public record IoTHubDeviceCountStats(
    double? Snapshot,
    double? Peak,
    double? Average);
