// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppLens.Options.Resource;

/// <summary>
/// Options for the AppLens resource diagnose command.
/// </summary>
public class ResourceDiagnoseOptions : SubscriptionOptions
{
    /// <summary>
    /// The user's question for diagnosis.
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// The name of the resource to diagnose.
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// The Resource Type of the resource to diagnose
    /// </summary>
    public string? ResourceType { get; set; }
}
