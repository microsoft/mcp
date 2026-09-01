// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore LIFECYCLESERVICEGROUPNAME PLANLIFECYCLESERVICEGROUPNAME

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests;

/// <summary>
/// Live / recorded integration tests for the Resilience Management toolset.
/// Resources are provisioned by test-resources.bicep + test-resources-post.ps1.
/// </summary>
public class ResilienceManagementCommandTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // Preserve LRO Location paths during recording; replacing the entire header breaks polling playback.
    public override List<string> DisabledDefaultSanitizers =>
    [
        .. base.DisabledDefaultSanitizers,
        "AZSDK2003"
    ];

    // Prepend the base sanitizers (e.g. WWW-Authenticate) then add tool-specific ones.
    // Sanitize the required per-invocation operation-id request GUID for playback matching and the
    // x-ms-operation-identifier response header, which contains the real tenant ID and object ID.
    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
    [
        .. base.HeaderRegexSanitizers,
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("x-ms-operation-identifier")
        {
            Value = "sanitized"
        }),
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("operation-id")
        {
            Value = "sanitized"
        }),
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("Location")
        {
            Regex = "([?&](?:t|c|s|h)=)(?<value>[^&]+)",
            GroupForReplace = "value",
            Value = "sanitized"
        })
    ];

    public override List<UriRegexSanitizer> UriRegexSanitizers =>
    [
        .. base.UriRegexSanitizers,
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "([?&](?:t|c|s|h)=)(?<value>[^&]+)",
            GroupForReplace = "value",
            Value = "sanitized"
        }),
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = @"resource[Gg]roups/([^?\\/]+)",
            GroupForReplace = "1",
            Value = "Sanitized"
        })
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..subscription")
        {
            Value = "Sanitized"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..healthModelId")
        {
            Value = "Sanitized"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..chaosExperimentId")
        {
            Value = "Sanitized"
        })
    ];

    [Fact]
    public async Task Should_get_usage_plan()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var usagePlanName = RegisterOrRetrieveDeploymentOutputVariable("usagePlanName", "USAGEPLANNAME");

        var result = await CallToolAsync(
            "resilience_usageplan_get",
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
            "resilience_usageplan_enrollment_get",
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
    public async Task Should_get_goal_template()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var goalTemplate = RegisterOrRetrieveDeploymentOutputVariable("goalTemplateName", "GOALTEMPLATENAME");

        var result = await CallToolAsync(
            "resilience_goal_template_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "name", goalTemplate }
            });

        var template = result.AssertProperty("goalTemplate");
        Assert.False(string.IsNullOrEmpty(template.AssertProperty("name").GetString()));
    }

    [Fact]
    public async Task Should_get_goal_assignment()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var goalAssignment = RegisterOrRetrieveDeploymentOutputVariable("goalAssignmentName", "GOALASSIGNMENTNAME");

        var result = await CallToolAsync(
            "resilience_goal_assignment_get",
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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");

        var result = await CallToolAsync(
            "resilience_drill_get",
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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");

        var result = await CallToolAsync(
            "resilience_drill_update",
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
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");

        var result = await CallToolAsync(
            "resilience_drill_create",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drillName },
                { "subscription", Settings.SubscriptionId },
                { "region", "westus2" },
                { "resource-group", resourceGroupName },
                { "drill-type", "Zonal" },
                { "rbac-setup-mode", "AutomatedBuiltinRoles" },
                { "recoveryplan", recoveryPlan }
            });

        var drill = result.AssertProperty("drill");
        Assert.EndsWith(drillName, drill.AssertProperty("id").GetString(), StringComparison.OrdinalIgnoreCase);
        var properties = drill.AssertProperty("properties");
        Assert.Equal("Succeeded", properties.AssertProperty("provisioningState").GetString());
        Assert.Equal("Zonal", properties.AssertProperty("drillType").GetString());
        var recoveryPlanId = properties.AssertProperty("recoveryPlanProperties").AssertProperty("recoveryPlanId").GetString();
        Assert.EndsWith($"/recoveryPlans/{recoveryPlan}", recoveryPlanId, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_delete_drill()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "LIFECYCLESERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DELETEDRILLNAME");

        var result = await CallToolAsync(
            "resilience_drill_delete",
            new()
            {
                { "service-group", serviceGroup },
                { "drill", drillName }
            });

        Assert.True(result.AssertProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Should_list_drill_resources()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");

        var result = await CallToolAsync(
            "resilience_drill_resource_get",
            new()
            {
                { "service-group", serviceGroup },
                { "drill", drillName }
            });

        Assert.Equal(JsonValueKind.Array, result.AssertProperty("drillResources").ValueKind);
    }

    [Fact]
    public async Task Should_get_drill_resource()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");

        var listResult = await CallToolAsync(
            "resilience_drill_resource_get",
            new()
            {
                { "service-group", serviceGroup },
                { "drill", drillName }
            });

        var drillResources = listResult.AssertProperty("drillResources");
        Assert.NotEqual(0, drillResources.GetArrayLength());
        var drillResourceName = RegisterOrRetrieveVariable(
            "drillResourceName",
            drillResources.EnumerateArray().First().AssertProperty("name").GetString()!);

        var result = await CallToolAsync(
            "resilience_drill_resource_get",
            new()
            {
                { "service-group", serviceGroup },
                { "drill", drillName },
                { "name", drillResourceName }
            });

        Assert.Equal(JsonValueKind.Object, result.AssertProperty("drillResource").ValueKind);
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_start_and_end_drill()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        bool startAccepted = false;

        try
        {
            var startResult = await StartDrillAsync(serviceGroup, drillName);
            startAccepted = true;

            Assert.False(string.IsNullOrEmpty(startResult.AssertProperty("operationId").GetString()));
            Assert.Equal("Accepted", startResult.AssertProperty("status").GetString());

            var endResult = await EndDrillAsync(serviceGroup, drillName);
            startAccepted = false;

            Assert.False(string.IsNullOrEmpty(endResult.AssertProperty("operationId").GetString()));
            Assert.Equal("Accepted", endResult.AssertProperty("status").GetString());
        }
        finally
        {
            if (startAccepted)
            {
                await EndDrillAsync(serviceGroup, drillName);
            }
        }
    }

    private async Task<JsonElement> StartDrillAsync(string serviceGroup, string drillName)
    {
        const int maxAttempts = 36;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            JsonElement? response = await CallToolAsync(
                "resilience_drill_start",
                new()
                {
                    { "service-group", serviceGroup },
                    { "drill", drillName },
                    { "mode", "TestFailover" }
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

    private async Task<JsonElement> EndDrillAsync(string serviceGroup, string drillName)
    {
        const int maxAttempts = 36;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            JsonElement? response = await CallToolAsync(
                "resilience_drill_end",
                new()
                {
                    { "service-group", serviceGroup },
                    { "drill", drillName },
                    { "attestation", "Success" },
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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLRUNNAME");

        var result = await CallToolAsync(
            "resilience_drill_run_get",
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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLRUNNAME");

        var result = await CallToolAsync(
            "resilience_drill_run_get",
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
    public async Task Should_list_drill_run_resources()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLRUNNAME");
        var drillRunResource = RegisterOrRetrieveDeploymentOutputVariable("drillRunResourceName", "DRILLRUNRESOURCENAME");

        var result = await CallToolAsync(
            "resilience_drill_run_resource_get",
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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLRUNNAME");
        var drillRunResource = RegisterOrRetrieveDeploymentOutputVariable("drillRunResourceName", "DRILLRUNRESOURCENAME");

        var result = await CallToolAsync(
            "resilience_drill_run_resource_get",
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
            "resilience_goal_resource_get",
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
            "resilience_recoveryplan_get",
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
            "resilience_recoveryplan_get",
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
            "resilience_recoveryplan_create",
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
            "resilience_recoveryplan_checkreadiness",
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
        bool recoveryPlanExists = false;

        try
        {
            var createResult = await CallToolAsync(
                "resilience_recoveryplan_create",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "recoveryplan", recoveryPlan },
                    { "plan-type", "Zonal" },
                    { "plan-description", "Recovery plan lifecycle test." },
                    { "identity-type", "SystemAssigned" },
                    { "default-group-description", "Lifecycle default group" }
                });
            recoveryPlanExists = true;

            var createdPlan = createResult.AssertProperty("recoveryPlan");
            Assert.EndsWith($"/recoveryPlans/{recoveryPlan}", createdPlan.AssertProperty("id").GetString());
            Assert.Equal("SystemAssigned", createdPlan.AssertProperty("identity").AssertProperty("type").GetString());
            var createdDefaultGroup = createdPlan.AssertProperty("defaultGroup");
            var defaultGroupId = createdDefaultGroup.AssertProperty("groupUniqueId").GetString();
            Assert.False(string.IsNullOrEmpty(defaultGroupId));
            Assert.Equal("Lifecycle default group", createdDefaultGroup.AssertProperty("description").GetString());

            var getResult = await CallToolAsync(
                "resilience_recoveryplan_get",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "name", recoveryPlan }
                });
            Assert.EndsWith(
                $"/recoveryPlans/{recoveryPlan}",
                getResult.AssertProperty("recoveryPlan").AssertProperty("id").GetString());

            var updateResult = await CallToolAsync(
                "resilience_recoveryplan_create",
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
                "resilience_recoveryplan_delete",
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
                "resilience_recoveryplan_delete",
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
                    "resilience_recoveryplan_delete",
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
                "resilience_recoveryplan_create",
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

            var finalizeResult = await CallToolAsync(
                "resilience_recoveryplan_finalize",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "recoveryplan", recoveryPlan }
                });

            Assert.True(Guid.TryParse(finalizeResult.AssertProperty("operationId").GetString(), out _));

            var getResult = await CallToolAsync(
                "resilience_recoveryplan_get",
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
                    "resilience_recoveryplan_delete",
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
            "resilience_recoveryplan_resource_get",
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
            "resilience_recoveryplan_resource_get",
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
            "resilience_recoveryplan_resource_get",
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
            "resilience_recoveryplan_resource_update",
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
            "resilience_recoveryplan_resource_get",
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
    public async Task Should_validate_recovery_plan_for_failover()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");
        var listedResources = await CallToolAsync(
            "resilience_recoveryplan_resource_get",
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
                "resilience_recoveryplan_resource_get",
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
            "resilience_recoveryplan_validateforfailover",
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
            "resilience_recoveryplan_validateforreprotect",
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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");

        var result = await CallToolAsync(
            "resilience_recoveryplan_validateforoperation",
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
    [InlineData("resilience_recoveryplan_failover")]
    [InlineData("resilience_recoveryplan_finalize")]
    [InlineData("resilience_recoveryplan_reprotect")]
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
        if (toolName == "resilience_recoveryplan_failover")
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
    [InlineData("resilience_recoveryjob_retry")]
    [InlineData("resilience_recoveryjob_resume")]
    public async Task Should_reject_recovery_job_action_when_job_does_not_exist(string toolName)
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");
        var parameters = new Dictionary<string, object?>
        {
            { "tenant", Settings.TenantId },
            { "service-group", serviceGroup },
            { "recoveryplan", recoveryPlan },
            { "recovery-job", "22222222-2222-2222-2222-222222222222" }
        };
        if (toolName == "resilience_recoveryjob_resume")
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
    public async Task Should_get_recovery_job()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");

        var listResult = await CallToolAsync(
            "resilience_recoveryjob_get",
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
            "resilience_recoveryjob_get",
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

    [Fact]
    public async Task Should_list_recovery_job_resources()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var recoveryPlan = RegisterOrRetrieveDeploymentOutputVariable("recoveryPlanName", "RECOVERYPLANNAME");

        var listResult = await CallToolAsync(
            "resilience_recoveryjob_get",
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
            "resilience_recoveryjob_resource_get",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "recoveryplan", recoveryPlan },
                { "recovery-job", recoveryJob }
            });

        Assert.Equal(JsonValueKind.Array, result.AssertProperty("recoveryJobResources").ValueKind);
    }

    [Fact]
    public async Task Should_create_usage_plan()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("createResourceGroupName", Settings.ResourceGroupName);
        const string usagePlanName = "mcp-usage-plan";

        var result = await CallToolAsync(
            "resilience_usageplan_create",
            new()
            {
                { "tenant", Settings.TenantId },
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "usage-plan", usagePlanName },
                { "plan-type", "Basic" }
            });

        var usagePlan = result.AssertProperty("usagePlan");
        Assert.False(string.IsNullOrEmpty(usagePlan.AssertProperty("name").GetString()));
    }

    [Fact]
    public async Task Should_create_usage_plan_enrollment()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var usagePlanName = RegisterOrRetrieveDeploymentOutputVariable("usagePlanName", "USAGEPLANNAME");
        var enrollmentName = RegisterOrRetrieveDeploymentOutputVariable("enrollmentName", "ENROLLMENTNAME");

        var result = await CallToolAsync(
            "resilience_usageplan_enrollment_create",
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
}
