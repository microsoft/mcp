// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests;

// Creation owns a separate fixture and server, independent of the existing lifecycle recordings.
public class DrillCreateRecordedTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : ResilienceManagementRecordedTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task Should_create_drill()
    {
        var resourceGroupName = RegisterOrRetrieveDeploymentOutputVariable("resourceGroupName", "CREATEASSETRESOURCEGROUPNAME");
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "CREATESERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "CREATERECOVERYPLANNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "CREATEDRILLNAME");
        var location = RegisterOrRetrieveDeploymentOutputVariable("location", "CREATELOCATION");
        var healthModelId = RegisterOrRetrieveDeploymentOutputVariable("healthModelId", "HEALTHMODELID");
        var getParameters = new Dictionary<string, object?>
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "name", drillName }
        };

        var missing = await CallToolAsync("resiliency_drill_get", getParameters, resultProcessor: element => element);
        Assert.Equal(404, missing.AssertProperty("status").GetInt32());

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
                { "resource-group", resourceGroupName },
                { "drill-type", "Zonal" },
                { "rbac-setup-mode", "AutomatedBuiltinRoles" },
                { "recoveryplan", recoveryPlan },
                { "goal-assignment-properties", """{"identity":{"type":"SystemAssigned"}}""" },
                { "health-model-monitoring-properties", JsonSerializer.Serialize(new { identity = new { type = "SystemAssigned" }, healthModelId }) }
            });

            AssertConfiguration(result.AssertProperty("drill"), serviceGroup, drillName, recoveryPlan, healthModelId);
            var readBack = await CallToolAsync("resiliency_drill_get", getParameters);
            AssertConfiguration(readBack.AssertProperty("drill"), serviceGroup, drillName, recoveryPlan, healthModelId);
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
                var afterDelete = await CallToolAsync("resiliency_drill_get", getParameters, resultProcessor: element => element);
                Assert.Equal(404, afterDelete.AssertProperty("status").GetInt32());
            }
            catch (Exception cleanupFailure) when (failure is not null)
            {
                throw new AggregateException("The creation scenario and its cleanup both failed.", failure, cleanupFailure);
            }
        }
    }

    private static void AssertConfiguration(JsonElement drill, string serviceGroup, string drillName, string recoveryPlan, string healthModelId)
    {
        string scope = $"/providers/Microsoft.Management/serviceGroups/{serviceGroup}/providers/Microsoft.AzureResilienceManagement";
        Assert.Equal($"{scope}/drills/{drillName}", drill.AssertProperty("id").GetString(), ignoreCase: true);
        var properties = drill.AssertProperty("properties");
        Assert.Equal("Succeeded", properties.AssertProperty("provisioningState").GetString());
        Assert.Equal("NotRunning", properties.AssertProperty("executionState").GetString());
        Assert.Equal("Zonal", properties.AssertProperty("drillType").GetString());
        Assert.Equal($"{scope}/recoveryPlans/{recoveryPlan}",
            properties.AssertProperty("recoveryPlanProperties").AssertProperty("recoveryPlanId").GetString(), ignoreCase: true);
        Assert.Equal("SystemAssigned", properties.AssertProperty("goalAssignmentProperties").AssertProperty("identity").AssertProperty("type").GetString());
        var monitoring = properties.AssertProperty("healthModelMonitoringProperties");
        Assert.Equal(healthModelId, monitoring.AssertProperty("healthModelId").GetString(), ignoreCase: true);
        Assert.Equal("SystemAssigned", monitoring.AssertProperty("identity").AssertProperty("type").GetString());
    }
}
