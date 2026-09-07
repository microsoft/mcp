// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

namespace Azure.Mcp.Tools.AzureMigrate.Helpers;

/// <summary>
/// Builds the <c>properties</c> payload for a Platform Landing Zone PUT.
/// </summary>
/// <remarks>
/// <para>
/// The ARM operation is a declarative replace, so a caller who wants to change one setting on an
/// existing landing zone would otherwise have to restate the entire configuration. To avoid that, the
/// builder layers the caller's options on top of the current resource state read back from GET.
/// </para>
/// <para>
/// Top-level scalars merge. Blocks the caller touches are replaced wholesale rather than deep-merged,
/// because several of them are discriminated unions: deep-merging a new discriminator over an old one
/// leaves behind properties belonging to the previous shape. The service re-applies its own defaults to
/// every replaced block, so nothing the caller cares about is lost.
/// </para>
/// </remarks>
internal static class PlatformLandingZoneRequestBuilder
{
    /// <summary>
    /// Properties owned by the service. They are echoed by GET but must never be sent back on a PUT.
    /// </summary>
    private static readonly string[] s_readOnlyProperties = ["provisioningState", "status", "artifactId"];

    /// <summary>
    /// Builds the desired <c>properties</c> object.
    /// </summary>
    /// <param name="options">The caller-supplied options.</param>
    /// <param name="existing">The current <c>properties</c> from GET, or null when creating.</param>
    /// <returns>The properties object to send.</returns>
    public static JsonObject BuildProperties(RequestOptions options, JsonObject? existing)
    {
        var properties = existing is null
            ? new JsonObject()
            : (JsonObject)existing.DeepClone();

        foreach (var readOnlyProperty in s_readOnlyProperties)
        {
            properties.Remove(readOnlyProperty);
        }

        ApplyScalars(properties, options);
        ApplyRegions(properties, options);
        ApplyPlatformSubscriptions(properties, options);
        ApplyConnectivity(properties, options);

        return properties;
    }

