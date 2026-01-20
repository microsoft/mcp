// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json.Serialization;
namespace Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

/// <summary>
/// Options for the platform landing zone get modification guidance command.
/// </summary>
public class GetModificationGuidanceOptions : BaseAzureMigrateOptions
{
    /// <summary>
    /// Gets or sets the user's question or modification request for the landing zone.
    /// Examples: "turn off bastion", "disable Enable-DDoS-VNET policy", "change IP ranges"
    /// </summary>
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }
}
