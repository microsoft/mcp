// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

/// <summary>
/// Options for the platform landing zone get guidance command.
/// </summary>
public class GetGuidanceOptions : BaseAzureMigrateOptions
{
    /// <summary>
    /// Gets or sets the scenario key for the modification.
    /// </summary>
    [JsonPropertyName("scenario")]
    public string? Scenario { get; set; }

    /// <summary>
    /// Gets or sets the policy name for policy-related scenarios.
    /// </summary>
    [JsonPropertyName("policyName")]
    public string? PolicyName { get; set; }

    /// <summary>
    /// Gets or sets whether to list all policies by archetype.
    /// </summary>
    [JsonPropertyName("listPolicies")]
    public bool ListPolicies { get; set; }
}
