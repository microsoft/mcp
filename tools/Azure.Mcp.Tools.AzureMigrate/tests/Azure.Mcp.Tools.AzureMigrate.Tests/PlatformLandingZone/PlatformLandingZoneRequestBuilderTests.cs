// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.PlatformLandingZone;

public class PlatformLandingZoneRequestBuilderTests
{
    [Fact]
    public void BuildProperties_OmittedOptions_ProduceEmptyPayload()
    {
        var properties = PlatformLandingZoneRequestBuilder.BuildProperties(CreateOptions(), existing: null);

        // Anything the caller does not specify is left to the service to default.
        Assert.Empty(properties);
    }

    [Fact]
    public void BuildProperties_MapsTogglesToArmValues()
    {
        var options = CreateOptions();
        options.Bastion = "disabled";
        options.Ddos = "disabled";
        options.PrivateDns = "disabled";
        options.ExpressRoute = "disabled";
        options.VpnGateway = "disabled";

        var connectivity = Assert.IsType<JsonObject>(
            PlatformLandingZoneRequestBuilder.BuildProperties(options, existing: null)["connectivity"]);

        Assert.Equal("Disabled", (string?)connectivity["bastion"]!["deploymentMode"]);
        Assert.Equal("Disabled", (string?)connectivity["ddosProtection"]!["deploymentMode"]);
        Assert.Equal("None", (string?)connectivity["privateDns"]!["zoneMode"]);
        Assert.Equal("None", (string?)connectivity["expressRoute"]!["topology"]);
        Assert.Equal("None", (string?)connectivity["vpnGateway"]!["topology"]);
    }

    [Fact]
    public void BuildProperties_EnabledGateway_InheritsConnectivityTopology()
    {
        var options = CreateOptions();
        options.NetworkArchitecture = "vwan";
        options.ExpressRoute = "enabled";

        var connectivity = Assert.IsType<JsonObject>(
            PlatformLandingZoneRequestBuilder.BuildProperties(options, existing: null)["connectivity"]);

        Assert.Equal("VirtualWan", (string?)connectivity["topology"]);
        Assert.Equal("VirtualWan", (string?)connectivity["expressRoute"]!["topology"]);
    }

    [Fact]
    public void BuildProperties_EnabledGateway_ReusesTopologyFromExistingResource()
    {
        var existing = new JsonObject
        {
            ["connectivity"] = new JsonObject { ["topology"] = "HubAndSpoke" }
        };

        var options = CreateOptions();
        options.VpnGateway = "enabled";

        var connectivity = Assert.IsType<JsonObject>(
            PlatformLandingZoneRequestBuilder.BuildProperties(options, existing)["connectivity"]);

        Assert.Equal("HubAndSpoke", (string?)connectivity["vpnGateway"]!["topology"]);
    }

    [Fact]
    public void BuildProperties_EnabledGateway_WithoutKnownTopology_Throws()
    {
        var options = CreateOptions();
        options.ExpressRoute = "enabled";

        var exception = Assert.Throws<ArgumentException>(
            () => PlatformLandingZoneRequestBuilder.BuildProperties(options, existing: null));

        Assert.Contains("--network-architecture", exception.Message);
    }

    [Fact]
    public void BuildProperties_PreservesUntouchedConfigurationOnUpdate()
    {
        var existing = new JsonObject
        {
            ["scaleTier"] = "Full",
            ["organizationName"] = "contoso",
            ["connectivity"] = new JsonObject
            {
                ["topology"] = "HubAndSpoke",
                ["firewall"] = new JsonObject { ["kind"] = "AzureFirewall" },
                ["ddosProtection"] = new JsonObject { ["deploymentMode"] = "Enabled" }
            }
        };

        var options = CreateOptions();
        options.Ddos = "disabled";

        var properties = PlatformLandingZoneRequestBuilder.BuildProperties(options, existing);
        var connectivity = Assert.IsType<JsonObject>(properties["connectivity"]);

        Assert.Equal("Full", (string?)properties["scaleTier"]);
        Assert.Equal("contoso", (string?)properties["organizationName"]);
        Assert.Equal("HubAndSpoke", (string?)connectivity["topology"]);
        Assert.Equal("AzureFirewall", (string?)connectivity["firewall"]!["kind"]);
        Assert.Equal("Disabled", (string?)connectivity["ddosProtection"]!["deploymentMode"]);
    }

