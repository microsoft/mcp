// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Services.Telemetry;

/// <summary>
/// A no-operation implementation of the ITelemetryService interface. This class can be used when telemetry is disabled
/// or not needed. All methods in this class are implemented to do nothing and return default values as appropriate.
/// </summary>
public sealed class NoopTelemetryService : ITelemetryService
{
    public void Dispose()
    {
        return;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Activity? StartActivity(string activityName) => null;

    public Activity? StartActivity(string activityName, Implementation? clientInfo) => null;
}
