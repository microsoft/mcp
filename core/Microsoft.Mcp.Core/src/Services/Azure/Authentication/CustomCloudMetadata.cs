// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Metadata required to connect to a custom Azure cloud.
/// </summary>
public sealed class CustomCloudMetadata
{
    public string? AuthorityHost { get; set; }

    public string? ArmEndpoint { get; set; }

    public string? ResourceManagerAudience { get; set; }

    public string? LogAnalyticsEndpoint { get; set; }

    public string? LogAnalyticsScope { get; set; }

    public string? ApplicationInsightsEndpoint { get; set; }
}
