// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore DELETEASSETRESOURCEGROUPNAME DRILLRESOURCEID HEALTHMODELID LIFECYCLEENROLLMENTNAME LIFECYCLESERVICEGROUPNAME CREATIONSERVICEGROUPNAME PLANLIFECYCLESERVICEGROUPNAME RECOVERYRESOURCEID slis WORKFLOWSERVICEGROUPNAME WORKFLOWRECOVERYPLANNAME WORKFLOWRECOVERYRESOURCEID DRILLSDELETEASSETRESOURCEGROUPNAME DRILLSDRILLNAME DRILLSDRILLRESOURCEID DRILLSDRILLRUNNAME DRILLSDRILLRUNRESOURCENAME DRILLSHEALTHMODELID DRILLSLIFECYCLESERVICEGROUPNAME DRILLSRECOVERYRESOURCEID DRILLSSERVICEGROUPNAME DRILLSSOURCELOCATION

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests;

/// <summary>
/// Live / recorded integration tests for the Resilience Management toolset.
/// Resources are provisioned by test-resources.bicep + test-resources-post.ps1.
/// </summary>
public partial class ResilienceManagementCommandTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : ResilienceManagementRecordedTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task Should_get_usage_plan()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var usagePlanName = RegisterOrRetrieveDeploymentOutputVariable("usagePlanName", "USAGEPLANNAME");

        var result = await CallToolAsync(
            "resiliency_usageplan_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "name", usagePlanName }
            });

        var usagePlan = result.AssertProperty("usagePlan");
        Assert.False(string.IsNullOrEmpty(usagePlan.AssertProperty("name").GetString()));
    }

    [Fact]
    public async Task Should_get_usage_plan_enrollment()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var usagePlanName = RegisterOrRetrieveDeploymentOutputVariable("usagePlanName", "USAGEPLANNAME");
        var enrollmentName = RegisterOrRetrieveDeploymentOutputVariable("enrollmentName", "ENROLLMENTNAME");

        var result = await CallToolAsync(
            "resiliency_usageplan_enrollment_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "usage-plan", usagePlanName },
                { "name", enrollmentName }
            });

        var enrollment = result.AssertProperty("enrollment");
        Assert.False(string.IsNullOrEmpty(enrollment.AssertProperty("name").GetString()));
    }

    [Fact]
    public async Task Should_get_goal_assignment()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var goalAssignment = RegisterOrRetrieveDeploymentOutputVariable("goalAssignmentName", "GOALASSIGNMENTNAME");

        var result = await CallToolAsync(
            "resiliency_goal_assignment_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "name", goalAssignment }
            });

        var assignment = result.AssertProperty("goalAssignment");
        Assert.False(string.IsNullOrEmpty(assignment.AssertProperty("name").GetString()));
    }

    [Fact]
    public async Task Should_list_drills()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");

        var result = await CallToolAsync(
            "resiliency_drill_get",
            new()
            {
                { "service-group", serviceGroup }
            });

        var drills = result.AssertProperty("drills");
        Assert.Equal(JsonValueKind.Array, drills.ValueKind);
        Assert.Contains(drills.EnumerateArray(), drill =>
            drill.TryGetProperty("id", out var id) &&
            (id.GetString()?.EndsWith(drillName, StringComparison.OrdinalIgnoreCase) ?? false));
    }

    [Fact]
    public async Task Should_update_drill()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");

        var result = await CallToolAsync(
            "resiliency_drill_update",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drillName },
                { "rbac-setup-mode", "AutomatedBuiltinRoles" }
            });

        var drill = result.AssertProperty("drill");
        Assert.EndsWith(drillName, drill.AssertProperty("id").GetString(), StringComparison.OrdinalIgnoreCase);
        var properties = drill.AssertProperty("properties");
        Assert.Equal("Succeeded", properties.AssertProperty("provisioningState").GetString());
        Assert.Equal("AutomatedBuiltinRoles", properties.AssertProperty("rbacSetupMode").GetString());
    }

    [Fact]
    public async Task Should_create_drill()
    {
        var resourceGroup = RegisterOrRetrieveDeploymentOutputVariable("resourceGroupName", "DRILLSDELETEASSETRESOURCEGROUPNAME");
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSLIFECYCLESERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveVariable("recoveryPlanName", $"mcp-plan-{Guid.NewGuid().ToString("N")[..8]}");
        var drillName = RegisterOrRetrieveVariable("drillName", $"mcp-drill-{Guid.NewGuid().ToString("N")[..8]}");
        var location = RegisterOrRetrieveDeploymentOutputVariable("location", "LOCATION");
        List<Exception> failures = [];
        try
        {
            var planResult = await CallToolAsync("resiliency_recoveryplan_create", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "plan-type", "Zonal" },
                { "plan-description", "Drill creation lifecycle test." },
                { "identity-type", "SystemAssigned" }
            });
            Assert.EndsWith($"/recoveryPlans/{recoveryPlan}",
                planResult.AssertProperty("recoveryPlan").AssertProperty("id").GetString(), StringComparison.OrdinalIgnoreCase);

            var result = await CallToolAsync("resiliency_drill_create", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drillName },
                { "subscription", Settings.SubscriptionId },
                { "region", location },
                { "resource-group", resourceGroup },
                { "drill-type", "Zonal" },
                { "rbac-setup-mode", "AutomatedBuiltinRoles" }
            });
            var drill = result.AssertProperty("drill");
            Assert.EndsWith($"/drills/{drillName}", drill.AssertProperty("id").GetString(), StringComparison.OrdinalIgnoreCase);
            var properties = drill.AssertProperty("properties");
            Assert.Equal("Succeeded", properties.AssertProperty("provisioningState").GetString());
            Assert.Equal("Zonal", properties.AssertProperty("drillType").GetString());
            // recoveryPlanId is read-only: the service links the service group's Zonal recovery plan.
            var recoveryPlanId = properties.AssertProperty("recoveryPlanProperties").AssertProperty("recoveryPlanId").GetString();
            Assert.EndsWith($"/recoveryPlans/{recoveryPlan}", recoveryPlanId, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            failures.Add(ex);
        }

        foreach (var (tool, option, name, successProperty) in new[]
        {
            ("resiliency_drill_delete", "drill", drillName, "success"),
            ("resiliency_recoveryplan_delete", "recoveryplan", recoveryPlan, "deleted")
        })
        {
            try
            {
                var deleted = await CallToolAsync(tool, new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { option, name }
                });
                Assert.True(deleted.AssertProperty(successProperty).GetBoolean());
            }
            catch (Exception cleanupFailure)
            {
                failures.Add(cleanupFailure);
            }
        }
        if (failures.Count > 0)
        {
            throw new AggregateException("The drill creation scenario or its cleanup failed.", failures);
        }
    }

    [Fact]
    public async Task Should_create_drill_without_recovery_plan()
    {
        var resourceGroup = RegisterOrRetrieveDeploymentOutputVariable("resourceGroupName", "DRILLSDELETEASSETRESOURCEGROUPNAME");
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSLIFECYCLESERVICEGROUPNAME");
        var location = RegisterOrRetrieveDeploymentOutputVariable("location", "LOCATION");
        var drillName = RegisterOrRetrieveVariable("drillName", $"mcp-drill-{Guid.NewGuid().ToString("N")[..8]}");
        Exception? failure = null;
        try
        {
            var result = await CallToolAsync("resiliency_drill_create", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drillName },
                { "subscription", Settings.SubscriptionId },
                { "region", location },
                { "resource-group", resourceGroup },
                { "drill-type", "Zonal" },
                { "rbac-setup-mode", "AutomatedBuiltinRoles" },
                { "identity", "{\"type\":\"SystemAssigned\"}" },
                { "chaos-resource-properties", "{\"identity\":{\"type\":\"SystemAssigned\"},\"chaosResourceIdentityForFaults\":{\"type\":\"SystemAssigned\"}}" },
                { "recovery-plan-properties", "{\"identity\":{\"type\":\"SystemAssigned\"}}" },
                { "monitoring-properties", "{\"identity\":{\"type\":\"SystemAssigned\"}}" }
            });
            var drill = result.AssertProperty("drill");
            Assert.EndsWith($"/drills/{drillName}", drill.AssertProperty("id").GetString(), StringComparison.OrdinalIgnoreCase);
            Assert.Equal("SystemAssigned", drill.AssertProperty("identity").AssertProperty("type").GetString());
            var properties = drill.AssertProperty("properties");
            Assert.Equal("Succeeded", properties.AssertProperty("provisioningState").GetString());
            Assert.Equal("Zonal", properties.AssertProperty("drillType").GetString());
            var chaosProperties = properties.AssertProperty("chaosResourceProperties");
            Assert.Equal("SystemAssigned", chaosProperties.AssertProperty("identity").AssertProperty("type").GetString());
            Assert.Equal("SystemAssigned", chaosProperties.AssertProperty("chaosResourceIdentityForFaults").AssertProperty("type").GetString());
            var recoveryPlanProperties = properties.AssertProperty("recoveryPlanProperties");
            Assert.Equal("SystemAssigned", recoveryPlanProperties.AssertProperty("identity").AssertProperty("type").GetString());
            Assert.True(!recoveryPlanProperties.TryGetProperty("recoveryPlanId", out var recoveryPlanId) ||
                recoveryPlanId.ValueKind == JsonValueKind.Null);
            Assert.Equal("SystemAssigned", properties.AssertProperty("monitoringProperties").AssertProperty("identity").AssertProperty("type").GetString());
        }
        catch (Exception ex)
        {
            failure = ex;
            throw;
        }
        finally
        {
            try
            {
                var deleted = await CallToolAsync("resiliency_drill_delete", new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "drill", drillName }
                });
                Assert.True(deleted.AssertProperty("success").GetBoolean());
            }
            catch (Exception cleanupFailure) when (failure is not null)
            {
                throw new AggregateException("The creation scenario and its cleanup both failed.", failure, cleanupFailure);
            }
        }
    }

    [Fact]
    public async Task Should_get_drill()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        var result = await CallToolAsync("resiliency_drill_get", new()
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "name", drillName }
        });
        var drill = result.AssertProperty("drill");
        Assert.EndsWith($"/drills/{drillName}", drill.AssertProperty("id").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Zonal", drill.AssertProperty("properties").AssertProperty("drillType").GetString());
    }

    [Fact]
    public async Task Should_update_and_clear_drill_health_monitoring()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        var healthModelId = RegisterOrRetrieveDeploymentOutputVariable("healthModelId", "DRILLSHEALTHMODELID");
        var getParameters = new Dictionary<string, object?>
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "name", drill }
        };
        var baseline = (await CallToolAsync("resiliency_drill_get", getParameters)).AssertProperty("drill").AssertProperty("properties");
        Assert.Equal("Succeeded", baseline.AssertProperty("provisioningState").GetString());
        Assert.Equal("NotRunning", baseline.AssertProperty("executionState").GetString());
        var originalConfiguration = GetWritableDrillConfiguration(baseline, "healthModelMonitoringProperties", "healthModelId");
        var configuration = new JsonObject
        {
            ["identity"] = new JsonObject { ["type"] = "SystemAssigned" },
            ["healthModelId"] = healthModelId
        };
        var parameters = new Dictionary<string, object?>
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "drill", drill },
            { "health-model-monitoring-properties", configuration.ToJsonString() }
        };
        Exception? failure = null;
        try
        {
            var configured = await CallToolAsync("resiliency_drill_update", parameters);
            var properties = configured.AssertProperty("drill").AssertProperty("properties");
            Assert.Equal("Succeeded", properties.AssertProperty("provisioningState").GetString());
            AssertHealthModelConfiguration(properties, healthModelId);
            var readBack = await CallToolAsync("resiliency_drill_get", getParameters);
            AssertHealthModelConfiguration(readBack.AssertProperty("drill").AssertProperty("properties"), healthModelId);

            configuration.Remove("identity");
            parameters["health-model-monitoring-properties"] = configuration.ToJsonString();
            var patched = await CallToolAsync("resiliency_drill_update", parameters);
            AssertHealthModelConfiguration(patched.AssertProperty("drill").AssertProperty("properties"), healthModelId);
            readBack = await CallToolAsync("resiliency_drill_get", getParameters);
            AssertHealthModelConfiguration(readBack.AssertProperty("drill").AssertProperty("properties"), healthModelId);

            parameters["health-model-monitoring-properties"] = "null";
            var cleared = await CallToolAsync("resiliency_drill_update", parameters);
            properties = cleared.AssertProperty("drill").AssertProperty("properties");
            Assert.True(!properties.TryGetProperty("healthModelMonitoringProperties", out var health) || health.ValueKind == JsonValueKind.Null);
            readBack = await CallToolAsync("resiliency_drill_get", getParameters);
            properties = readBack.AssertProperty("drill").AssertProperty("properties");
            Assert.True(!properties.TryGetProperty("healthModelMonitoringProperties", out health) || health.ValueKind == JsonValueKind.Null);
        }
        catch (Exception ex)
        {
            failure = ex;
            throw;
        }
        finally
        {
            try
            {
                parameters["health-model-monitoring-properties"] = originalConfiguration ?? "null";
                var restored = await CallToolAsync("resiliency_drill_update", parameters);
                Assert.Equal("Succeeded", restored.AssertProperty("drill").AssertProperty("properties").AssertProperty("provisioningState").GetString());
                var readBack = await CallToolAsync("resiliency_drill_get", getParameters);
                var properties = readBack.AssertProperty("drill").AssertProperty("properties");
                Assert.Equal("NotRunning", properties.AssertProperty("executionState").GetString());
                var restoredConfiguration = GetWritableDrillConfiguration(properties, "healthModelMonitoringProperties", "healthModelId");
                Assert.True(JsonNode.DeepEquals(
                    JsonNode.Parse(originalConfiguration ?? "null"),
                    JsonNode.Parse(restoredConfiguration ?? "null")),
                    "The original Health Model monitoring configuration was not restored.");
            }
            catch (Exception cleanupFailure) when (failure is not null)
            {
                throw new AggregateException("The monitoring scenario and its restoration both failed.", failure, cleanupFailure);
            }
        }
    }

    private static void AssertHealthModelConfiguration(JsonElement properties, string healthModelId)
    {
        var health = properties.AssertProperty("healthModelMonitoringProperties");
        Assert.Equal("SystemAssigned", health.AssertProperty("identity").AssertProperty("type").GetString());
        Assert.Equal(healthModelId, health.AssertProperty("healthModelId").GetString(), ignoreCase: true);
    }

    private static string? GetWritableDrillConfiguration(JsonElement properties, string propertyName, string? selection)
    {
        if (!properties.TryGetProperty(propertyName, out var configuration) || configuration.ValueKind == JsonValueKind.Null)
        {
            return null;
        }
        var identity = configuration.AssertProperty("identity");
        var writableIdentity = new JsonObject { ["type"] = identity.AssertProperty("type").GetString() };
        if (identity.TryGetProperty("userAssignedIdentity", out var userIdentity) && userIdentity.ValueKind == JsonValueKind.String)
        {
            writableIdentity["userAssignedIdentity"] = userIdentity.GetString();
        }
        var writableConfiguration = new JsonObject { ["identity"] = writableIdentity };
        if (selection is not null)
        {
            writableConfiguration[selection] = JsonNode.Parse(configuration.AssertProperty(selection).GetRawText());
        }
        return writableConfiguration.ToJsonString();
    }

    [Fact]
    public async Task Should_delete_drill()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSLIFECYCLESERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveVariable("drillName", $"mcp-delete-{Guid.NewGuid().ToString("N")[..8]}");
        var assetGroup = TestMode == Microsoft.Mcp.Tests.Helpers.TestMode.Playback ||
            Settings.DeploymentOutputs.ContainsKey("DELETEASSETRESOURCEGROUPNAME")
            ? RegisterOrRetrieveDeploymentOutputVariable("deleteAssetGroup", "DRILLSDELETEASSETRESOURCEGROUPNAME")
            : RegisterOrRetrieveVariable("deleteAssetGroup", Settings.ResourceGroupName);
        var location = RegisterOrRetrieveDeploymentOutputVariable("location", "LOCATION");
        var created = await CallToolAsync("resiliency_drill_create", new()
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "drill", drillName },
            { "subscription", Settings.SubscriptionId },
            { "region", location },
            { "resource-group", assetGroup },
            { "drill-type", "Zonal" },
            { "rbac-setup-mode", "AutomatedBuiltinRoles" }
        });
        Assert.Equal("Succeeded", created.AssertProperty("drill").AssertProperty("properties").AssertProperty("provisioningState").GetString());

        var result = await CallToolAsync(
            "resiliency_drill_delete",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drillName }
            });

        Assert.True(result.AssertProperty("success").GetBoolean());
        var missing = await CallToolAsync("resiliency_drill_get", new()
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "name", drillName }
        }, resultProcessor: element => element);
        Assert.Equal(404, missing.AssertProperty("status").GetInt32());
    }

    [Fact]
    public async Task Should_check_drill_resync_readiness()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        await ResyncDrillAndWaitAsync(serviceGroup, drillName);
    }

    private async Task ResyncDrillAndWaitAsync(string serviceGroup, string drillName)
    {
        var getParameters = new Dictionary<string, object?>
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "name", drillName }
        };
        var before = await CallToolAsync("resiliency_drill_get", getParameters);
        var beforeProperties = before.AssertProperty("drill").AssertProperty("properties");
        var previousCheck = beforeProperties.TryGetProperty("lastResyncReadinessCheckTime", out var previous)
            ? previous.GetString() : null;

        var result = await CallToolWithConflictRetryAsync(
            "resiliency_drill_check-resync-readiness",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drillName }
            });

        var readiness = result.AssertProperty("readiness");
        Assert.False(string.IsNullOrEmpty(readiness.AssertProperty("operationId").GetString()));
        for (int attempt = 0; attempt < 60; attempt++)
        {
            var current = await CallToolAsync("resiliency_drill_get", getParameters);
            var properties = current.AssertProperty("drill").AssertProperty("properties");
            if (properties.TryGetProperty("lastResyncReadinessCheckTime", out var checkedAt) &&
                checkedAt.ValueKind == JsonValueKind.String &&
                checkedAt.GetString() != previousCheck)
            {
                return;
            }
            await Task.Delay(PollInterval(10000), TestContext.Current.CancellationToken);
        }
        Assert.Fail("The drill resync/readiness timestamp did not advance.");
    }

    [Theory]
    [InlineData("Failover", true)]
    [InlineData("Reprotect", true)]
    [InlineData("Reprotect", false)]
    [InlineData("FailoverReverse", true)]
    [InlineData("ReprotectReverse", true)]
    public Task Should_validate_drill_for_execution(string operation, bool selectSource) =>
        ValidateDrillForExecutionAsync(operation, selectSource);

    [Fact]
    public Task Should_validate_drill_for_execution_with_default_operation() =>
        ValidateDrillForExecutionAsync(null, selectSource: true);

    private async Task ValidateDrillForExecutionAsync(string? operation, bool selectSource)
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        var sourceLocation = RegisterOrRetrieveDeploymentOutputVariable("sourceLocation", "DRILLSSOURCELOCATION");

        var parameters = new Dictionary<string, object?>
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drillName }
            };
        if (operation is not null)
        {
            parameters["operation-name"] = operation;
        }
        if (selectSource)
        {
            parameters["source-locations"] = new[] { sourceLocation };
        }
        var result = await CallToolWithConflictRetryAsync("resiliency_drill_validate-for-execution", parameters);

        var validation = result.AssertProperty("validation");
        Assert.False(string.IsNullOrEmpty(validation.AssertProperty("operationId").GetString()));
    }

    [Fact]
    public async Task Should_list_drill_resources()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        var drillResourceId = RegisterOrRetrieveDeploymentOutputVariable("drillResourceId", "DRILLSDRILLRESOURCEID");

        var result = await CallToolAsync(
            "resiliency_drill_resource_get",
            new()
            {
                { "service-group", serviceGroup },
                { "drill", drillName }
            });

        var resources = result.AssertProperty("drillResources");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);
        Assert.Contains(resources.EnumerateArray(), resource =>
            string.Equals(drillResourceId, resource.AssertProperty("id").GetString(), StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_get_drill_resource()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        var drillResourceId = RegisterOrRetrieveDeploymentOutputVariable("drillResourceId", "DRILLSDRILLRESOURCEID");
        var drillResourceName = new ResourceIdentifier(drillResourceId).Name;

        var result = await CallToolAsync(
            "resiliency_drill_resource_get",
            new()
            {
                { "service-group", serviceGroup },
                { "drill", drillName },
                { "name", drillResourceName }
            });

        var resource = result.AssertProperty("drillResource");
        Assert.Equal(drillResourceId, resource.AssertProperty("id").GetString(), ignoreCase: true);
        Assert.Contains(resource.AssertProperty("properties").AssertProperty("drillType").GetString(), new[] { "Zonal", "Regional" });
    }

    [Theory]
    [InlineData("TestFailover")]
    [InlineData("Failover")]
    public async Task Should_start_and_end_drill(string mode)
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        await WithDrillRunAsync(serviceGroup, drillName, run =>
        {
            Assert.True(Guid.TryParse(run, out _));
            return Task.CompletedTask;
        }, mode);
    }

    private async Task WithDrillRunAsync(string serviceGroup, string drill, Func<string, Task> scenario, string mode = "Failover")
    {
        var existing = await GetDrillRunNamesAsync(serviceGroup, drill);
        bool started = false;
        Exception? failure = null;
        try
        {
            var start = await StartDrillAsync(serviceGroup, drill, mode);
            started = true;
            Assert.True(Guid.TryParse(start.AssertProperty("operationId").GetString(), out _));
            Assert.Equal("Accepted", start.AssertProperty("status").GetString());
            await WaitForDrillExecutionStateAsync(serviceGroup, drill, "Running");
            string? run = null;
            for (int attempt = 0; attempt < 120 && run is null; attempt++)
            {
                var created = (await GetDrillRunNamesAsync(serviceGroup, drill)).Except(existing).ToArray();
                Assert.True(created.Length <= 1, "Another execution is modifying the isolated drill fixture.");
                run = created.SingleOrDefault();
                if (run is null)
                {
                    await Task.Delay(PollInterval(10000), TestContext.Current.CancellationToken);
                }
            }
            Assert.NotNull(run);
            await scenario(run);
        }
        catch (Exception ex)
        {
            failure = ex;
        }

        try
        {
            if (started)
            {
                var end = await EndDrillAsync(serviceGroup, drill, failure is null ? "Success" : "Failed");
                Assert.True(Guid.TryParse(end.AssertProperty("operationId").GetString(), out _));
                Assert.Equal("Accepted", end.AssertProperty("status").GetString());
                await WaitForDrillExecutionStateAsync(serviceGroup, drill, "NotRunning");
            }
        }
        catch (Exception cleanupFailure) when (failure is not null)
        {
            throw new AggregateException("The drill scenario and its cleanup both failed.", failure, cleanupFailure);
        }
        if (failure is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

    private async Task<string[]> GetDrillRunNamesAsync(string serviceGroup, string drill)
    {
        var result = await CallToolAsync("resiliency_drill_run_get", new()
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "drill", drill }
        });
        // Display names are sanitized; ARM IDs retain each run's identity in playback.
        return result.AssertProperty("drillRuns").EnumerateArray()
            .Select(run => new ResourceIdentifier(run.AssertProperty("id").GetString()!).Name).ToArray();
    }

    private async Task WaitForDrillExecutionStateAsync(string serviceGroup, string drill, string expected)
    {
        for (int attempt = 0; attempt < 90; attempt++)
        {
            var result = await CallToolAsync("resiliency_drill_get", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "name", drill }
            });
            var properties = result.AssertProperty("drill").AssertProperty("properties");
            Assert.NotEqual("Failed", properties.AssertProperty("provisioningState").GetString());
            if (properties.AssertProperty("executionState").GetString() == expected)
            {
                return;
            }
            await Task.Delay(PollInterval(10000), TestContext.Current.CancellationToken);
        }
        Assert.Fail($"Drill execution state did not become {expected}.");
    }

    private async Task<JsonElement> StartDrillAsync(string serviceGroup, string drillName, string mode)
    {
        const int maxAttempts = 36;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            JsonElement? response = await CallToolAsync(
                "resiliency_drill_start",
                new()
                {
                    { "service-group", serviceGroup },
                    { "drill", drillName },
                    { "mode", mode }
                },
                resultProcessor: element => element);
            Assert.True(response.HasValue);

            var status = response.Value.AssertProperty("status").GetInt32();
            if (status == 200)
            {
                return response.Value.AssertProperty("results");
            }

            Assert.Equal(409, status);
            await Task.Delay(PollInterval(15000), TestContext.Current.CancellationToken);
        }

        Assert.Fail($"The drill start operation was not accepted after {maxAttempts} attempts.");
        return default;
    }

    private async Task<JsonElement> EndDrillAsync(string serviceGroup, string drillName, string attestation = "Success")
    {
        const int maxAttempts = 36;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            JsonElement? response = await CallToolAsync(
                "resiliency_drill_end",
                new()
                {
                    { "service-group", serviceGroup },
                    { "drill", drillName },
                    { "attestation", attestation },
                    { "attestation-notes", "Azure MCP recorded lifecycle test completed." }
                },
                resultProcessor: element => element);
            Assert.True(response.HasValue);

            var status = response.Value.AssertProperty("status").GetInt32();
            if (status == 200)
            {
                return response.Value.AssertProperty("results");
            }

            Assert.Equal(409, status);
            await Task.Delay(PollInterval(15000), TestContext.Current.CancellationToken);
        }

        Assert.Fail($"The drill end operation was not accepted after {maxAttempts} attempts.");
        return default;
    }

    [Fact]
    public async Task Should_list_drill_runs()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLSDRILLRUNNAME");

        var result = await CallToolAsync(
            "resiliency_drill_run_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill }
            });

        var drillRuns = result.AssertProperty("drillRuns");
        Assert.Equal(JsonValueKind.Array, drillRuns.ValueKind);
        Assert.Contains(drillRuns.EnumerateArray(), item =>
            item.TryGetProperty("id", out var id) &&
            (id.GetString()?.EndsWith(drillRun, StringComparison.OrdinalIgnoreCase) ?? false));
    }

    [Fact]
    public async Task Should_get_drill_run()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLSDRILLRUNNAME");

        var result = await CallToolAsync(
            "resiliency_drill_run_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "name", drillRun }
            });

        var returnedDrillRun = result.AssertProperty("drillRun");
        Assert.True(returnedDrillRun.AssertProperty("id").GetString()?.EndsWith(drillRun, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_add_notes_to_drill_run()
    {
        const string note = "Recorded MCP add-notes validation.";
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        await WithDrillRunAsync(serviceGroup, drill, async drillRun =>
        {
            var result = await CallToolAsync(
                "resiliency_drill_run_add-notes",
                new()
                {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun },
                { "notes", note }
                });

            Assert.True(result.AssertProperty("accepted").GetBoolean());
            Assert.Equal(drillRun, result.AssertProperty("drillRun").GetString());
            Assert.True(Guid.TryParse(result.AssertProperty("operationId").GetString(), out _));
            await WaitForDrillRunNoteAsync(serviceGroup, drill, drillRun, note);
        });
    }

    [Fact]
    public Task Should_mark_drill_run_stage_complete()
        => RunDrillFailoverLifecycleAsync(selectResources: false, autoFailover: false, completeReprotection: false);

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    public Task Should_start_resume_and_reprotect_drill_run_failover(bool selectResources, bool autoFailover)
        => RunDrillFailoverLifecycleAsync(selectResources, autoFailover);

    // The GA service completes fault injection but never starts the automatic failover stage, so no recording exists.
    [Fact]
    [LiveTestOnly]
    public Task Should_start_resume_and_reprotect_drill_run_with_auto_failover()
        => RunDrillFailoverLifecycleAsync(selectResources: true, autoFailover: true);

    private async Task RunDrillFailoverLifecycleAsync(bool selectResources, bool autoFailover, bool completeReprotection = true)
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        var sourceLocation = RegisterOrRetrieveDeploymentOutputVariable("drillRunSourceLocation", "DRILLSSOURCELOCATION");
        var recoveryResourceId = RegisterOrRetrieveDeploymentOutputVariable("recoveryResourceId", "DRILLSRECOVERYRESOURCEID");
        var drillResourceId = RegisterOrRetrieveDeploymentOutputVariable("drillResourceId", "DRILLSDRILLRESOURCEID");
        await WithDrillRunAsync(serviceGroup, drill, async drillRun =>
        {
            var failoverParameters = new Dictionary<string, object?>
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun },
                { "source-locations", new[] { sourceLocation } },
                { "auto-failover", autoFailover },
                // Recovery Orchestration otherwise rejects failover whenever newly discovered members leave the plan in Warning.
                { "user-consent", "Allowed" }
            };
            if (selectResources)
            {
                failoverParameters["selected-resource-ids"] = new[] { drillResourceId };
            }
            var result = await CallToolWithConflictRetryAsync("resiliency_drill_run_failover", failoverParameters);
            Assert.True(result.AssertProperty("accepted").GetBoolean());
            Assert.Equal(drillRun, result.AssertProperty("drillRun").GetString());
            Assert.True(Guid.TryParse(result.AssertProperty("operationId").GetString(), out _));

            // Automatic failover can continue past fault injection without pausing for manual stages.
            var manualStage = autoFailover
                ? await WaitForAutomaticFailoverProgressAsync(serviceGroup, drill, drillRun)
                : "FaultInjection";
            if (manualStage == "FaultInjection")
            {
                await WaitForStageVerbAsync(serviceGroup, drill, drillRun, "FaultInjection", "MarkAsComplete");
                result = await CallToolWithConflictRetryAsync("resiliency_drill_run_mark-complete", new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "drill", drill },
                    { "drill-run", drillRun },
                    { "stage", "FaultInjection" }
                }, maxAttempts: 36);
                Assert.True(Guid.TryParse(result.AssertProperty("result").AssertProperty("operationId").GetString(), out _));
                await WaitForStageVerbAsync(serviceGroup, drill, drillRun, "Failover", "Start");
            }
            if (manualStage is not null)
            {
                result = await CallToolWithConflictRetryAsync("resiliency_drill_run_resume", new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "drill", drill },
                    { "drill-run", drillRun }
                }, maxAttempts: 36);
                Assert.True(result.AssertProperty("accepted").GetBoolean());
                Assert.Equal(drillRun, result.AssertProperty("drillRun").GetString());
                Assert.True(Guid.TryParse(result.AssertProperty("operationId").GetString(), out _));
            }

            await WaitForStageVerbAsync(serviceGroup, drill, drillRun, "Failover", "MarkAsComplete");
            result = await CallToolWithConflictRetryAsync("resiliency_drill_run_mark-complete", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun },
                { "stage", "Failover" }
            }, maxAttempts: 36);
            Assert.True(Guid.TryParse(result.AssertProperty("result").AssertProperty("operationId").GetString(), out _));
            await WaitForStageVerbAsync(serviceGroup, drill, drillRun, "Reprotect", "Start", "Retry");
            if (!completeReprotection)
            {
                return;
            }

            var reprotectParameters = new Dictionary<string, object?>
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun }
            };
            if (selectResources)
            {
                reprotectParameters["selected-resource-ids"] = new[] { recoveryResourceId };
            }
            result = await CallToolWithConflictRetryAsync("resiliency_drill_run_reprotect", reprotectParameters, maxAttempts: 36);
            Assert.True(result.AssertProperty("accepted").GetBoolean());
            Assert.Equal(drillRun, result.AssertProperty("drillRun").GetString());
            Assert.True(Guid.TryParse(result.AssertProperty("operationId").GetString(), out _));
            await WaitForStageVerbAsync(serviceGroup, drill, drillRun, "Reprotect", "MarkAsComplete");

            var resources = await CallToolAsync("resiliency_drill_run_resource_get", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun }
            });
            var runResources = resources.AssertProperty("drillRunResources").EnumerateArray().ToArray();
            Assert.NotEmpty(runResources);
            foreach (var resource in runResources)
            {
                var resourceName = new ResourceIdentifier(resource.AssertProperty("id").GetString()!).Name;
                var detail = await CallToolAsync("resiliency_drill_run_resource_get", new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "drill", drill },
                    { "drill-run", drillRun },
                    { "name", resourceName }
                });
                AssertNoFailedDrillTasks(detail.AssertProperty("drillRunResource").AssertProperty("properties"));
            }
            result = await CallToolWithConflictRetryAsync("resiliency_drill_run_mark-complete", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun },
                { "stage", "Reprotect" }
            }, maxAttempts: 36);
            Assert.True(Guid.TryParse(result.AssertProperty("result").AssertProperty("operationId").GetString(), out _));
        });
    }

    private async Task<string?> WaitForAutomaticFailoverProgressAsync(string serviceGroup, string drill, string drillRun)
    {
        for (int attempt = 0; attempt < 60; attempt++)
        {
            var result = await CallToolAsync("resiliency_drill_run_get", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "name", drillRun }
            });
            var properties = result.AssertProperty("drillRun").AssertProperty("properties");
            AssertNoFailedDrillTasks(properties);
            var stages = properties.AssertProperty("supportedVerbsForStage").EnumerateArray().ToArray();
            bool HasVerb(string stageName, string verb) => stages.Any(stage =>
                stage.AssertProperty("drillRunStage").GetString() == stageName &&
                stage.AssertProperty("supportedVerbs").EnumerateArray().Any(value => value.GetString() == verb));

            if (HasVerb("Failover", "Cancel") || HasVerb("Failover", "MarkAsComplete"))
            {
                return null;
            }
            if (HasVerb("Failover", "Start"))
            {
                return "Failover";
            }
            if (HasVerb("FaultInjection", "MarkAsComplete"))
            {
                return "FaultInjection";
            }
            await Task.Delay(PollInterval(10000), TestContext.Current.CancellationToken);
        }
        Assert.Fail("Automatic failover did not reach an actionable or running failover stage.");
        return null;
    }

    private async Task WaitForStageVerbAsync(string serviceGroup, string drill, string drillRun, string stageName, params string[] expectedVerbs)
    {
        for (int attempt = 0; attempt < 60; attempt++)
        {
            var result = await CallToolAsync("resiliency_drill_run_get", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "name", drillRun }
            });
            var properties = result.AssertProperty("drillRun").AssertProperty("properties");
            AssertNoFailedDrillTasks(properties);
            if (properties.AssertProperty("supportedVerbsForStage").EnumerateArray().Any(stage =>
                stage.AssertProperty("drillRunStage").GetString() == stageName &&
                stage.AssertProperty("supportedVerbs").EnumerateArray().Any(verb => expectedVerbs.Contains(verb.GetString()))))
            {
                return;
            }
            await Task.Delay(PollInterval(10000), TestContext.Current.CancellationToken);
        }
        Assert.Fail($"Drill run stage '{stageName}' did not offer verb(s) [{string.Join(", ", expectedVerbs)}] within the timeout.");
    }

    private static void AssertNoFailedDrillTasks(JsonElement task)
    {
        if (task.TryGetProperty("status", out var status))
        {
            Assert.True(status.GetString() is not ("Failed" or "Cancelled"), $"Drill task failed: {task}");
        }
        if (task.TryGetProperty("jobExtendedInfo", out var extended) &&
            extended.ValueKind == JsonValueKind.Object &&
            extended.TryGetProperty("tasksList", out var tasks) &&
            tasks.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in tasks.EnumerateArray())
            {
                AssertNoFailedDrillTasks(child);
            }
        }
        if (task.TryGetProperty("subTasksList", out var subTasks) && subTasks.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in subTasks.EnumerateArray())
            {
                AssertNoFailedDrillTasks(child);
            }
        }
    }

    private async Task WaitForDrillRunNoteAsync(string serviceGroup, string drill, string drillRun, string expectedNote)
    {
        bool notePersisted = false;
        for (int attempt = 0; attempt < 60 && !notePersisted; attempt++)
        {
            var getResult = await CallToolAsync(
                "resiliency_drill_run_get",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "drill", drill },
                    { "name", drillRun }
                });

            JsonElement notes = getResult
                .AssertProperty("drillRun")
                .AssertProperty("properties")
                .AssertProperty("notes");
            notePersisted = notes.ValueKind == JsonValueKind.Array &&
                notes.EnumerateArray().Any(note => note.GetString()?.Contains(expectedNote, StringComparison.Ordinal) == true);

            if (!notePersisted && TestMode != Microsoft.Mcp.Tests.Helpers.TestMode.Playback)
            {
                await Task.Delay(PollInterval(15000), TestContext.Current.CancellationToken);
            }
        }

        Assert.True(notePersisted, $"Drill run note '{expectedNote}' was not persisted within the timeout.");
    }

    /// <summary>
    /// Retries only explicit HTTP 409 conflicts while a prior drill operation releases its lock.
    /// </summary>
    private async Task<JsonElement?> CallToolWithConflictRetryAsync(string toolName, Dictionary<string, object?> parameters, int maxAttempts = 6)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var response = await CallToolAsync(toolName, parameters, resultProcessor: element => element);
            var status = response.AssertProperty("status").GetInt32();
            if (status == 200)
            {
                return response.AssertProperty("results");
            }
            Assert.True(status == 409, $"Unexpected response from {toolName}: {response}");
            if (TestMode != Microsoft.Mcp.Tests.Helpers.TestMode.Playback)
            {
                await Task.Delay(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);
            }
        }

        Assert.Fail($"{toolName} remained in conflict after {maxAttempts} attempts.");
        return null;
    }


    [Fact]
    public async Task Should_list_drill_run_resources()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLSDRILLRUNNAME");
        var drillRunResource = RegisterOrRetrieveDeploymentOutputVariable("drillRunResourceName", "DRILLSDRILLRUNRESOURCENAME");

        var result = await CallToolAsync(
            "resiliency_drill_run_resource_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun }
            });

        var drillRunResources = result.AssertProperty("drillRunResources");
        Assert.Equal(JsonValueKind.Array, drillRunResources.ValueKind);
        Assert.Contains(drillRunResources.EnumerateArray(), item =>
            item.TryGetProperty("id", out var id) &&
            (id.GetString()?.EndsWith(drillRunResource, StringComparison.OrdinalIgnoreCase) ?? false));
    }

    [Fact]
    public async Task Should_get_drill_run_resource()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLSDRILLRUNNAME");
        var drillRunResource = RegisterOrRetrieveDeploymentOutputVariable("drillRunResourceName", "DRILLSDRILLRUNRESOURCENAME");

        var result = await CallToolAsync(
            "resiliency_drill_run_resource_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun },
                { "name", drillRunResource }
            });

        var returnedDrillRunResource = result.AssertProperty("drillRunResource");
        Assert.True(returnedDrillRunResource.AssertProperty("id").GetString()?.EndsWith(drillRunResource, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_list_goal_resources()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var goalAssignment = RegisterOrRetrieveDeploymentOutputVariable("goalAssignmentName", "GOALASSIGNMENTNAME");

        var result = await CallToolAsync(
            "resiliency_goal_resource_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "goal-assignment", goalAssignment }
            });

        Assert.Equal(JsonValueKind.Array, result.AssertProperty("goalResources").ValueKind);
    }

    [Fact]
    public async Task Should_get_recoveryplan()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");

        var result = await CallToolAsync(
            "resiliency_recoveryplan_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "name", recoveryPlan }
            });

        var plan = result.AssertProperty("recoveryPlan");
        Assert.False(string.IsNullOrEmpty(plan.AssertProperty("name").GetString()));
    }

    [Fact]
    public async Task Should_update_recoveryplan()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");
        var existingResult = await CallToolAsync(
            "resiliency_recoveryplan_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "name", recoveryPlan }
            });
        var existingPlan = existingResult.AssertProperty("recoveryPlan");
        var existingRecoveryGroups = existingPlan
            .AssertProperty("properties")
            .AssertProperty("recoveryGroupsSetting");
        var existingDefaultGroupProperties = existingRecoveryGroups
            .AssertProperty("defaultGroup")
            .AssertProperty("properties");
        (string? GroupUniqueId, int OrderId, string? Description)[] existingAdditionalGroups = existingRecoveryGroups
            .AssertProperty("additionalGroups")
            .EnumerateArray()
            .Select(group => group.AssertProperty("properties"))
            .Select(properties => (
                properties.AssertProperty("groupUniqueId").GetString(),
                properties.AssertProperty("orderId").GetInt32(),
                properties.AssertProperty("description").GetString()))
            .ToArray();
        var defaultGroupId = existingDefaultGroupProperties.AssertProperty("groupUniqueId").GetString();
        var defaultGroupDescription = existingDefaultGroupProperties.AssertProperty("description").GetString();
        Assert.False(string.IsNullOrEmpty(defaultGroupId));
        Assert.False(string.IsNullOrEmpty(defaultGroupDescription));

        var result = await CallToolAsync(
            "resiliency_recoveryplan_create",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "plan-type", "Zonal" },
                { "plan-description", "Recovery plan created by Azure MCP tests." },
                { "identity-type", "SystemAssigned" }
            });

        var plan = result.AssertProperty("recoveryPlan");
        Assert.EndsWith($"/recoveryPlans/{recoveryPlan}", plan.AssertProperty("id").GetString());
        Assert.Equal("SystemAssigned", plan.AssertProperty("identity").AssertProperty("type").GetString());
        var updatedDefaultGroup = plan.AssertProperty("defaultGroup");
        Assert.Equal(defaultGroupId, updatedDefaultGroup.AssertProperty("groupUniqueId").GetString());
        Assert.Equal(defaultGroupDescription, updatedDefaultGroup.AssertProperty("description").GetString());
        Assert.Equal(
            existingAdditionalGroups,
            plan.AssertProperty("additionalGroups").EnumerateArray().Select(group => (
                group.AssertProperty("groupUniqueId").GetString(),
                group.AssertProperty("orderId").GetInt32(),
                group.AssertProperty("description").GetString())));
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_check_recoveryplan_readiness()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");

        var result = await CallToolAsync(
            "resiliency_recoveryplan_checkreadiness",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan }
            });

        Assert.True(Guid.TryParse(result.AssertProperty("operationId").GetString(), out _));
        Assert.True(Guid.TryParse(result.AssertProperty("recoveryJobId").GetString(), out _));
        Assert.False(string.IsNullOrWhiteSpace(result.AssertProperty("status").GetString()));
        Assert.True(result.AssertProperty("isReady").ValueKind is JsonValueKind.True or JsonValueKind.False);
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_create_update_and_delete_recoveryplan()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("lifecycleServiceGroupName", "PLANLIFECYCLESERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveVariable("lifecycleRecoveryPlanName", $"mcp-lifecycle-{Guid.NewGuid().ToString("N")[..8]}");
        var requestedDefaultGroupId = RegisterOrRetrieveVariable("lifecycleDefaultGroupId", Guid.NewGuid().ToString());
        var automationAccountId = RegisterOrRetrieveDeploymentOutputVariable("automationAccountId", "AUTOMATIONACCOUNTID");
        var failoverRunbook = RegisterOrRetrieveDeploymentOutputVariable("failoverRunbookName", "FAILOVERRUNBOOKNAME");
        var preActions = new JsonArray
        {
            new JsonObject
            {
                ["type"] = "CustomRunbook",
                ["name"] = "Start-dependencies",
                ["description"] = "Start application dependencies",
                ["timeoutInMinutes"] = 30,
                ["actionResourceId"] = $"{automationAccountId}/runbooks/{failoverRunbook}",
                ["associatedIdentity"] = new JsonObject { ["type"] = "SystemAssigned" }
            }
        };
        bool recoveryPlanExists = false;

        try
        {
            var createResult = await CallToolAsync(
                "resiliency_recoveryplan_create",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "recoveryplan", recoveryPlan },
                    { "plan-type", "Zonal" },
                    { "plan-description", "Recovery plan lifecycle test." },
                    { "identity-type", "SystemAssigned" },
                    { "default-group-description", "Lifecycle default group" },
                    { "default-group-unique-id", requestedDefaultGroupId },
                    { "default-group-pre-actions", preActions.ToJsonString() }
                });
            recoveryPlanExists = true;

            var createdPlan = createResult.AssertProperty("recoveryPlan");
            Assert.EndsWith($"/recoveryPlans/{recoveryPlan}", createdPlan.AssertProperty("id").GetString());
            Assert.Equal("SystemAssigned", createdPlan.AssertProperty("identity").AssertProperty("type").GetString());
            var createdDefaultGroup = createdPlan.AssertProperty("defaultGroup");
            var defaultGroupId = createdDefaultGroup.AssertProperty("groupUniqueId").GetString();
            Assert.False(string.IsNullOrEmpty(defaultGroupId));
            // Recordings sanitize the default group ID, so only Live and Record runs can compare the requested value.
            if (TestMode != Microsoft.Mcp.Tests.Helpers.TestMode.Playback)
            {
                Assert.Equal(requestedDefaultGroupId, defaultGroupId, ignoreCase: true);
            }
            Assert.Equal("Lifecycle default group", createdDefaultGroup.AssertProperty("description").GetString());

            var getResult = await CallToolAsync(
                "resiliency_recoveryplan_get",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "name", recoveryPlan }
                });
            var retrievedPlan = getResult.AssertProperty("recoveryPlan");
            Assert.EndsWith(
                $"/recoveryPlans/{recoveryPlan}",
                retrievedPlan.AssertProperty("id").GetString());
            var preAction = Assert.Single(retrievedPlan.AssertProperty("properties").AssertProperty("recoveryGroupsSetting")
                .AssertProperty("defaultGroup").AssertProperty("properties").AssertProperty("preActions").EnumerateArray());
            Assert.Equal($"{automationAccountId}/runbooks/{failoverRunbook}", preAction.AssertProperty("actionResourceId").GetString(), ignoreCase: true);
            Assert.Equal("SystemAssigned", preAction.AssertProperty("associatedIdentity").AssertProperty("type").GetString());

            var updateResult = await CallToolAsync(
                "resiliency_recoveryplan_create",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "recoveryplan", recoveryPlan },
                    { "plan-type", "Zonal" },
                    { "plan-description", "Updated recovery plan lifecycle test." },
                    { "identity-type", "SystemAssigned" }
                });
            var updatedPlan = updateResult.AssertProperty("recoveryPlan");
            Assert.Equal(
                "Updated recovery plan lifecycle test.",
                updatedPlan.AssertProperty("planDescription").GetString());
            var updatedDefaultGroup = updatedPlan.AssertProperty("defaultGroup");
            Assert.Equal(defaultGroupId, updatedDefaultGroup.AssertProperty("groupUniqueId").GetString());
            Assert.Equal("Lifecycle default group", updatedDefaultGroup.AssertProperty("description").GetString());

            var deleteResult = await CallToolAsync(
                "resiliency_recoveryplan_delete",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "recoveryplan", recoveryPlan }
                });
            recoveryPlanExists = false;
            Assert.True(deleteResult.AssertProperty("deleted").GetBoolean());
            Assert.Equal(recoveryPlan, deleteResult.AssertProperty("recoveryPlan").GetString());

            var repeatedDeleteResult = await CallToolAsync(
                "resiliency_recoveryplan_delete",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "recoveryplan", recoveryPlan }
                });
            Assert.False(repeatedDeleteResult.AssertProperty("deleted").GetBoolean());
        }
        finally
        {
            if (recoveryPlanExists)
            {
                await CallToolAsync(
                    "resiliency_recoveryplan_delete",
                    new()
                    {
                        { "tenant", Settings.TenantId },
                        { "service-group", serviceGroup },
                        { "recoveryplan", recoveryPlan }
                    });
            }
        }
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_finalize_recoveryplan()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("finalizeServiceGroupName", "PLANLIFECYCLESERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveVariable("finalizeRecoveryPlanName", $"mcp-finalize-{Guid.NewGuid().ToString("N")[..8]}");
        bool recoveryPlanExists = false;

        try
        {
            await CallToolAsync(
                "resiliency_recoveryplan_create",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "recoveryplan", recoveryPlan },
                    { "plan-type", "Zonal" },
                    { "plan-description", "Recoveryplan finalize test." },
                    { "identity-type", "SystemAssigned" },
                    { "default-group-description", "Finalize default group" }
                });
            recoveryPlanExists = true;

            var listedResources = await CallToolAsync(
                "resiliency_recoveryplan_resource_get",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "recoveryplan", recoveryPlan }
                });
            var resourcesToExclude = new JsonArray();
            foreach (var resourceSummary in listedResources.AssertProperty("recoveryResources").EnumerateArray())
            {
                var resourceId = resourceSummary.AssertProperty("id").GetString();
                var resourceName = resourceId?.Split('/').Last();
                Assert.False(string.IsNullOrEmpty(resourceName));

                var resourceResult = await CallToolAsync(
                    "resiliency_recoveryplan_resource_get",
                    new()
                    {
                        { "tenant", Settings.TenantId },
                        { "service-group", serviceGroup },
                        { "recoveryplan", recoveryPlan },
                        { "name", resourceName }
                    });
                var recoveryResourceUniqueId = resourceResult
                    .AssertProperty("recoveryResource")
                    .AssertProperty("properties")
                    .AssertProperty("recoveryResourceUniqueId")
                    .GetString();
                resourcesToExclude.Add(new JsonObject
                {
                    ["properties"] = new JsonObject
                    {
                        ["recoveryResourceUniqueId"] = recoveryResourceUniqueId,
                        ["inclusionState"] = "Excluded"
                    }
                });
            }

            if (resourcesToExclude.Count > 0)
            {
                var updateResult = await CallToolAsync(
                    "resiliency_recoveryplan_resource_update",
                    new()
                    {
                        { "tenant", Settings.TenantId },
                        { "service-group", serviceGroup },
                        { "recoveryplan", recoveryPlan },
                        { "resources-to-update", resourcesToExclude.ToJsonString() }
                    });
                Assert.Empty(updateResult
                    .AssertProperty("result")
                    .AssertProperty("failedResources")
                    .EnumerateArray());
            }

            var finalizeResult = await CallToolAsync(
                "resiliency_recoveryplan_finalize",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "recoveryplan", recoveryPlan }
                });

            Assert.True(Guid.TryParse(finalizeResult.AssertProperty("operationId").GetString(), out _));

            var getResult = await CallToolAsync(
                "resiliency_recoveryplan_get",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "name", recoveryPlan }
                });
            var planState = getResult
                .AssertProperty("recoveryPlan")
                .AssertProperty("properties")
                .AssertProperty("planState")
                .GetString();
            Assert.Contains(planState, new[] { "Ready", "Warning" });
        }
        finally
        {
            if (recoveryPlanExists)
            {
                await CallToolAsync(
                    "resiliency_recoveryplan_delete",
                    new()
                    {
                        { "tenant", Settings.TenantId },
                        { "service-group", serviceGroup },
                        { "recoveryplan", recoveryPlan }
                    });
            }
        }
    }

    [Fact]
    public async Task Should_list_recovery_resources()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");

        var result = await CallToolAsync(
            "resiliency_recoveryplan_resource_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan }
            });

        Assert.Equal(JsonValueKind.Array, result.AssertProperty("recoveryResources").ValueKind);
    }

    [Fact]
    public async Task Should_update_recoveryplan_resources()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");

        var listedResources = await CallToolAsync(
            "resiliency_recoveryplan_resource_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan }
            });
        var resourceSummary = listedResources.AssertProperty("recoveryResources").EnumerateArray().First();
        var resourceId = resourceSummary.AssertProperty("id").GetString();
        var resourceName = resourceId?.Split('/').Last();
        Assert.False(string.IsNullOrEmpty(resourceName));

        var resourceResult = await CallToolAsync(
            "resiliency_recoveryplan_resource_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "name", resourceName }
            });
        var recoveryResource = resourceResult.AssertProperty("recoveryResource");
        var recoveryResourceUniqueId = recoveryResource
            .AssertProperty("properties")
            .AssertProperty("recoveryResourceUniqueId")
            .GetString();
        var updatedResource = new JsonObject
        {
            ["properties"] = new JsonObject
            {
                ["recoveryResourceUniqueId"] = recoveryResourceUniqueId,
                ["inclusionState"] = "Excluded"
            }
        };

        var result = await CallToolAsync(
            "resiliency_recoveryplan_resource_update",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "resources-to-update", $"[{updatedResource.ToJsonString()}]" }
            });

        var failedResources = result.AssertProperty("result").AssertProperty("failedResources");
        Assert.Equal(JsonValueKind.Array, failedResources.ValueKind);
        Assert.Empty(failedResources.EnumerateArray());

        var updatedResourceResult = await CallToolAsync(
            "resiliency_recoveryplan_resource_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "name", resourceName }
            });
        var inclusionState = updatedResourceResult
            .AssertProperty("recoveryResource")
            .AssertProperty("properties")
            .AssertProperty("inclusionState")
            .GetString();
        Assert.Equal("Excluded", inclusionState);
    }

    [Fact]
    public async Task Should_reject_cross_zone_vm_recovery_without_zone_movement_protection()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "DRILLSRECOVERYPLANNAME");
        var recoveryResourceId = RegisterOrRetrieveDeploymentOutputVariable("recoveryResourceId", "DRILLSRECOVERYRESOURCEID");
        var resourceName = new ResourceIdentifier(recoveryResourceId).Name;

        async Task<JsonElement> GetResourcePropertiesAsync() => (await CallToolAsync("resiliency_recoveryplan_resource_get", new()
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "recoveryplan", recoveryPlan },
            { "name", resourceName }
        })).AssertProperty("recoveryResource").AssertProperty("properties");

        async Task UpdateProtectionAsync(string uniqueId, string solutionType, JsonNode setting)
        {
            var update = new JsonObject
            {
                ["properties"] = new JsonObject
                {
                    ["recoveryResourceUniqueId"] = uniqueId,
                    ["selectedProtectionSolutionType"] = solutionType,
                    ["selectedProtectionSolutionSetting"] = setting
                }
            };
            var result = await CallToolAsync("resiliency_recoveryplan_resource_update", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "resources-to-update", $"[{update.ToJsonString()}]" }
            });
            Assert.Empty(result.AssertProperty("result").AssertProperty("failedResources").EnumerateArray());
        }

        // The drills fixture VM is the only VM recovery resource. It has no zone-movement protection
        // (resiliencyProfile.zoneMovement requires a Compute preview), so the service must reject the selection.
        var original = await GetResourcePropertiesAsync();
        Assert.Equal("Included", original.AssertProperty("inclusionState").GetString());
        var uniqueId = original.AssertProperty("recoveryResourceUniqueId").GetString()!;
        var originalType = original.AssertProperty("selectedProtectionSolutionType").GetString()!;
        var originalSetting = JsonNode.Parse(original.AssertProperty("selectedProtectionSolutionSetting").GetRawText())!;
        Assert.Equal("CustomRunbook", originalType);
        try
        {
            var update = new JsonObject
            {
                ["properties"] = new JsonObject
                {
                    ["recoveryResourceUniqueId"] = uniqueId,
                    ["selectedProtectionSolutionType"] = "CrossZoneVMRecovery",
                    ["selectedProtectionSolutionSetting"] = new JsonObject { ["protectionSolutionType"] = "CrossZoneVMRecovery" }
                }
            };
            var response = await CallToolAsync("resiliency_recoveryplan_resource_update", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "resources-to-update", $"[{update.ToJsonString()}]" }
            }, resultProcessor: element => element);

            Assert.Equal(400, response.AssertProperty("status").GetInt32());
            Assert.Contains("RHROUserErrorUpdateResourcesOperationFailed", response.AssertProperty("message").GetString(), StringComparison.Ordinal);
            Assert.Equal(originalType, (await GetResourcePropertiesAsync()).AssertProperty("selectedProtectionSolutionType").GetString());
        }
        finally
        {
            if ((await GetResourcePropertiesAsync()).AssertProperty("selectedProtectionSolutionType").GetString() != originalType)
            {
                await UpdateProtectionAsync(uniqueId, originalType, originalSetting);
            }
        }
    }

    [Fact]
    public async Task Should_add_or_update_drill_resources()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME");
        var drillResourceId = RegisterOrRetrieveDeploymentOutputVariable("drillResourceId", "DRILLSDRILLRESOURCEID");

        var includePayload = new JsonArray
        {
            new JsonObject { ["id"] = drillResourceId }
        };

        var result = await CallToolWithConflictRetryAsync(
            "resiliency_drill_resource_add-or-update",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drillName },
                { "fault-duration-minutes", "1" },
                { "force-inclusion-and-update", "Disable" },
                { "include-resources", includePayload.ToJsonString() }
            });

        var operationId = result.AssertProperty("result").AssertProperty("operationId").GetString();
        Assert.False(string.IsNullOrEmpty(operationId));
        // Resource updates do not refresh the drill-level readiness summary.
        await ResyncDrillAndWaitAsync(serviceGroup, drillName);
        for (int attempt = 0; attempt < 60; attempt++)
        {
            var current = await CallToolAsync("resiliency_drill_get", new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "name", drillName }
            });
            var properties = current.AssertProperty("drill").AssertProperty("properties");
            var chaos = properties.AssertProperty("chaosResourceProperties");
            if (chaos.TryGetProperty("faultDurationInMin", out var duration) &&
                duration.ValueKind == JsonValueKind.Number && duration.GetInt32() == 1 &&
                (properties.AssertProperty("executionReadinessState").GetString() == "Ready" ||
                (properties.TryGetProperty("attentionReason", out var attention) &&
                attention.ValueKind == JsonValueKind.Object &&
                attention.TryGetProperty("includedResourceInDrill", out var inclusion) &&
                inclusion.GetString() == "Exists")))
            {
                return;
            }
            await Task.Delay(PollInterval(10000), TestContext.Current.CancellationToken);
        }
        Assert.Fail("The drill did not report an included resource after adding the dedicated VM.");
    }

    [Fact]
    public async Task Should_validate_recovery_plan_for_failover()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");
        var listedResources = await CallToolAsync(
            "resiliency_recoveryplan_resource_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan }
            });
        string? resourceId = null;
        string? sourceLocation = null;
        foreach (JsonElement resource in listedResources.AssertProperty("recoveryResources").EnumerateArray())
        {
            string candidateResourceId = resource.AssertProperty("id").GetString()!;
            var resourceResult = await CallToolAsync(
                "resiliency_recoveryplan_resource_get",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "recoveryplan", recoveryPlan },
                    { "name", candidateResourceId.Split('/').Last() }
                });
            JsonElement physicalZones = resourceResult
                .AssertProperty("recoveryResource")
                .AssertProperty("properties")
                .AssertProperty("resourcePhysicalZones");
            sourceLocation = physicalZones.ValueKind == JsonValueKind.Array
                ? physicalZones.EnumerateArray().Select(zone => zone.GetString()).FirstOrDefault(zone => !string.IsNullOrWhiteSpace(zone))
                : null;
            if (sourceLocation is not null)
            {
                resourceId = candidateResourceId;
                break;
            }
        }

        Assert.False(string.IsNullOrEmpty(resourceId), "The Zonal recoveryplan must contain a resource with a physical zone.");
        Assert.False(string.IsNullOrEmpty(sourceLocation), "A physical source zone is required for Zonal failover validation.");

        var result = await CallToolAsync(
            "resiliency_recoveryplan_validateforfailover",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "source-locations", new[] { sourceLocation } },
                { "selected-resource-ids", new[] { resourceId } }
            });

        Assert.True(Guid.TryParse(result.AssertProperty("operationId").GetString(), out _));
        JsonElement qualifications = result.AssertProperty("recoveryResourceQualifications");
        Assert.Equal(JsonValueKind.Array, qualifications.ValueKind);
        Assert.NotEmpty(qualifications.EnumerateArray());
    }

    [Fact]
    public async Task Should_validate_recoveryplan_for_reprotect()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");

        var result = await CallToolAsync(
            "resiliency_recoveryplan_validateforreprotect",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan }
            });

        Assert.True(Guid.TryParse(result.AssertProperty("operationId").GetString(), out _));
        JsonElement qualifications = result.AssertProperty("recoveryResourceQualifications");
        Assert.Equal(JsonValueKind.Array, qualifications.ValueKind);
        Assert.NotEmpty(qualifications.EnumerateArray());
    }

    [Fact]
    public async Task Should_validate_recoveryplan_for_operation()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("workflowServiceGroupName", "WORKFLOWSERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("workflowRecoveryPlanName", "WORKFLOWRECOVERYPLANNAME");

        var result = await CallToolAsync(
            "resiliency_recoveryplan_validateforoperation",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "operation-name", "Failover" }
            });

        Assert.True(Guid.TryParse(result.AssertProperty("operationId").GetString(), out _));
        Assert.Equal("Failover", result.AssertProperty("operationName").GetString());
        Assert.True(result.AssertProperty("isValid").GetBoolean());
    }

    [Theory]
    [InlineData("resiliency_recoveryplan_failover")]
    [InlineData("resiliency_recoveryplan_finalize")]
    [InlineData("resiliency_recoveryplan_reprotect")]
    public async Task Should_reject_recovery_plan_action_when_plan_does_not_exist(string toolName)
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        const string missingRecoveryPlan = "missing-action-plan";
        var parameters = new Dictionary<string, object?>
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "recoveryplan", missingRecoveryPlan }
        };
        if (toolName == "resiliency_recoveryplan_failover")
        {
            parameters["source-locations"] = new[] { "eastus" };
        }

        JsonElement? result = await CallToolAsync(
            toolName,
            parameters,
            resultProcessor: response => response);

        Assert.NotNull(result);
        Assert.Equal(404, result.Value.AssertProperty("status").GetInt32());
        Assert.Contains("not found", result.Value.AssertProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("resiliency_recoveryjob_retry")]
    [InlineData("resiliency_recoveryjob_resume")]
    public async Task Should_reject_recovery_job_action_when_job_does_not_exist(string toolName)
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");
        var parameters = new Dictionary<string, object?>
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "recoveryplan", recoveryPlan },
            { "recoveryjob", "22222222-2222-2222-2222-222222222222" }
        };
        if (toolName == "resiliency_recoveryjob_resume")
        {
            parameters["description"] = "Approve recovery action";
        }

        JsonElement? result = await CallToolAsync(
            toolName,
            parameters,
            resultProcessor: response => response);

        Assert.NotNull(result);
        Assert.Equal(404, result.Value.AssertProperty("status").GetInt32());
        Assert.Contains("not found", result.Value.AssertProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_retry_failed_recovery_job_and_validate_terminal_result()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");
        var recoveryJob = RegisterOrRetrieveDeploymentOutputVariable("recoveryJobName", "RECOVERYJOBNAME");

        JsonElement failedJob = await GetRecoveryJobAsync(serviceGroup, recoveryPlan, recoveryJob);
        JsonElement failedJobProperties = failedJob.AssertProperty("properties");
        Assert.Equal("Failed", failedJobProperties.AssertProperty("status").GetString());
        string? initialLastModifiedAt = failedJob.AssertProperty("systemData").AssertProperty("lastModifiedAt").GetString();

        var retryResult = await CallToolAsync(
            "resiliency_recoveryjob_retry",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "recoveryjob", recoveryJob }
            });

        Assert.True(Guid.TryParse(retryResult.AssertProperty("operationId").GetString(), out _));
        Assert.Equal("Accepted", retryResult.AssertProperty("status").GetString());
        JsonElement terminalJob = await WaitForRecoveryJobRetryTerminalStateAsync(serviceGroup, recoveryPlan, recoveryJob, initialLastModifiedAt);
        JsonElement terminalJobProperties = terminalJob.AssertProperty("properties");
        Assert.NotEqual(initialLastModifiedAt, terminalJob.AssertProperty("systemData").AssertProperty("lastModifiedAt").GetString());
        Assert.Contains(terminalJobProperties.AssertProperty("status").GetString(), TerminalRecoveryJobStatuses);
    }

    [Fact]
    public async Task Should_failover_resume_and_reprotect_with_terminal_results()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("workflowServiceGroupName", "WORKFLOWSERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("workflowRecoveryPlanName", "WORKFLOWRECOVERYPLANNAME");
        var recoveryResourceId = RegisterOrRetrieveDeploymentOutputVariable("workflowRecoveryResourceId", "WORKFLOWRECOVERYRESOURCEID");
        HashSet<string> existingJobs = await GetRecoveryJobNamesAsync(serviceGroup, recoveryPlan);

        var failoverResult = await CallToolAsync(
            "resiliency_recoveryplan_failover",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "selected-resource-ids", new[] { recoveryResourceId } },
                { "user-consent", "Allowed" }
            });

        Assert.True(Guid.TryParse(failoverResult.AssertProperty("operationId").GetString(), out _));
        Assert.Equal("Accepted", failoverResult.AssertProperty("status").GetString());
        string failoverJob = await RegisterOrRetrieveNewRecoveryJobAsync(
            "workflowFailoverJobName",
            serviceGroup,
            recoveryPlan,
            existingJobs);
        JsonElement pausedJob = await WaitForRecoveryJobStatusAsync(serviceGroup, recoveryPlan, failoverJob, "Paused");
        Assert.Equal("Paused", pausedJob.AssertProperty("properties").AssertProperty("status").GetString());

        var resumeResult = await CallToolAsync(
            "resiliency_recoveryjob_resume",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "recoveryjob", failoverJob },
                { "description", "Approve recorded failover workflow." }
            });

        Assert.True(Guid.TryParse(resumeResult.AssertProperty("operationId").GetString(), out _));
        Assert.Equal("Accepted", resumeResult.AssertProperty("status").GetString());
        JsonElement completedFailover = await WaitForRecoveryJobTerminalStateAsync(serviceGroup, recoveryPlan, failoverJob);
        Assert.Contains(
            completedFailover.AssertProperty("properties").AssertProperty("status").GetString(),
            SuccessfulRecoveryJobStatuses);
        existingJobs.Add(failoverJob);

        var reprotectResult = await CallToolAsync(
            "resiliency_recoveryplan_reprotect",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "selected-resource-ids", new[] { recoveryResourceId } }
            });

        Assert.True(Guid.TryParse(reprotectResult.AssertProperty("operationId").GetString(), out _));
        Assert.Equal("Accepted", reprotectResult.AssertProperty("status").GetString());
        string reprotectJob = await RegisterOrRetrieveNewRecoveryJobAsync(
            "workflowReprotectJobName",
            serviceGroup,
            recoveryPlan,
            existingJobs);
        JsonElement completedReprotect = await WaitForRecoveryJobTerminalStateAsync(serviceGroup, recoveryPlan, reprotectJob);
        Assert.Contains(
            completedReprotect.AssertProperty("properties").AssertProperty("status").GetString(),
            SuccessfulRecoveryJobStatuses);
    }

    [Fact]
    public async Task Should_get_recovery_job()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");

        var listResult = await CallToolAsync(
            "resiliency_recoveryjob_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan }
            });

        var recoveryJobs = listResult.AssertProperty("recoveryJobs");
        Assert.NotEqual(0, recoveryJobs.GetArrayLength());
        var recoveryJob = RegisterOrRetrieveVariable(
            "recoveryJobName",
            recoveryJobs.EnumerateArray().First().AssertProperty("name").GetString()!);

        var result = await CallToolAsync(
            "resiliency_recoveryjob_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "name", recoveryJob }
            });

        var job = result.AssertProperty("recoveryJob");
        Assert.False(string.IsNullOrEmpty(job.AssertProperty("name").GetString()));
    }

    private static readonly string[] TerminalRecoveryJobStatuses =
    [
        "NotApplicable",
        "Completed",
        "CompletedWithWarnings",
        "Failed",
        "Skipped",
        "Cancelled"
    ];

    private static readonly string[] SuccessfulRecoveryJobStatuses =
    [
        "Completed",
        "CompletedWithWarnings"
    ];

    private async Task<JsonElement> GetRecoveryJobAsync(string serviceGroup, string recoveryPlan, string recoveryJob)
    {
        var result = await CallToolAsync(
            "resiliency_recoveryjob_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "name", recoveryJob }
            });

        return result.AssertProperty("recoveryJob");
    }

    private async Task<HashSet<string>> GetRecoveryJobNamesAsync(string serviceGroup, string recoveryPlan)
    {
        var result = await CallToolAsync(
            "resiliency_recoveryjob_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan }
            });

        return result.AssertProperty("recoveryJobs")
            .EnumerateArray()
            .Select(job => job.AssertProperty("name").GetString()!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<string> WaitForNewRecoveryJobAsync(string serviceGroup, string recoveryPlan, HashSet<string> existingJobs)
    {
        const int maxAttempts = 40;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            HashSet<string> currentJobs = await GetRecoveryJobNamesAsync(serviceGroup, recoveryPlan);
            string? newJob = currentJobs.FirstOrDefault(job => !existingJobs.Contains(job));
            if (newJob is not null)
            {
                return newJob;
            }

            await Task.Delay(PollInterval(15000), TestContext.Current.CancellationToken);
        }

        Assert.Fail($"No new recovery job appeared after {maxAttempts} attempts.");
        return string.Empty;
    }

    private async Task<string> RegisterOrRetrieveNewRecoveryJobAsync(
        string variableName,
        string serviceGroup,
        string recoveryPlan,
        HashSet<string> existingJobs)
    {
        if (TestMode == Microsoft.Mcp.Tests.Helpers.TestMode.Playback)
        {
            return RegisterOrRetrieveVariable(variableName, string.Empty);
        }

        string recoveryJob = await WaitForNewRecoveryJobAsync(serviceGroup, recoveryPlan, existingJobs);
        return RegisterOrRetrieveVariable(variableName, recoveryJob);
    }

    private async Task<JsonElement> WaitForRecoveryJobTerminalStateAsync(string serviceGroup, string recoveryPlan, string recoveryJob)
    {
        const int maxAttempts = 40;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            JsonElement job = await GetRecoveryJobAsync(serviceGroup, recoveryPlan, recoveryJob);
            string? status = job.AssertProperty("properties").AssertProperty("status").GetString();
            if (TerminalRecoveryJobStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                return job;
            }

            await Task.Delay(PollInterval(15000), TestContext.Current.CancellationToken);
        }

        Assert.Fail($"Recovery job '{recoveryJob}' did not reach a terminal state after {maxAttempts} attempts.");
        return default;
    }

    private async Task<JsonElement> WaitForRecoveryJobRetryTerminalStateAsync(
        string serviceGroup,
        string recoveryPlan,
        string recoveryJob,
        string? initialLastModifiedAt)
    {
        const int maxAttempts = 40;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            JsonElement job = await GetRecoveryJobAsync(serviceGroup, recoveryPlan, recoveryJob);
            JsonElement properties = job.AssertProperty("properties");
            string? lastModifiedAt = job.AssertProperty("systemData").AssertProperty("lastModifiedAt").GetString();
            string? status = properties.AssertProperty("status").GetString();
            if (!string.Equals(lastModifiedAt, initialLastModifiedAt, StringComparison.Ordinal) &&
                TerminalRecoveryJobStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                return job;
            }

            await Task.Delay(PollInterval(15000), TestContext.Current.CancellationToken);
        }

        Assert.Fail($"Recovery job '{recoveryJob}' did not start a retry and reach a terminal state after {maxAttempts} attempts.");
        return default;
    }

    private async Task<JsonElement> WaitForRecoveryJobStatusAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string expectedStatus)
    {
        const int maxAttempts = 40;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            JsonElement job = await GetRecoveryJobAsync(serviceGroup, recoveryPlan, recoveryJob);
            string? status = job.AssertProperty("properties").AssertProperty("status").GetString();
            if (string.Equals(status, expectedStatus, StringComparison.OrdinalIgnoreCase))
            {
                return job;
            }
            if (TerminalRecoveryJobStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                Assert.Fail($"Recovery job '{recoveryJob}' reached terminal state '{status}' before expected state '{expectedStatus}'.");
            }

            await Task.Delay(PollInterval(15000), TestContext.Current.CancellationToken);
        }

        Assert.Fail($"Recovery job '{recoveryJob}' did not reach state '{expectedStatus}' after {maxAttempts} attempts.");
        return default;
    }

    [Fact]
    public async Task Should_list_recovery_job_resources()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");

        var listResult = await CallToolAsync(
            "resiliency_recoveryjob_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan }
            });

        var recoveryJobs = listResult.AssertProperty("recoveryJobs");
        Assert.NotEqual(0, recoveryJobs.GetArrayLength());
        var recoveryJob = RegisterOrRetrieveVariable(
            "recoveryJobName",
            recoveryJobs.EnumerateArray().First().AssertProperty("name").GetString()!);

        var result = await CallToolAsync(
            "resiliency_recoveryjob_resource_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "recoveryjob", recoveryJob }
            });

        Assert.Equal(JsonValueKind.Array, result.AssertProperty("recoveryJobResources").ValueKind);
    }

    [Fact]
    public async Task Should_create_usage_plan()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("createResourceGroupName", Settings.ResourceGroupName);
        var usagePlanName = RegisterOrRetrieveVariable("createUsagePlanName", $"mcp-usage-plan-{Guid.NewGuid().ToString("N")[..8]}");

        try
        {
            var result = await CallToolAsync(
                "resiliency_usageplan_create",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", resourceGroupName },
                    { "usage-plan", usagePlanName },
                    { "plan-type", "Standard" },
                    { "tags", "{\"mcp-test\":\"usage-plan\"}" }
                });

            var usagePlan = result.AssertProperty("usagePlan");
            Assert.EndsWith($"/usagePlans/{usagePlanName}", usagePlan.AssertProperty("id").GetString(), StringComparison.OrdinalIgnoreCase);
            Assert.Equal("usage-plan", usagePlan.AssertProperty("tags").AssertProperty("mcp-test").GetString());

            // The service drops tags sent on create, so confirm they persisted on the stored plan.
            var persisted = await CallToolAsync(
                "resiliency_usageplan_get",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", resourceGroupName },
                    { "name", usagePlanName }
                });
            Assert.Equal(
                "usage-plan",
                persisted.AssertProperty("usagePlan").AssertProperty("tags").AssertProperty("mcp-test").GetString());
        }
        finally
        {
            await CallToolAsync(
                "resiliency_usageplan_delete",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", resourceGroupName },
                    { "usage-plan", usagePlanName }
                });
        }
    }

    [Fact]
    public async Task Should_delete_usage_plan()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("deleteResourceGroupName", Settings.ResourceGroupName);
        const string usagePlanName = "mcp-delete-plan";

        await CallToolAsync(
            "resiliency_usageplan_create",
            new()
            {
                { "tenant", Settings.TenantId },
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "usage-plan", usagePlanName },
                { "plan-type", "Standard" }
            });

        var result = await CallToolAsync(
            "resiliency_usageplan_delete",
            new()
            {
                { "tenant", Settings.TenantId },
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "usage-plan", usagePlanName }
            });

        Assert.True(result.AssertProperty("deleted").GetBoolean());
        Assert.Equal(usagePlanName, result.AssertProperty("usagePlan").GetString());

        var repeatedResult = await CallToolAsync(
            "resiliency_usageplan_delete",
            new()
            {
                { "tenant", Settings.TenantId },
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "usage-plan", usagePlanName }
            });

        Assert.False(repeatedResult.AssertProperty("deleted").GetBoolean());
    }

    [Fact]
    public async Task Should_create_usage_plan_enrollment()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var usagePlanName = RegisterOrRetrieveDeploymentOutputVariable("usagePlanName", "USAGEPLANNAME");
        var enrollmentName = RegisterOrRetrieveDeploymentOutputVariable("enrollmentName", "ENROLLMENTNAME");

        var result = await CallToolAsync(
            "resiliency_usageplan_enrollment_create",
            new()
            {
                { "tenant", Settings.TenantId },
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "usage-plan", usagePlanName },
                { "enrollment", enrollmentName },
                { "service-group", serviceGroup }
            });

        var enrollment = result.AssertProperty("enrollment");
        Assert.False(string.IsNullOrEmpty(enrollment.AssertProperty("name").GetString()));
    }

    [Fact]
    public async Task Should_delete_usage_plan_enrollment()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("deleteEnrollmentResourceGroupName", Settings.ResourceGroupName);
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("deleteEnrollmentServiceGroupName", "LIFECYCLESERVICEGROUPNAME");
        var usagePlanName = RegisterOrRetrieveDeploymentOutputVariable("deleteEnrollmentUsagePlanName", "USAGEPLANNAME");
        var enrollmentName = RegisterOrRetrieveDeploymentOutputVariable("deleteEnrollmentName", "LIFECYCLEENROLLMENTNAME");

        try
        {
            var result = await CallToolAsync(
                "resiliency_usageplan_enrollment_delete",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", resourceGroupName },
                    { "usage-plan", usagePlanName },
                    { "enrollment", enrollmentName }
                });

            Assert.True(result.AssertProperty("deleted").GetBoolean());
            Assert.Equal(usagePlanName, result.AssertProperty("usagePlan").GetString());
            Assert.Equal(enrollmentName, result.AssertProperty("enrollment").GetString());

            var repeatedResult = await CallToolAsync(
                "resiliency_usageplan_enrollment_delete",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", resourceGroupName },
                    { "usage-plan", usagePlanName },
                    { "enrollment", enrollmentName }
                });

            Assert.False(repeatedResult.AssertProperty("deleted").GetBoolean());
        }
        finally
        {
            await CallToolAsync(
                "resiliency_usageplan_enrollment_create",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", resourceGroupName },
                    { "usage-plan", usagePlanName },
                    { "enrollment", enrollmentName },
                    { "service-group", serviceGroup }
                });
        }
    }
}
