// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Services.Telemetry;

public interface ITelemetryService : IDisposable
{
    ValueTask<Activity?> StartActivity(string activityName);

    ValueTask<Activity?> StartActivity(string activityName, Implementation? clientInfo);
}
