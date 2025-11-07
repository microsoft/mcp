// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.ManagedLustre.LiveTests;

public partial class ManagedLustreCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    private static readonly string[] _sanitizedHeaders =
    [
        "x-ms-correlation-request-id",
        "x-ms-operation-identifier",
        "x-ms-routing-request-id",
        "x-ms-served-by",
        "X-MSEdge-Ref"
    ];

    // Keep the default sanitizers defined in RecordedCommandTestsBase and add the following headers to be sanitized:
    // - x-ms-correlation-request-id
    // - x-ms-operation-identifier
    // - x-ms-routing-request-id
    // - x-ms-served-by
    // - X-MSEdge-Ref
    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
    [
        .. base.HeaderRegexSanitizers,
        .. _sanitizedHeaders.Select(h => new HeaderRegexSanitizer(new HeaderRegexSanitizerBody(h)))
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..encryptionSettings.keyEncryptionKey.keyUrl")
        {
            Value = "sanitized",
            Regex = "https://(.*?)\\.",
            GroupForReplace = "1"
        })
    ];

    public override CustomDefaultMatcher? TestMatcher => new()
    {
        IgnoredHeaders = string.Join(',', _sanitizedHeaders),
        CompareBodies = false
    };

    [Fact]
    public async Task Should_list_filesystems_by_subscription()
    {
        var result = await CallToolAsync(
            "managedlustre_fs_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var fileSystems = result.AssertProperty("fileSystems");
        Assert.Equal(JsonValueKind.Array, fileSystems.ValueKind);
        var found = false;

        var resourceBaseName = SanitizeAndRecordBaseName(Settings.ResourceBaseName, "resourceBaseName");
        foreach (var fs in fileSystems.EnumerateArray())
        {
            if (fs.ValueKind != JsonValueKind.Object)
                continue;

            if (fs.TryGetProperty("name", out var nameProp) &&
                nameProp.ValueKind == JsonValueKind.String &&
                string.Equals(nameProp.GetString(), resourceBaseName, StringComparison.OrdinalIgnoreCase))
            {
                found = true;
                break;
            }
        }

        Assert.True(found, $"Expected at least one filesystem in resource group with name '{resourceBaseName}'.");
    }

    [Fact]
    public async Task Should_calculate_required_subnet_size()
    {
        var result = await CallToolAsync(
            "managedlustre_fs_subnetsize_ask",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "sku", "AMLFS-Durable-Premium-40" },
                { "size", 480 }
            });

        var ips = result.AssertProperty("numberOfRequiredIPs");
        Assert.Equal(JsonValueKind.Number, ips.ValueKind);
        Assert.Equal(21, ips.GetInt32());
    }

    [Fact]
    public async Task Should_get_sku_info()
    {
        var result = await CallToolAsync(
            "managedlustre_fs_sku_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var skus = result.AssertProperty("skus");
        Assert.Equal(JsonValueKind.Array, skus.ValueKind);
    }

    [Fact]
    public async Task Should_get_sku_info_zonal_support()
    {
        var result = await CallToolAsync(
            "managedlustre_fs_sku_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "location", "westeurope" }
            });

        var skus = result.AssertProperty("skus");
        foreach (var sku in skus.EnumerateArray())
        {
            var supportsZones = sku.AssertProperty("supportsZones");
            Assert.True(supportsZones.GetBoolean(), "'supportsZones' must be true.");
        }
    }

    [Fact]
    public async Task Should_get_sku_info_no_zonal_support()
    {
        var result = await CallToolAsync(
            "managedlustre_fs_sku_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "location", "westus" }
            });

        var skus = result.AssertProperty("skus");
        Assert.Equal(JsonValueKind.Array, skus.ValueKind);
        foreach (var sku in skus.EnumerateArray())
        {
            var supportsZones = sku.AssertProperty("supportsZones");
            Assert.False(supportsZones.GetBoolean(), "'supportsZones' must be false.");
        }
    }

    [Fact]
    public async Task Should_create_azure_managed_lustre_no_blob_no_cmk()
    {
        var fsName = RegisterOrRetrieveVariable("amlfsName", $"amlfs-{Guid.NewGuid().ToString("N")[..8]}");
        var subnetId = SanitizeAndRecordSubnetId(Settings.DeploymentOutputs.GetValueOrDefault("AMLFS_SUBNET_ID", ""), "amlfsSubnetId");
        var location = RegisterOrRetrieveVariable("location", Settings.DeploymentOutputs.GetValueOrDefault("LOCATION", ""));

        // Calculate CMK required variables

        var keyUri = SanitizeAndRecordKeyVaultUri(Settings.DeploymentOutputs.GetValueOrDefault("KEY_URI_WITH_VERSION", ""), "keyUriWithVersion");
        var keyVaultResourceId = SanitizeAndRecordKeyVaultResource(Settings.DeploymentOutputs.GetValueOrDefault("KEY_VAULT_RESOURCE_ID", ""), "keyVaultResourceId");
        var userAssignedIdentityId = SanitizeAndRecordUserAssignedIdentityId(Settings.DeploymentOutputs.GetValueOrDefault("USER_ASSIGNED_IDENTITY_RESOURCE_ID", ""), "userAssignedIdentityId");

        // Calculate HSM required variables
        var hsmDataContainerId = SanitizeAndRecordContainerId(Settings.DeploymentOutputs.GetValueOrDefault("HSM_CONTAINER_ID", ""), "hsmContainerId");
        var hsmLogContainerId = SanitizeAndRecordContainerId(Settings.DeploymentOutputs.GetValueOrDefault("HSM_LOGS_CONTAINER_ID", ""), "hsmLogsContainerId");

        var result = await CallToolAsync(
            "managedlustre_fs_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName) },
                { "location", location },
                { "name", fsName },
                { "sku", "AMLFS-Durable-Premium-500" },
                { "size", 4 },
                { "zone", 1 },
                { "subnet-id", subnetId },
                { "hsm-container", hsmDataContainerId },
                { "hsm-log-container", hsmLogContainerId },
                { "custom-encryption", true },
                { "key-url", keyUri },
                { "source-vault", keyVaultResourceId },
                { "user-assigned-identity-id", userAssignedIdentityId },
                { "maintenance-day", "Monday" },
                { "maintenance-time", "01:00" },
                { "root-squash-mode", "All" },
                { "no-squash-nid-list", "10.0.0.4"},
                { "squash-uid", 1000 },
                { "squash-gid", 1000 }
            });

        var fileSystem = result.AssertProperty("fileSystem");
        Assert.Equal(JsonValueKind.Object, fileSystem.ValueKind);

        var name = fileSystem.GetProperty("name").GetString();
        // Test Proxy sanitizes the name, in playback check for sanitized name, otherwise check for actual name.
        Assert.Equal((TestMode == TestMode.Playback) ? "Sanitized" : fsName, name);

        var fsLocation = fileSystem.GetProperty("location").GetString();
        Assert.Equal(location, fsLocation);

        var capacity = fileSystem.AssertProperty("storageCapacityTiB");
        Assert.Equal(JsonValueKind.Number, capacity.ValueKind);
        Assert.Equal(4, capacity.GetInt32());
    }

    [Fact]
    public async Task Should_update_maintenance_and_verify_with_list()
    {
        var resourceBaseName = SanitizeAndRecordBaseName(Settings.ResourceBaseName, "resourceBaseName");
        // Update maintenance window for existing filesystem
        var updateResult = await CallToolAsync(
            "managedlustre_fs_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName) },
                { "name", resourceBaseName },
                { "maintenance-day", "Wednesday" },
                { "maintenance-time", "11:00" }
            });

        var updatedFs = updateResult.AssertProperty("fileSystem");
        Assert.Equal(JsonValueKind.Object, updatedFs.ValueKind);

        // Verify via list
        var listResult = await CallToolAsync(
            "managedlustre_fs_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var fileSystems = listResult.AssertProperty("fileSystems");
        Assert.Equal(JsonValueKind.Array, fileSystems.ValueKind);

        var found = false;
        foreach (var fs in fileSystems.EnumerateArray())
        {
            if (fs.ValueKind != JsonValueKind.Object)
                continue;

            if (fs.TryGetProperty("name", out var nameProp) &&
                nameProp.ValueKind == JsonValueKind.String &&
                string.Equals(nameProp.GetString(), resourceBaseName, StringComparison.OrdinalIgnoreCase))
            {
                // Check maintenance fields
                if (fs.TryGetProperty("maintenanceDay", out var dayProp) && dayProp.ValueKind == JsonValueKind.String &&
                    fs.TryGetProperty("maintenanceTime", out var timeProp) && timeProp.ValueKind == JsonValueKind.String)
                {
                    Assert.Equal("Wednesday", dayProp.GetString());
                    Assert.Equal("11:00", timeProp.GetString());
                    found = true;
                    break;
                }
            }
        }

        Assert.True(found, $"Expected filesystem '{resourceBaseName}' to have maintenance Wednesday at 11:00.");
    }

    [Fact]
    public async Task Should_check_subnet_size_and_succeed()
    {
        var result = await CallToolAsync(
            "managedlustre_fs_subnetsize_validate",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "sku", "AMLFS-Durable-Premium-40" },
                { "size", 480 },
                { "location", RegisterOrRetrieveVariable("location", Settings.DeploymentOutputs.GetValueOrDefault("LOCATION", "")) },
                { "subnet-id", SanitizeAndRecordSubnetId(Settings.DeploymentOutputs.GetValueOrDefault("AMLFS_SUBNET_ID", ""), "amlfsSubnetId") }
            });

        var valid = result.AssertProperty("valid");
        Assert.Equal(JsonValueKind.True, valid.ValueKind);
        Assert.True(valid.GetBoolean());
    }

    [Fact]
    public async Task Should_check_subnet_size_and_fail()
    {
        var result = await CallToolAsync(
            "managedlustre_fs_subnetsize_validate",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "sku", "AMLFS-Durable-Premium-40" },
                { "size", 1008 },
                { "location", RegisterOrRetrieveVariable("Location", Settings.DeploymentOutputs.GetValueOrDefault("LOCATION", "")) },
                { "subnet-id", SanitizeAndRecordSubnetId(Settings.DeploymentOutputs.GetValueOrDefault("AMLFS_SUBNET_SMALL_ID", ""), "amlfsSubnetSmallId") }
            });

        var valid = result.AssertProperty("valid");
        Assert.Equal(JsonValueKind.False, valid.ValueKind);
        Assert.False(valid.GetBoolean());
    }

    [Fact]
    public async Task Should_update_root_squash_and_verify_with_list()
    {
        var resourceBaseName = SanitizeAndRecordBaseName(Settings.ResourceBaseName, "resourceBaseName");
        // Update root squash settings for existing filesystem
        var updateResult = await CallToolAsync(
            "managedlustre_fs_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName) },
                { "name", resourceBaseName },
                { "root-squash-mode", "All" },
                { "squash-uid", 2000 },
                { "squash-gid", 2000 },
                { "no-squash-nid-list", "10.0.0.5@tcp" }
            });

        var updatedFs = updateResult.AssertProperty("fileSystem");
        Assert.Equal(JsonValueKind.Object, updatedFs.ValueKind);

        // Validate root squash fields on direct update response
        var rsMode = updatedFs.AssertProperty("rootSquashMode");
        Assert.Equal(JsonValueKind.String, rsMode.ValueKind);
        Assert.Equal("All", rsMode.GetString());
        var rsUid = updatedFs.AssertProperty("squashUid");
        Assert.Equal(JsonValueKind.Number, rsUid.ValueKind);
        var rsGid = updatedFs.AssertProperty("squashGid");
        Assert.Equal(JsonValueKind.Number, rsGid.ValueKind);
        var rsNoSquashList = updatedFs.AssertProperty("noSquashNidList");
        Assert.Equal(JsonValueKind.String, rsNoSquashList.ValueKind);

        // Verify via list
        var listResult = await CallToolAsync(
            "managedlustre_fs_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var fileSystems = listResult.AssertProperty("fileSystems");
        Assert.Equal(JsonValueKind.Array, fileSystems.ValueKind);

        var found = false;
        foreach (var fs in fileSystems.EnumerateArray())
        {
            if (fs.TryGetProperty("name", out var nameProp) &&
                nameProp.ValueKind == JsonValueKind.String &&
                string.Equals(nameProp.GetString(), resourceBaseName, StringComparison.OrdinalIgnoreCase))
            {
                // Assert required root squash fields (must be present)
                var listMode = fs.AssertProperty("rootSquashMode");
                Assert.Equal(JsonValueKind.String, listMode.ValueKind);
                Assert.Equal("All", listMode.GetString());

                var listUid = fs.AssertProperty("squashUid");
                Assert.Equal(JsonValueKind.Number, listUid.ValueKind);
                Assert.Equal(2000, listUid.GetInt32());

                var listGid = fs.AssertProperty("squashGid");
                Assert.Equal(JsonValueKind.Number, listGid.ValueKind);
                Assert.Equal(2000, listGid.GetInt32());

                var listNoSquash = fs.AssertProperty("noSquashNidList");
                Assert.Equal(JsonValueKind.String, listNoSquash.ValueKind);
                Assert.Equal("10.0.0.5@tcp", listNoSquash.GetString());
                found = true;
                break;
            }
        }

        Assert.True(found, $"Expected filesystem '{resourceBaseName}' to be present after root squash update.");
    }

    private string SanitizeAndRecordBaseName(string baseName, string name) => SanitizeAndRecord(baseName, name, val => "Sanitized");

    private string SanitizeAndRecordKeyVaultUri(string keyUri, string name)
        => SanitizeAndRecord(keyUri, name, val => Regex.Replace(val, "https://.*?\\.", "https://Sanitized."));

    private string SanitizeAndRecordSubnetId(string subnetId, string name)
        => SanitizeAndRecordWithSubscription(subnetId, name, "/virtualNetworks/.*?-vnet/subnets", "/virtualNetworks/Sanitized-vnet/subnets");

    private string SanitizeAndRecordContainerId(string containerId, string name)
        => SanitizeAndRecordWithSubscription(containerId, name, "/storageAccounts/.*?/blobServices/", "/storageAccounts/Sanitized/blobServices/");

    private string SanitizeAndRecordKeyVaultResource(string keyVaultResourceId, string name)
        => SanitizeAndRecordWithSubscription(keyVaultResourceId, name, "/vaults/.*", "/vaults/Sanitized");

    private string SanitizeAndRecordUserAssignedIdentityId(string userAssignedIdentityId, string name)
        => SanitizeAndRecordWithSubscription(userAssignedIdentityId, name, "/userAssignedIdentities/.*?-uai", "/userAssignedIdentities/Sanitized-uai");

    /// <summary>
    /// Common sanitization and recording method for various resource IDs. If the test mode is live, returns the unsanitized value.
    /// If the test mode is recorded, the value is sanitized and registered with the given name and the unsanitized value is returned,
    /// as this ensures what is used matches the real value.
    /// If the test mode is playback, the sanitized value is returned.
    /// All resource IDs have a base sanitization of subscription ID and then a specific sanitization based on the resource type.
    /// </summary>
    /// <param name="unsanitizedValue"></param>
    /// <param name="name"></param>
    /// <param name="replaceRegex"></param>
    /// <param name="replacement"></param>
    /// <returns></returns>
    private string SanitizeAndRecordWithSubscription(string unsanitizedValue, string name, string replaceRegex, string replacement)
        => SanitizeAndRecord(unsanitizedValue, name, val =>
        {
            var sanitizedValue = SubscriptionSanitizationRegex().Replace(val, $"/subscriptions/{Guid.Empty}/resourceGroups");
            return Regex.Replace(val, replaceRegex, replacement);
        });

    private string SanitizeAndRecord(string unsanitizedValue, string name, Func<string, string> sanitizer)
    {
        if (TestMode == TestMode.Live)
        {
            // Live tests don't record anything, so just use the actual value.
            return unsanitizedValue;
        }
        else if (TestMode == TestMode.Record)
        {
            // Record tests need to sanitize and register the value, but use the actual value in the test.
            RegisterVariable(name, sanitizer.Invoke(unsanitizedValue));

            return unsanitizedValue;
        }
        else
        {
            // Playback tests need to use the sanitized value.
            return TestVariables[name];
        }
    }

    [GeneratedRegex("/subscriptions/(.*?)/resourceGroups")]
    private static partial Regex SubscriptionSanitizationRegex();
    [Fact]
    public async Task Should_create_autoexport_job()
    {
        var result = await CallToolAsync(
            "managedlustre_fs_autoexport-job_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "filesystem-name", Settings.ResourceBaseName },
                { "tenant", Settings.TenantId }
            });

        var jobName = result.AssertProperty("jobName");
        Assert.Equal(JsonValueKind.String, jobName.ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(jobName.GetString()));
    }
}
