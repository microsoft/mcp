// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore gamcp GOALSSERVICEGROUPNAME STORAGEACCOUNTID

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Microsoft.Mcp.Tests;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests;

public partial class ResilienceManagementCommandTests
{
    [Fact]
    public async Task Should_manage_goal_assignment_ga_lifecycle()
    {
        string serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("goalsServiceGroup", "GOALSSERVICEGROUPNAME");
        string assignment = RegisterOrRetrieveVariable("goalsAssignment", $"gamcp{Guid.NewGuid():N}"[..24]);
        Dictionary<string, object?> parameters = new()
        {
            ["service-group"] = serviceGroup,
            ["goal-assignment"] = assignment,
            ["tenant"] = Settings.TenantId
        };

        JsonElement? missing = await CallToolAsync("resiliency_goal_assignment_get",
            new() { ["service-group"] = serviceGroup, ["name"] = assignment, ["tenant"] = Settings.TenantId },
            resultProcessor: root => root);
        Assert.Equal(404, missing.AssertProperty("status").GetInt32());

        try
        {
            var created = await CallGoalLifecycleToolAsync("create", new(parameters) { ["require-zonal-resiliency"] = true });
            AssertGoalRequirement(created, true);

            // The dedicated storage fixture has no Monitor SLI. Exercise explicit empty-list PATCH,
            // not nonempty SLI replacement, and independently verify the persisted GA state.
            var updated = await CallGoalLifecycleToolAsync("update", new(parameters) { ["clear-service-level-resources"] = true });
            AssertGoalRequirement(updated, true);
            var persisted = await CallToolAsync("resiliency_goal_assignment_get",
                new() { ["service-group"] = serviceGroup, ["name"] = assignment, ["tenant"] = Settings.TenantId });
            AssertGoalRequirement(persisted, true);
            Assert.Empty(persisted.AssertProperty("goalAssignment").AssertProperty("properties").AssertProperty("serviceLevelResources").EnumerateArray());

        }
        finally
        {
            var deleted = await CallGoalLifecycleToolAsync("delete", parameters);
            Assert.True(deleted.AssertProperty("deleted").GetBoolean());
        }

        var absent = await CallGoalLifecycleToolAsync("delete", parameters);
        Assert.False(absent.AssertProperty("deleted").GetBoolean());
    }

    [Fact]
    public async Task Should_manage_goal_resource_ga_lifecycle()
    {
        string serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("goalsServiceGroup", "GOALSSERVICEGROUPNAME");
        string resourceId = RegisterOrRetrieveDeploymentOutputVariable("goalsResourceId", "STORAGEACCOUNTID");
        string assignment = RegisterOrRetrieveVariable("goalsAssignment", $"gamcp{Guid.NewGuid():N}"[..24]);
        Dictionary<string, object?> parameters = new()
        {
            ["service-group"] = serviceGroup,
            ["goal-assignment"] = assignment,
            ["tenant"] = Settings.TenantId
        };
        JsonElement? missing = await CallToolAsync("resiliency_goal_assignment_get",
            new() { ["service-group"] = serviceGroup, ["name"] = assignment, ["tenant"] = Settings.TenantId },
            resultProcessor: root => root);
        Assert.Equal(404, missing.AssertProperty("status").GetInt32());
        try
        {
            AssertGoalRequirement(await CallGoalLifecycleToolAsync("create", new(parameters) { ["require-zonal-resiliency"] = true }), true);
            AssertGoalOperation(await CallGoalLifecycleToolAsync("refresh-resources", parameters));
            string member = await WaitForGoalMemberAsync(serviceGroup, assignment, resourceId);
            string excluded = CreateGoalResourceUpdate(resourceId, "Excluded");
            AssertGoalOperation(await CallGoalLifecycleToolAsync("update-resources", new(parameters) { ["resources"] = excluded }));
            await WaitForGoalParticipationAsync(serviceGroup, assignment, member, "Excluded");

            string included = CreateGoalResourceUpdate(resourceId, "Included");
            AssertGoalOperation(await CallGoalLifecycleToolAsync("update-resources", new(parameters) { ["resources"] = included }));
            await WaitForGoalParticipationAsync(serviceGroup, assignment, member, "Included");

        }
        finally
        {
            var deleted = await CallGoalLifecycleToolAsync("delete", parameters);
            Assert.True(deleted.AssertProperty("deleted").GetBoolean());
        }

        var absent = await CallGoalLifecycleToolAsync("delete", parameters);
        Assert.False(absent.AssertProperty("deleted").GetBoolean());
    }

