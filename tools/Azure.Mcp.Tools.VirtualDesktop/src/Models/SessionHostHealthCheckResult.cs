// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.DesktopVirtualization.Models;

namespace Azure.Mcp.Tools.VirtualDesktop.Models;

public class SessionHostHealthCheckResult
{
    public SessionHostHealthCheckResult(SessionHostHealthCheckReport report)
    {
        HealthCheckName = report.HealthCheckName?.ToString();
        HealthCheckResult = report.HealthCheckResult?.ToString();
        AdditionalFailureDetails = ConvertFailureDetails(report.AdditionalFailureDetails);
    }

    /// <summary> Default constructor for serialization. </summary>
    public SessionHostHealthCheckResult() { }

    public string? HealthCheckName { get; set; }
    public string? HealthCheckResult { get; set; }
    public SessionHostHealthCheckFailureDetails? AdditionalFailureDetails { get; set; }

    private static SessionHostHealthCheckFailureDetails? ConvertFailureDetails(ResourceManager.DesktopVirtualization.Models.SessionHostHealthCheckFailureDetails? details)
    {
        if (details == null)
        {
            return null;
        }

        return new SessionHostHealthCheckFailureDetails(details);
    }
}
