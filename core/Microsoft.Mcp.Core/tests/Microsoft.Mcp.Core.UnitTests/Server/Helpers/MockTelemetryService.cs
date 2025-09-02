using System.Diagnostics;
using Microsoft.Mcp.Core.Services.Telemetry;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.UnitTests.Server.Helpers;

public class MockTelemetryService : ITelemetryService
{
    public ValueTask<Activity?> StartActivity(string activityName)
    {
        return ValueTask.FromResult<Activity?>(null);
    }

    public ValueTask<Activity?> StartActivity(string activityName, Implementation? clientInfo)
    {
        return ValueTask.FromResult<Activity?>(null);
    }

    public void Dispose()
    {
    }
}