    [Theory]
    [InlineData("update")]
    [InlineData("recommend-capacity")]
    [InlineData("refresh-resources")]
    [InlineData("update-resources")]
    public async Task Should_reject_missing_goal_assignment_ga(string operation)
    {
        string serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("goalsServiceGroup", "GOALSSERVICEGROUPNAME");
        string assignment = RegisterOrRetrieveVariable("missingGoalsAssignment", $"missing{Guid.NewGuid():N}"[..24]);
        Dictionary<string, object?> parameters = new()
        {
            ["service-group"] = serviceGroup,
            ["goal-assignment"] = assignment,
            ["tenant"] = Settings.TenantId
        };
        if (operation == "update")
        {
            parameters["clear-service-level-resources"] = true;
        }
        else if (operation == "update-resources")
        {
            string resourceId = RegisterOrRetrieveDeploymentOutputVariable("goalsResourceId", "STORAGEACCOUNTID");
            parameters["resources"] = CreateGoalResourceUpdate(resourceId, "Excluded");
        }

        JsonElement? response = await CallToolAsync($"resiliency_goal_assignment_{operation}", parameters, resultProcessor: root => root);
        Assert.Equal(404, response.AssertProperty("status").GetInt32());
        Assert.False(response!.Value.TryGetProperty("results", out var results) && results.ValueKind != JsonValueKind.Null);
    }

    [Fact]
    public async Task Should_recommend_goal_capacity_ga()
    {
        string serviceGroup = RegisterOrRetrieveDeploymentOutputVariable("goalsServiceGroup", "GOALSSERVICEGROUPNAME");
        string resourceId = RegisterOrRetrieveDeploymentOutputVariable("goalsResourceId", "STORAGEACCOUNTID");
        string assignment = RegisterOrRetrieveVariable("goalsAssignment", $"gamcp{Guid.NewGuid():N}"[..24]);
        Dictionary<string, object?> parameters = new()
        {
            ["service-group"] = serviceGroup,
            ["goal-assignment"] = assignment,
            ["tenant"] = Settings.TenantId
        };
        JsonElement? missing = await CallToolAsync("resiliency_goal_assignment_get",
            new() { ["service-group"] = serviceGroup, ["name"] = assignment, ["tenant"] = Settings.TenantId },
            resultProcessor: root => root);
        Assert.Equal(404, missing.AssertProperty("status").GetInt32());
        try
        {
            AssertGoalRequirement(await CallGoalLifecycleToolAsync("create", new(parameters) { ["require-zonal-resiliency"] = true }), true);
            AssertGoalOperation(await CallGoalLifecycleToolAsync("recommend-capacity",
                new(parameters) { ["resource-ids"] = new[] { resourceId } }));
        }
        finally
        {
            Assert.True((await CallGoalLifecycleToolAsync("delete", parameters)).AssertProperty("deleted").GetBoolean());
        }
    }

