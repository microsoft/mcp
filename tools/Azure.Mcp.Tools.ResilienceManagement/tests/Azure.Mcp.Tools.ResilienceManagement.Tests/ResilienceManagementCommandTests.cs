// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore DELETEASSETRESOURCEGROUPNAME DRILLRESOURCEID HEALTHMODELID LIFECYCLEENROLLMENTNAME LIFECYCLESERVICEGROUPNAME CREATIONSERVICEGROUPNAME PLANLIFECYCLESERVICEGROUPNAME WORKFLOWSERVICEGROUPNAME WORKFLOWRECOVERYPLANNAME WORKFLOWRECOVERYRESOURCEID

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
public class ResilienceManagementCommandTests(
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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");

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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");

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
    public async Task Should_get_drill()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
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
    public async Task Should_delete_drill()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "LIFECYCLESERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveVariable("drillName", $"mcp-delete-{Guid.NewGuid().ToString("N")[..8]}");
        var assetGroup = TestMode == Microsoft.Mcp.Tests.Helpers.TestMode.Playback ||
            Settings.DeploymentOutputs.ContainsKey("DELETEASSETRESOURCEGROUPNAME")
            ? RegisterOrRetrieveDeploymentOutputVariable("deleteAssetGroup", "DELETEASSETRESOURCEGROUPNAME")
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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var sourceLocation = RegisterOrRetrieveDeploymentOutputVariable("sourceLocation", "DRILLRUNSOURCELOCATION");

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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var drillResourceId = RegisterOrRetrieveDeploymentOutputVariable("drillResourceId", "DRILLRESOURCEID");

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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var drillResourceId = RegisterOrRetrieveDeploymentOutputVariable("drillResourceId", "DRILLRESOURCEID");
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

    [Fact]
    public async Task Should_start_and_end_drill()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        await WithDrillRunAsync(serviceGroup, drillName, run =>
        {
            Assert.True(Guid.TryParse(run, out _));
            return Task.CompletedTask;
        });
    }

    private async Task WithDrillRunAsync(string serviceGroup, string drill, Func<string, Task> scenario)
    {
        var existing = await GetDrillRunNamesAsync(serviceGroup, drill);
        bool started = false;
        Exception? failure = null;
        try
        {
            var start = await StartDrillAsync(serviceGroup, drill);
            started = true;
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

    private async Task<JsonElement> StartDrillAsync(string serviceGroup, string drillName)
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
                    { "mode", "Failover" }
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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLRUNNAME");

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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLRUNNAME");

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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLRUNNAME");
        var drillRunResource = RegisterOrRetrieveDeploymentOutputVariable("drillRunResourceName", "DRILLRUNRESOURCENAME");

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
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drill = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var drillRun = RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLRUNNAME");
        var drillRunResource = RegisterOrRetrieveDeploymentOutputVariable("drillRunResourceName", "DRILLRUNRESOURCENAME");

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
                "resiliency_recoveryplan_get",
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
    public async Task Should_add_or_update_drill_resources()
    {
        var serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "SERVICEGROUPNAME");
        var drillName = RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLNAME");
        var drillResourceId = RegisterOrRetrieveDeploymentOutputVariable("drillResourceId", "DRILLRESOURCEID");

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
        const string usagePlanName = "mcp-usage-plan";

        var result = await CallToolAsync(
            "resiliency_usageplan_create",
            new()
            {
                { "tenant", Settings.TenantId },
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "usage-plan", usagePlanName },
                { "plan-type", "Standard" }
            });

        var usagePlan = result.AssertProperty("usagePlan");
        Assert.False(string.IsNullOrEmpty(usagePlan.AssertProperty("name").GetString()));
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