    [Fact]
    public void BuildProperties_ReplacesDiscriminatedBlockRatherThanMerging()
    {
        // The previous shape's properties would be invalid under the new discriminator, so the whole
        // block has to be replaced and re-defaulted by the service.
        var existing = new JsonObject
        {
            ["connectivity"] = new JsonObject
            {
                ["expressRoute"] = new JsonObject
                {
                    ["topology"] = "HubAndSpoke",
                    ["sku"] = "ErGw1AZ",
                    ["fastPathEnabled"] = true
                }
            }
        };

        var options = CreateOptions();
        options.ExpressRoute = "disabled";

        var expressRoute = Assert.IsType<JsonObject>(
            PlatformLandingZoneRequestBuilder.BuildProperties(options, existing)["connectivity"]!["expressRoute"]);

        Assert.Equal("None", (string?)expressRoute["topology"]);
        Assert.False(expressRoute.ContainsKey("sku"));
        Assert.False(expressRoute.ContainsKey("fastPathEnabled"));
    }

    [Fact]
    public void BuildProperties_StripsServiceOwnedProperties()
    {
        var existing = new JsonObject
        {
            ["provisioningState"] = "Succeeded",
            ["status"] = "Succeeded",
            ["artifactId"] = "/subscriptions/s/artifacts/plz-default",
            ["organizationName"] = "contoso"
        };

        var properties = PlatformLandingZoneRequestBuilder.BuildProperties(CreateOptions(), existing);

        Assert.False(properties.ContainsKey("provisioningState"));
        Assert.False(properties.ContainsKey("status"));
        Assert.False(properties.ContainsKey("artifactId"));
        Assert.Equal("contoso", (string?)properties["organizationName"]);
    }

    [Fact]
    public void BuildProperties_MarksFirstRegionAsPrimary()
    {
        var options = CreateOptions();
        options.Regions = "eastus, westus2";

        var regions = Assert.IsType<JsonArray>(
            PlatformLandingZoneRequestBuilder.BuildProperties(options, existing: null)["regions"]);

        Assert.Equal(2, regions.Count);
        Assert.Equal("eastus", (string?)regions[0]!["name"]);
        Assert.Equal("Primary", (string?)regions[0]!["role"]);
        Assert.Equal("westus2", (string?)regions[1]!["name"]);
        Assert.Equal("Secondary", (string?)regions[1]!["role"]);
    }

    [Fact]
    public void BuildProperties_SubscriptionIds_ProduceDedicatedModel()
    {
        var options = CreateOptions();
        options.IdentitySubscriptionId = "id-sub";
        options.SecuritySubscriptionId = "sec-sub";

        var subscriptions = Assert.IsType<JsonObject>(
            PlatformLandingZoneRequestBuilder.BuildProperties(options, existing: null)["platformSubscriptions"]);

        Assert.Equal("Dedicated", (string?)subscriptions["subscriptionModel"]);
        Assert.Equal("id-sub", (string?)subscriptions["identity"]!["subscriptionId"]);
        Assert.Equal("sec-sub", (string?)subscriptions["security"]!["subscriptionId"]);
        Assert.False(subscriptions.ContainsKey("management"));
    }

    [Fact]
    public void BuildProperties_DoesNotMutateTheSuppliedExistingState()
    {
        var existing = new JsonObject
        {
            ["status"] = "Succeeded",
            ["connectivity"] = new JsonObject { ["topology"] = "HubAndSpoke" }
        };

        var options = CreateOptions();
        options.NetworkArchitecture = "vwan";

        PlatformLandingZoneRequestBuilder.BuildProperties(options, existing);

        Assert.Equal("Succeeded", (string?)existing["status"]);
        Assert.Equal("HubAndSpoke", (string?)existing["connectivity"]!["topology"]);
    }

    [Theory]
    [InlineData("maybe")]
    [InlineData("")]
    public void BuildProperties_InvalidToggle_Throws(string value)
    {
        var options = CreateOptions();
        options.Ddos = value;

        if (string.IsNullOrWhiteSpace(value))
        {
            // Whitespace is treated as "not supplied" rather than as an error.
            Assert.Empty(PlatformLandingZoneRequestBuilder.BuildProperties(options, existing: null));
            return;
        }

        Assert.Throws<ArgumentException>(() => PlatformLandingZoneRequestBuilder.BuildProperties(options, existing: null));
    }

    private static RequestOptions CreateOptions() => new()
    {
        Action = "create",
        MigrateProjectName = "project1",
        ResourceGroup = "rg1"
    };
}
