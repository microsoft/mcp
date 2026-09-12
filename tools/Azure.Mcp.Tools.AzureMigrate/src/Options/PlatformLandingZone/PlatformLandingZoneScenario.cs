// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

/// <summary>Modification scenarios supported by platform landing zone guidance.</summary>
public enum PlatformLandingZoneScenario
{
    /// <summary>Customize resource names.</summary>
    [JsonStringEnumMemberName("resource-names")]
    ResourceNames,

    /// <summary>Customize management groups.</summary>
    [JsonStringEnumMemberName("management-groups")]
    ManagementGroups,

    /// <summary>Configure DDoS protection.</summary>
    [JsonStringEnumMemberName("ddos")]
    Ddos,

    /// <summary>Configure Azure Bastion.</summary>
    [JsonStringEnumMemberName("bastion")]
    Bastion,

    /// <summary>Configure private DNS.</summary>
    [JsonStringEnumMemberName("dns")]
    Dns,

    /// <summary>Configure virtual network gateways.</summary>
    [JsonStringEnumMemberName("gateways")]
    Gateways,

    /// <summary>Configure deployment regions.</summary>
    [JsonStringEnumMemberName("regions")]
    Regions,

    /// <summary>Configure IP address ranges.</summary>
    [JsonStringEnumMemberName("ip-addresses")]
    IpAddresses,

    /// <summary>Configure policy enforcement.</summary>
    [JsonStringEnumMemberName("policy-enforcement")]
    PolicyEnforcement,

    /// <summary>Configure policy assignments.</summary>
    [JsonStringEnumMemberName("policy-assignment")]
    PolicyAssignment,

    /// <summary>Configure Azure Monitor Agent.</summary>
    [JsonStringEnumMemberName("ama")]
    Ama,

    /// <summary>Configure Azure Monitor Baseline Alerts.</summary>
    [JsonStringEnumMemberName("amba")]
    Amba,

    /// <summary>Configure Microsoft Defender plans.</summary>
    [JsonStringEnumMemberName("defender")]
    Defender,

    /// <summary>Configure zero-trust networking.</summary>
    [JsonStringEnumMemberName("zero-trust")]
    ZeroTrust,

    /// <summary>Configure Sovereign Landing Zone controls.</summary>
    [JsonStringEnumMemberName("slz")]
    Slz
}
