// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

public record RoutingObservationWindow(
    string StartTime,
    string EndTime,
    string Interval);
