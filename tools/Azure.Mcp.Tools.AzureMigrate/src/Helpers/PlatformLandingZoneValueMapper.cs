// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Helpers;

/// <summary>
/// Maps the friendly, lower-case values accepted on the command line onto the exact enum values
/// required by the Platform Landing Zone ARM contract (which is PascalCase and case-sensitive on
/// discriminators).
/// </summary>
internal static class PlatformLandingZoneValueMapper
{
    /// <summary>
    /// Maps a network architecture to a <c>connectivity.topology</c> value.
    /// </summary>
    public static string MapConnectivityTopology(string value) => value.ToLowerInvariant() switch
    {
        "hubspoke" or "hubandspoke" or "hub-and-spoke" => "HubAndSpoke",
        "vwan" or "virtualwan" or "virtual-wan" => "VirtualWan",
        _ => throw new ArgumentException(
            $"Invalid network architecture '{value}'. Valid values are: hubspoke, vwan.")
    };

    /// <summary>
    /// Maps a firewall type to a <c>connectivity.firewall.kind</c> discriminator.
    /// </summary>
    public static string MapFirewallKind(string value) => value.ToLowerInvariant() switch
    {
        "azurefirewall" or "azure-firewall" => "AzureFirewall",
        "nva" or "thirdpartynva" or "third-party-nva" => "ThirdPartyNva",
        "none" or "disabled" => "None",
        _ => throw new ArgumentException(
            $"Invalid firewall type '{value}'. Valid values are: azurefirewall, nva, none.")
    };

    /// <summary>
    /// Maps a version control system to a <c>versionControlSystem</c> value.
    /// </summary>
    public static string MapVersionControlSystem(string value) => value.ToLowerInvariant() switch
    {
        "github" => "GitHub",
        "azuredevops" or "azure-devops" or "ado" => "AzureDevOps",
        "local" => "Local",
        _ => throw new ArgumentException(
            $"Invalid version control system '{value}'. Valid values are: local, github, azuredevops.")
    };

    /// <summary>
    /// Maps a scale tier to a <c>scaleTier</c> value.
    /// </summary>
    public static string MapScaleTier(string value) => value.ToLowerInvariant() switch
    {
        "full" => "Full",
        "managementonly" or "management-only" => "ManagementOnly",
        _ => throw new ArgumentException(
            $"Invalid scale tier '{value}'. Valid values are: full, managementonly.")
    };

    /// <summary>
    /// Maps an enabled/disabled toggle to a <c>deploymentMode</c> value, used by Bastion and DDoS
    /// protection.
    /// </summary>
    public static string MapDeploymentMode(string value, string optionName) =>
        ParseToggle(value, optionName) ? "Enabled" : "Disabled";

    /// <summary>
    /// Maps an enabled/disabled toggle to a <c>privateDns.zoneMode</c> value.
    /// </summary>
    public static string MapPrivateDnsZoneMode(string value, string optionName) =>
        ParseToggle(value, optionName) ? "PrivateLinkAndAutoRegistration" : "None";

    /// <summary>
    /// Maps an enabled/disabled toggle to a gateway <c>topology</c> discriminator. An enabled gateway
    /// inherits the connectivity topology; a disabled gateway uses the <c>None</c> shape.
    /// </summary>
    /// <param name="value">The toggle value supplied by the caller.</param>
    /// <param name="optionName">The option name, used in error messages.</param>
    /// <param name="connectivityTopology">
    /// The already-mapped connectivity topology the gateway is deployed into. Required when enabling a
    /// gateway, because the gateway shape is selected by the same topology discriminator.
    /// </param>
    public static string MapGatewayTopology(string value, string optionName, string? connectivityTopology)
    {
        if (!ParseToggle(value, optionName))
        {
            return "None";
        }

        if (string.IsNullOrEmpty(connectivityTopology))
        {
            throw new ArgumentException(
                $"'--{optionName} enabled' requires the network architecture to be known, because the " +
                "gateway shape is selected by the connectivity topology. Supply --network-architecture " +
                "in the same call, or omit this option to accept the service default.");
        }

        return connectivityTopology;
    }

    /// <summary>
    /// Parses an enabled/disabled toggle.
    /// </summary>
    private static bool ParseToggle(string value, string optionName) => value.ToLowerInvariant() switch
    {
        "enabled" or "enable" or "true" or "yes" or "on" => true,
        "disabled" or "disable" or "false" or "no" or "off" => false,
        _ => throw new ArgumentException(
            $"Invalid value '{value}' for --{optionName}. Valid values are: enabled, disabled.")
    };
}