    private static void ApplyScalars(JsonObject properties, RequestOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ScaleTier))
        {
            properties["scaleTier"] = PlatformLandingZoneValueMapper.MapScaleTier(options.ScaleTier);
        }

        if (!string.IsNullOrWhiteSpace(options.VersionControlSystem))
        {
            properties["versionControlSystem"] =
                PlatformLandingZoneValueMapper.MapVersionControlSystem(options.VersionControlSystem);
        }

        if (!string.IsNullOrWhiteSpace(options.OrganizationName))
        {
            properties["organizationName"] = options.OrganizationName;
        }

        if (!string.IsNullOrWhiteSpace(options.ServiceName))
        {
            properties["serviceName"] = options.ServiceName;
        }

        if (!string.IsNullOrWhiteSpace(options.ParentManagementGroupId))
        {
            properties["parentManagementGroupId"] = options.ParentManagementGroupId;
        }
    }

    private static void ApplyRegions(JsonObject properties, RequestOptions options)
    {
        var regions = ParseRegions(options.Regions);
        if (regions.Length == 0)
        {
            return;
        }

        var array = new JsonArray();
        for (var index = 0; index < regions.Length; index++)
        {
            array.Add(new JsonObject
            {
                ["name"] = regions[index],
                ["role"] = index == 0 ? "Primary" : "Secondary"
            });
        }

        properties["regions"] = array;
    }

    private static void ApplyPlatformSubscriptions(JsonObject properties, RequestOptions options)
    {
        var identity = options.IdentitySubscriptionId;
        var management = options.ManagementSubscriptionId;
        var connectivity = options.ConnectivitySubscriptionId;
        var security = options.SecuritySubscriptionId;

        if (string.IsNullOrWhiteSpace(identity) &&
            string.IsNullOrWhiteSpace(management) &&
            string.IsNullOrWhiteSpace(connectivity) &&
            string.IsNullOrWhiteSpace(security))
        {
            return;
        }

        var subscriptions = new JsonObject { ["subscriptionModel"] = "Dedicated" };
        AddSubscriptionRef(subscriptions, "identity", identity);
        AddSubscriptionRef(subscriptions, "management", management);
        AddSubscriptionRef(subscriptions, "connectivity", connectivity);
        AddSubscriptionRef(subscriptions, "security", security);

        properties["platformSubscriptions"] = subscriptions;
    }

    private static void AddSubscriptionRef(JsonObject subscriptions, string name, string? subscriptionId)
    {
        if (!string.IsNullOrWhiteSpace(subscriptionId))
        {
            subscriptions[name] = new JsonObject { ["subscriptionId"] = subscriptionId };
        }
    }

    private static void ApplyConnectivity(JsonObject properties, RequestOptions options)
    {
        var touchesConnectivity =
            !string.IsNullOrWhiteSpace(options.NetworkArchitecture) ||
            !string.IsNullOrWhiteSpace(options.FirewallType) ||
            !string.IsNullOrWhiteSpace(options.Bastion) ||
            !string.IsNullOrWhiteSpace(options.Ddos) ||
            !string.IsNullOrWhiteSpace(options.PrivateDns) ||
            !string.IsNullOrWhiteSpace(options.ExpressRoute) ||
            !string.IsNullOrWhiteSpace(options.VpnGateway);

        if (!touchesConnectivity)
        {
            return;
        }

        var connectivity = properties["connectivity"] is JsonObject existingConnectivity
            ? (JsonObject)existingConnectivity.DeepClone()
            : new JsonObject();

        // The gateway shapes are selected by the same topology discriminator as the hub itself, so the
        // effective topology has to be resolved before any gateway toggle can be mapped.
        string? topology;
        if (!string.IsNullOrWhiteSpace(options.NetworkArchitecture))
        {
            topology = PlatformLandingZoneValueMapper.MapConnectivityTopology(options.NetworkArchitecture);
            connectivity["topology"] = topology;
        }
        else
        {
            topology = connectivity["topology"]?.GetValue<string>();
        }

        if (!string.IsNullOrWhiteSpace(options.FirewallType))
        {
            connectivity["firewall"] = new JsonObject
            {
                ["kind"] = PlatformLandingZoneValueMapper.MapFirewallKind(options.FirewallType)
            };
        }

        if (!string.IsNullOrWhiteSpace(options.Bastion))
        {
            connectivity["bastion"] = new JsonObject
            {
                ["deploymentMode"] = PlatformLandingZoneValueMapper.MapDeploymentMode(options.Bastion, "bastion")
            };
        }

        if (!string.IsNullOrWhiteSpace(options.Ddos))
        {
            connectivity["ddosProtection"] = new JsonObject
            {
                ["deploymentMode"] = PlatformLandingZoneValueMapper.MapDeploymentMode(options.Ddos, "ddos")
            };
        }

        if (!string.IsNullOrWhiteSpace(options.PrivateDns))
        {
            connectivity["privateDns"] = new JsonObject
            {
                ["zoneMode"] = PlatformLandingZoneValueMapper.MapPrivateDnsZoneMode(options.PrivateDns, "private-dns")
            };
        }

        if (!string.IsNullOrWhiteSpace(options.ExpressRoute))
        {
            connectivity["expressRoute"] = new JsonObject
            {
                ["topology"] = PlatformLandingZoneValueMapper.MapGatewayTopology(
                    options.ExpressRoute,
                    "express-route",
                    topology)
            };
        }

        if (!string.IsNullOrWhiteSpace(options.VpnGateway))
        {
            connectivity["vpnGateway"] = new JsonObject
            {
                ["topology"] = PlatformLandingZoneValueMapper.MapGatewayTopology(
                    options.VpnGateway,
                    "vpn-gateway",
                    topology)
            };
        }

        properties["connectivity"] = connectivity;
    }

    /// <summary>
    /// Splits the comma-separated region list.
    /// </summary>
    public static string[] ParseRegions(string? regions) =>
        string.IsNullOrWhiteSpace(regions)
            ? []
            : regions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