    private async Task<JsonElement?> CallGoalLifecycleToolAsync(string operation, Dictionary<string, object?> parameters)
    {
        for (int attempt = 0; attempt < 60; attempt++)
        {
            JsonElement? response = await CallToolAsync($"resiliency_goal_assignment_{operation}", parameters, resultProcessor: root => root);
            int status = response.AssertProperty("status").GetInt32();
            if (status != 409)
            {
                Assert.True(status == 200,
                    $"Goal assignment operation '{operation}' returned HTTP {status}: {response.AssertProperty("message").GetString()}");
                return response.AssertProperty("results");
            }

            await Task.Delay(PollInterval(5000), TestContext.Current.CancellationToken);
        }

        throw new TimeoutException($"The goal assignment remained busy during {operation}.");
    }

    private static void AssertGoalRequirement(JsonElement? result, bool expected)
    {
        JsonElement assignment = result.AssertProperty("goalAssignment");
        Assert.False(string.IsNullOrWhiteSpace(assignment.AssertProperty("id").GetString()));
        JsonElement properties = assignment.AssertProperty("properties");
        Assert.Equal(expected, properties.AssertProperty("requireZonalResiliency").GetBoolean());
        Assert.Equal("Succeeded", properties.AssertProperty("provisioningState").GetString());
    }

    private static void AssertGoalOperation(JsonElement? result)
    {
        JsonElement operation = result.AssertProperty("operation");
        bool completed = operation.AssertProperty("hasCompleted").GetBoolean();
        Assert.Equal(completed ? "Succeeded" : "Accepted", operation.AssertProperty("status").GetString());
        if (!completed)
        {
            Assert.True(Guid.TryParseExact(operation.AssertProperty("operationId").GetString(), "D", out _));
        }
    }

    private static string CreateGoalResourceUpdate(string resourceId, string participation) =>
        new JsonArray(new JsonObject
        {
            ["properties"] = new JsonObject
            {
                ["resourceArmId"] = resourceId,
                ["zonalResiliency"] = new JsonObject { ["goalParticipation"] = participation }
            }
        }).ToJsonString();

    private async Task<string> WaitForGoalMemberAsync(string serviceGroup, string assignment, string resourceId)
    {
        for (int attempt = 0; attempt < 60; attempt++)
        {
            var resources = await CallToolAsync("resiliency_goal_resource_get",
                new() { ["service-group"] = serviceGroup, ["goal-assignment"] = assignment, ["tenant"] = Settings.TenantId });
            foreach (JsonElement summary in resources.AssertProperty("goalResources").EnumerateArray())
            {
                string name = new ResourceIdentifier(summary.AssertProperty("id").GetString()!).Name;
                var member = await GetGoalMemberAsync(serviceGroup, assignment, name);
                if (string.Equals(member.AssertProperty("properties").AssertProperty("resourceArmId").GetString(),
                    resourceId, StringComparison.OrdinalIgnoreCase))
                {
                    return name;
                }
            }

            await Task.Delay(PollInterval(5000), TestContext.Current.CancellationToken);
        }

        throw new TimeoutException("The dedicated goal fixture resource was not discovered.");
    }

    private async Task<JsonElement> GetGoalMemberAsync(string serviceGroup, string assignment, string member)
    {
        var result = await CallToolAsync("resiliency_goal_resource_get", new()
        {
            ["service-group"] = serviceGroup,
            ["goal-assignment"] = assignment,
            ["name"] = member,
            ["tenant"] = Settings.TenantId
        });
        return result.AssertProperty("goalResource");
    }

    private async Task WaitForGoalParticipationAsync(string serviceGroup, string assignment, string member, string participation)
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            JsonElement resource = await GetGoalMemberAsync(serviceGroup, assignment, member);
            JsonElement zonal = resource.AssertProperty("properties").AssertProperty("zonalResiliency");
            if (zonal.AssertProperty("goalParticipation").GetString() == participation)
            {
                Assert.Equal("Succeeded", resource.AssertProperty("properties").AssertProperty("provisioningState").GetString());
                return;
            }

            await Task.Delay(PollInterval(5000), TestContext.Current.CancellationToken);
        }

        throw new TimeoutException("The requested goal participation did not persist.");
    }
}
