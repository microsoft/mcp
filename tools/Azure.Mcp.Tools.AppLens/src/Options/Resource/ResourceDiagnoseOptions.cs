// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppLens.Options.Resource;

/// <summary>
/// Options for the AppLens resource diagnose command.
/// </summary>
public class ResourceDiagnoseOptions : GlobalOptions
{
    /// <summary>
    /// The user's question for diagnosis.
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// The name of the resource to diagnose.
    /// </summary>
    public string ResourceName { get; set; } = string.Empty;

    /// <summary>
    /// Optional subscription name or ID for disambiguation.
    /// </summary>
    public string? SubscriptionNameOrId { get; set; }

    /// <summary>
    /// Optional resource type for disambiguation.
    /// </summary>
    public string? ResourceType { get; set; }
}
