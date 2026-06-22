// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppLens.Options.Resource;

/// <summary>
/// Options for the AppLens resource diagnose command.
/// </summary>
public sealed class ResourceDiagnoseOptions
{
    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = "Azure resource group name. Provide this when disambiguating between multiple resources of the same name.")]
    public string? ResourceGroup { get; set; }

    /// <summary>
    /// The user's question for diagnosis.
    /// </summary>
    [Option(Description = "User question")]
    public required string Question { get; set; }

    /// <summary>
    /// The name of the resource to diagnose.
    /// </summary>
    [Option(Description = "The name of the resource to investigate or diagnose")]
    public required string Resource { get; set; }

    /// <summary>
    /// The Resource Type of the resource to diagnose. This is optional and used to disambiguate between multiple resources with the same name.
    /// </summary>
    [Option(Description = "Resource type. Provide this when disambiguating between multiple resources of the same name.")]
    public string? ResourceType { get; set; }

    /// <summary>
    /// The subscription of the resource to diagnose. This is optional and used to disambiguate between multiple resources with the same name.
    /// </summary>
    [Option(Description = "Azure subscription ID or name. Provide this when disambiguating between multiple resources of the same name.")]
    public string? Subscription { get; set; }
}
