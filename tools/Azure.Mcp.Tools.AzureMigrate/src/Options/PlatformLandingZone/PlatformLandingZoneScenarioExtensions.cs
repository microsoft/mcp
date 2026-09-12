// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

internal static class PlatformLandingZoneScenarioExtensions
{
    internal static string ToValue(this PlatformLandingZoneScenario scenario) => scenario switch
    {
        PlatformLandingZoneScenario.ResourceNames => "resource-names",
        PlatformLandingZoneScenario.ManagementGroups => "management-groups",
        PlatformLandingZoneScenario.Ddos => "ddos",
        PlatformLandingZoneScenario.Bastion => "bastion",
        PlatformLandingZoneScenario.Dns => "dns",
        PlatformLandingZoneScenario.Gateways => "gateways",
        PlatformLandingZoneScenario.Regions => "regions",
        PlatformLandingZoneScenario.IpAddresses => "ip-addresses",
        PlatformLandingZoneScenario.PolicyEnforcement => "policy-enforcement",
        PlatformLandingZoneScenario.PolicyAssignment => "policy-assignment",
        PlatformLandingZoneScenario.Ama => "ama",
        PlatformLandingZoneScenario.Amba => "amba",
        PlatformLandingZoneScenario.Defender => "defender",
        PlatformLandingZoneScenario.ZeroTrust => "zero-trust",
        PlatformLandingZoneScenario.Slz => "slz",
        _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null)
    };
}
