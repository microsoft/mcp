// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.DeviceProvisioning.Tests;

public class DeviceProvisioningCommandTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // Resource-group names can include the recording user's alias.
    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        ..base.BodyKeySanitizers,
        new(new BodyKeySanitizerBody("$..resourcegroup"))
    ];

    public override List<BodyRegexSanitizer> BodyRegexSanitizers =>
    [
        ..base.BodyRegexSanitizers,
        new(new BodyRegexSanitizerBody
        {
            Regex = "resource[Gg]roups/(?<resourceGroup>[^/?\"]+)",
            GroupForReplace = "resourceGroup",
            Value = "Sanitized"
        }),
        new(new BodyRegexSanitizerBody
        {
            Regex = "(?<hubHost>[A-Za-z0-9.-]+\\.azure-devices\\.net)",
            GroupForReplace = "hubHost",
            Value = "Sanitized.azure-devices.net"
        })
    ];

    [Fact]
    public async Task ShouldGetDeviceProvisioningService()
    {
        var result = await CallToolAsync(
            "deviceprovisioning_service_get",
            new()
            {
                { "service", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        Assert.NotNull(result);

        var payload = result.Value;
        var provisioningService = payload.AssertProperty("deviceProvisioningService");
        Assert.Equal(JsonValueKind.Object, provisioningService.ValueKind);
        provisioningService.AssertProperty("name");
        Assert.NotEmpty(
            provisioningService.AssertProperty("idScope").GetString() ?? string.Empty);

        var linkedHubs = provisioningService.AssertProperty("iotHubs");
        Assert.Equal(JsonValueKind.Array, linkedHubs.ValueKind);

        var areResultsTruncated = payload.AssertProperty("areResultsTruncated");
        Assert.False(areResultsTruncated.GetBoolean());
    }

    [Fact]
    public async Task ShouldReportBlockedOnboardingForMissingEnrollment()
    {
        var result = await CallToolAsync(
            "deviceprovisioning_onboarding_validate",
            new()
            {
                { "service", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId },
                { "enrollment-id", "missing-onboarding-enrollment" },
                { "include-registration-state", false }
            });

        Assert.NotNull(result);

        var payload = result.Value;
        var validation = payload.AssertProperty("validation");
        Assert.Equal("issuesFound", validation.AssertProperty("validationResult").GetString());
        Assert.Equal(
            "missing-onboarding-enrollment",
            validation.AssertProperty("targetId").GetString());
        Assert.True(validation.AssertProperty("blockerCount").GetInt32() > 0);

        var areResultsTruncated = payload.AssertProperty("areResultsTruncated");
        Assert.False(areResultsTruncated.GetBoolean());
    }
}
