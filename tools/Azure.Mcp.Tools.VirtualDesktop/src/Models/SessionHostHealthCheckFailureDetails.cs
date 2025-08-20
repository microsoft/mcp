// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.VirtualDesktop.Models;

public class SessionHostHealthCheckFailureDetails
{
    public SessionHostHealthCheckFailureDetails(Azure.ResourceManager.DesktopVirtualization.Models.SessionHostHealthCheckFailureDetails details)
    {
        Message = details.Message;
        ErrorCode = details.ErrorCode;
        LastHealthCheckOn = details.LastHealthCheckOn;
    }

    /// <summary> Default constructor for serialization. </summary>
    public SessionHostHealthCheckFailureDetails() { }

    public string? Message { get; set; }
    public int? ErrorCode { get; set; }
    public DateTimeOffset? LastHealthCheckOn { get; set; }
}
