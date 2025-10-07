// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SignalR.Models;

/// <summary>
/// Represents an upstream template definition for forwarding events.
/// </summary>
public sealed class UpstreamTemplate
{
    /// <summary> Authentication settings for the upstream call. </summary>
    public AuthSettings? Auth { get; set; }

    /// <summary> Category pattern used to match events. </summary>
    public string? CategoryPattern { get; set; }

    /// <summary> Event name pattern used to match events. </summary>
    public string? EventPattern { get; set; }

    /// <summary> Hub name pattern used to match events. </summary>
    public string? HubPattern { get; set; }

    /// <summary> URL template for the upstream endpoint. </summary>
    public string? UrlTemplate { get; set; }
}

/// <summary> Represents authentication configuration for an upstream template. </summary>
public sealed class AuthSettings
{
    /// <summary> The auth type (e.g. ManagedIdentity). </summary>
    public string? Type { get; set; }

    /// <summary> The resource scope or audience for authentication. </summary>
    public string? Resource { get; set; }
}
