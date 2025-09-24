// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Services.Telemetry;

public interface ITelemetryService : IDisposable
{
    Activity? StartActivity(string activityName);

    Activity? StartActivity(string activityName, Implementation? clientInfo);

    ValueTask InitializeAsync();
}
