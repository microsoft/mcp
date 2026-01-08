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

    public override List<UriRegexSanitizer> UriRegexSanitizers =>
    [
        .. base.UriRegexSanitizers,
        // Sanitize operation IDs in ascOperations URLs
        new UriRegexSanitizer(new UriRegexSanitizerBody()
        {
            Regex = "/ascOperations/[a-f0-9-]{36}",
            Value = "/ascOperations/sanitized-operation-id"
        }),
        // Sanitize timestamps and certificate data in query parameters
        new UriRegexSanitizer(new UriRegexSanitizerBody()
        {
            Regex = "&t=\\d+&c=MII[^&]*",
            Value = "&t=sanitized&c=sanitized"
        }),
        // Sanitize signature and hash parameters
        new UriRegexSanitizer(new UriRegexSanitizerBody()
        {
            Regex = "&s=[^&]+&h=[^&\\s]+",
            Value = "&s=sanitized&h=sanitized"
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

        var amlfsId = Settings.DeploymentOutputs.GetValueOrDefault("AMLFS_ID", "");
        var resourceBaseName = SanitizeAndRecordBaseName(Settings.ResourceBaseName, "resourceBaseName");
        var amlfsName = amlfsId.Split('/').Last();
        var sanitizedAmlfsName = SanitizeAndRecordBaseName(amlfsName, "existingAmlfsName");

        foreach (var fs in fileSystems.EnumerateArray())
        {
            if (fs.ValueKind != JsonValueKind.Object)
                continue;

            if (fs.TryGetProperty("name", out var nameProp) &&
                nameProp.ValueKind == JsonValueKind.String &&
                string.Equals(nameProp.GetString(), sanitizedAmlfsName, StringComparison.OrdinalIgnoreCase))
            {
                found = true;
                break;
            }
        }

        Assert.True(found, $"Expected at least one filesystem in resource group with name '{sanitizedAmlfsName}'.");
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
    public async Task Should_create_azure_managed_lustre_with_storage_and_cmk()
    {
        // In playback mode, use the exact recorded name; in record mode, generate a new one
        var fsName = RegisterOrRetrieveVariable("amlfsHsmName", $"amlfs-{Guid.NewGuid().ToString("N")[..8]}");
        var subnetId = SanitizeAndRecordSubnetId(Settings.DeploymentOutputs.GetValueOrDefault("AMLFS_SUBNET_ID", ""), "amlfsSubnetId");
        var location = SanitizeAndRecordBaseName(RegisterOrRetrieveVariable("location", Settings.DeploymentOutputs.GetValueOrDefault("LOCATION", "")), "location");

        // Calculate HSM required variables
        var hsmDataContainerId = SanitizeAndRecordContainerId(Settings.DeploymentOutputs.GetValueOrDefault("HSM_CONTAINER_ID", ""), "hsmContainerId");
        var hsmLogContainerId = SanitizeAndRecordContainerId(Settings.DeploymentOutputs.GetValueOrDefault("HSM_LOGS_CONTAINER_ID", ""), "hsmLogsContainerId");

        // Calculate CMK required variables
        var keyUri = SanitizeAndRecordKeyVaultUri(Settings.DeploymentOutputs.GetValueOrDefault("KEY_URI_WITH_VERSION", ""), "keyUriWithVersion");
        var keyVaultResourceId = SanitizeAndRecordKeyVaultResource(Settings.DeploymentOutputs.GetValueOrDefault("KEY_VAULT_RESOURCE_ID", ""), "keyVaultResourceId");
        var userAssignedIdentityId = SanitizeAndRecordUserAssignedIdentityId(Settings.DeploymentOutputs.GetValueOrDefault("USER_ASSIGNED_IDENTITY_RESOURCE_ID", ""), "userAssignedIdentityId");
        var resourceGroupName = SanitizeAndRecordResourceGroupName(Settings.ResourceGroupName, "resourceGroupName");

        var result = await CallToolAsync(
            "managedlustre_fs_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
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
                { "maintenance-time", "01:00" }
            });

        var fileSystem = result.AssertProperty("fileSystem");
        Assert.Equal(JsonValueKind.Object, fileSystem.ValueKind);

        var name = fileSystem.GetProperty("name").GetString();

        var capacity = fileSystem.AssertProperty("storageCapacityTiB");
        Assert.Equal(JsonValueKind.Number, capacity.ValueKind);
        Assert.Equal(4, capacity.GetInt32());

        // Wait for filesystem to be available before creating jobs
        var maxWaitTime = TimeSpan.FromMinutes(30);
        var pollInterval = TestMode == TestMode.Playback ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromSeconds(30);
        var startTime = DateTime.UtcNow;
        var isAvailable = false;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            var listResult = await CallToolAsync(
                "managedlustre_fs_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId }
                });

            var fileSystems = listResult.AssertProperty("fileSystems");
            var targetFileSystem = fileSystems.EnumerateArray()
                .Where(fs => fs.TryGetProperty("name", out var nameProp) &&
                            nameProp.ValueKind == JsonValueKind.String &&
                            string.Equals(nameProp.GetString(), (TestMode == TestMode.Playback) ? "Sanitized" : fsName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(fs => fs.TryGetProperty("provisioningState", out var stateProp) &&
                                     stateProp.ValueKind == JsonValueKind.String &&
                                     string.Equals(stateProp.GetString(), "Succeeded", StringComparison.OrdinalIgnoreCase));

            if (!targetFileSystem.Equals(default(JsonElement)))
            {
                isAvailable = true;
                break;
            }

            if (isAvailable)
            {
                break;
            }

            await Task.Delay(pollInterval, TestContext.Current.CancellationToken);
        }

        Assert.True(isAvailable, $"Filesystem '{fsName}' did not reach 'Succeeded' provisioning state within {maxWaitTime.TotalMinutes} minutes.");

        // Wait for filesystem to stabilize before creating jobs
        await Task.Delay(TestMode == TestMode.Playback ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);

        // Test autoimport job lifecycle
        var autoimportJobNameStr = $"autoimport-{fsName}";
        var autoimportCreateResult = await CallToolAsync(
            "managedlustre_fs_blob_autoimport_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "filesystem-name", fsName },
                { "tenant", Settings.TenantId },
                { "job-name", autoimportJobNameStr }
            });

        var autoimportJobName = autoimportCreateResult.AssertProperty("jobName");
        Assert.Equal(JsonValueKind.String, autoimportJobName.ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(autoimportJobName.GetString()));

        // List autoimport jobs (get without job-name returns all jobs)
        var autoimportListResult = await CallToolAsync(
            "managedlustre_fs_blob_autoimport_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "filesystem-name", fsName },
                { "tenant", Settings.TenantId }
                // Intentionally omitting job-name to list all jobs
            });

        var autoimportJobs = autoimportListResult.AssertProperty("jobs");
        Assert.Equal(JsonValueKind.Array, autoimportJobs.ValueKind);
        var foundAutoimport = false;
        var jobTexts = autoimportJobs.EnumerateArray().Select(job => job.GetRawText());
        foreach (var jobText in jobTexts)
        {
            Output.WriteLine($"Checking job: {jobText}");
            if (jobText.Contains(autoimportJobNameStr))
            {
                Output.WriteLine($"Found job containing: '{autoimportJobNameStr}'");
                foundAutoimport = true;
                break;
            }
        }
        Assert.True(foundAutoimport, $"Expected to find autoimport job '{autoimportJobNameStr}' in the list.");

        // Get autoimport job
        var autoimportGetResult = await CallToolAsync(
            "managedlustre_fs_blob_autoimport_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "filesystem-name", fsName },
                { "job-name", autoimportJobNameStr },
                { "tenant", Settings.TenantId }
            });

        var autoimportJob = autoimportGetResult.AssertProperty("job");
        Assert.Equal(JsonValueKind.Object, autoimportJob.ValueKind);
        var autoimportJobText = autoimportJob.GetRawText();
        Assert.Contains(autoimportJobNameStr, autoimportJobText);

        // Wait 15 seconds to cancel auto import job
        await Task.Delay(TestMode == TestMode.Playback ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);

        // Cancel autoimport job
        var autoimportCancelResult = await CallToolAsync(
            "managedlustre_fs_blob_autoimport_cancel",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "filesystem-name", fsName },
                { "job-name", autoimportJobNameStr },
                { "tenant", Settings.TenantId }
            });

        var autoimportCancelJobName = autoimportCancelResult.AssertProperty("jobName");
        Assert.Contains(autoimportJobNameStr, autoimportCancelJobName.GetRawText());
        var autoimportCancelStatus = autoimportCancelResult.AssertProperty("status");
        Assert.Equal("Cancelled", autoimportCancelStatus.GetString());

        // Delete autoimport job
        var autoimportDeleteResult = await CallToolAsync(
            "managedlustre_fs_blob_autoimport_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "filesystem-name", fsName },
                { "job-name", autoimportJobNameStr },
                { "tenant", Settings.TenantId }
            });

        var autoimportDeleteJobName = autoimportDeleteResult.AssertProperty("jobName");
        Assert.Contains(autoimportJobNameStr, autoimportDeleteJobName.GetRawText());
        var autoimportDeleteStatus = autoimportDeleteResult.AssertProperty("status");
        Assert.Equal("Deleted", autoimportDeleteStatus.GetString());

        // Wait for filesystem to stabilize after deleting import job and before creating export job
        await Task.Delay(TestMode == TestMode.Playback ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);

        // Test autoexport job lifecycle.
        var autoexportJobNameStr = $"autoexport-{fsName}";
        var autoexportCreateResult = await CallToolAsync(
            "managedlustre_fs_blob_autoexport_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "filesystem-name", fsName },
                { "tenant", Settings.TenantId },
                { "job-name", autoexportJobNameStr }
            });

        var autoexportJobName = autoexportCreateResult.AssertProperty("jobName");
        Assert.Equal(JsonValueKind.String, autoexportJobName.ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(autoexportJobName.GetString()));

        // List autoexport jobs (get without job-name returns all jobs)
        var autoexportListResult = await CallToolAsync(
            "managedlustre_fs_blob_autoexport_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "filesystem-name", fsName },
                { "tenant", Settings.TenantId }
                // Intentionally omitting job-name to list all jobs
            });

        var autoexportJobs = autoexportListResult.AssertProperty("jobs");
        Assert.Equal(JsonValueKind.Array, autoexportJobs.ValueKind);
        var foundAutoexport = false;
        var autoexportJobTexts = autoexportJobs.EnumerateArray().Select(job => job.GetRawText());
        foreach (var jobText in autoexportJobTexts)
        {
            if (jobText.Contains(autoexportJobNameStr))
            {
                foundAutoexport = true;
                break;
            }
        }
        Assert.True(foundAutoexport, $"Expected to find autoexport job '{autoexportJobNameStr}' in the list.");

        // Get autoexport job
        var autoexportGetResult = await CallToolAsync(
            "managedlustre_fs_blob_autoexport_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "filesystem-name", fsName },
                { "job-name", autoexportJobNameStr },
                { "tenant", Settings.TenantId }
            });

        var autoexportJob = autoexportGetResult.AssertProperty("job");
        Assert.Equal(JsonValueKind.Object, autoexportJob.ValueKind);
        var autoexportJobText = autoexportJob.GetRawText();
        Assert.Contains(autoexportJobNameStr, autoexportJobText);

        // Wait 15 seconds to cancel auto export job.
        await Task.Delay(TestMode == TestMode.Playback ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);
        // Cancel autoexport job
        var autoexportCancelResult = await CallToolAsync(
            "managedlustre_fs_blob_autoexport_cancel",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "filesystem-name", fsName },
                { "job-name", autoexportJobNameStr },
                { "tenant", Settings.TenantId }
            });

        var autoexportCancelJobName = autoexportCancelResult.AssertProperty("jobName");
        Assert.Contains(autoexportJobNameStr, autoexportCancelJobName.GetRawText());
        var autoexportCancelStatus = autoexportCancelResult.AssertProperty("status");
        Assert.Equal("Cancelled", autoexportCancelStatus.GetString());

        // Delete autoexport job
        var autoexportDeleteResult = await CallToolAsync(
            "managedlustre_fs_blob_autoexport_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "filesystem-name", fsName },
                { "job-name", autoexportJobNameStr },
                { "tenant", Settings.TenantId }
            });

        var autoexportDeleteJobName = autoexportDeleteResult.AssertProperty("jobName");
        Assert.Contains(autoexportJobNameStr, autoexportDeleteJobName.GetRawText());
        var autoexportDeleteStatus = autoexportDeleteResult.AssertProperty("status");
        Assert.Equal("Deleted", autoexportDeleteStatus.GetString());
    }

    [Fact]
    public async Task Should_update_maintenance_and_verify_with_list()
    {
        var amlfsId = Settings.DeploymentOutputs.GetValueOrDefault("AMLFS_ID", "");
        var amlfsName = amlfsId.Split('/').Last();
        var sanitizedAmlfsName = SanitizeAndRecordBaseName(amlfsName, "existingAmlfsName");
        var resourceGroupName = SanitizeAndRecordResourceGroupName(Settings.ResourceGroupName, "resourceGroupName");

        // Update maintenance window for existing filesystem
        var updateResult = await CallToolAsync(
            "managedlustre_fs_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "name", sanitizedAmlfsName },
                { "maintenance-day", "Wednesday" },
                { "maintenance-time", "11:00" }
            });

        var updatedFs = updateResult.AssertProperty("fileSystem");
        Assert.Equal(JsonValueKind.Object, updatedFs.ValueKind);

        // Verify via list
        var listResult = await CallToolAsync(
            "managedlustre_fs_list",
            new Dictionary<string, object?>
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

            // Match by name and look for the filesystem with the updated maintenance settings
            if (fs.TryGetProperty("name", out var nameProp) &&
                nameProp.ValueKind == JsonValueKind.String &&
                string.Equals(nameProp.GetString(), sanitizedAmlfsName, StringComparison.OrdinalIgnoreCase))
            {
                // Check if this filesystem has the maintenance settings we just set
                if (fs.TryGetProperty("maintenanceDay", out var dayProp) && dayProp.ValueKind == JsonValueKind.String &&
                    fs.TryGetProperty("maintenanceTime", out var timeProp) && timeProp.ValueKind == JsonValueKind.String)
                {
                    // Check if this is the filesystem with our expected maintenance settings
                    if (dayProp.GetString() == "Wednesday" && timeProp.GetString() == "11:00")
                    {
                        found = true;
                        break;
                    }
                }
            }
        }

        Assert.True(found, $"Expected to find filesystem '{sanitizedAmlfsName}' with maintenance Wednesday at 11:00.");
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
        var amlfsId = Settings.DeploymentOutputs.GetValueOrDefault("AMLFS_ID", "");
        var amlfsName = amlfsId.Split('/').Last();
        var sanitizedAmlfsName = SanitizeAndRecordBaseName(amlfsName, "existingAmlfsName");
        var resourceGroupName = SanitizeAndRecordResourceGroupName(Settings.ResourceGroupName, "resourceGroupName");

        // Update root squash settings for existing filesystem
        var updateResult = await CallToolAsync(
            "managedlustre_fs_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "name", sanitizedAmlfsName },
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

        // Get the updated filesystem's ID for precise matching
        var updatedId = updatedFs.AssertProperty("id");
        Assert.Equal(JsonValueKind.String, updatedId.ValueKind);

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
            // Match by specific root squash configuration since IDs might have sanitization differences
            if (fs.TryGetProperty("name", out var nameProp) &&
                nameProp.ValueKind == JsonValueKind.String &&
                string.Equals(nameProp.GetString(), sanitizedAmlfsName, StringComparison.OrdinalIgnoreCase) &&
                fs.TryGetProperty("rootSquashMode", out var rootSquashProp) &&
                rootSquashProp.ValueKind == JsonValueKind.String &&
                string.Equals(rootSquashProp.GetString(), "All", StringComparison.OrdinalIgnoreCase) &&
                fs.TryGetProperty("squashUid", out var uidProp) &&
                uidProp.ValueKind == JsonValueKind.Number &&
                uidProp.GetInt32() == 2000 &&
                fs.TryGetProperty("squashGid", out var gidProp) &&
                gidProp.ValueKind == JsonValueKind.Number &&
                gidProp.GetInt32() == 2000)
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

        Assert.True(found, $"Expected filesystem '{sanitizedAmlfsName}' to be present after root squash update.");
    }

    private string SanitizeAndRecordBaseName(string baseName, string name) => SanitizeAndRecord(baseName, name, val => "Sanitized");

    private string SanitizeAndRecordResourceGroupName(string resourceGroupName, string name)
        => SanitizeAndRecord(resourceGroupName, name, val => Regex.Replace(val, @"^([^-]+)-.*", "$1-Sanitized"));

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
            return Regex.Replace(sanitizedValue, replaceRegex, replacement);
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
}
