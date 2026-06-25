// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests;

// Live (recorded) tests for the Resilience Management toolset.
//
// All resource names are derived from Settings.ResourceBaseName (AzureBackup-style), so the only
// requirement before recording is to provision resources whose names match these helpers:
//
//   service group        -> {ResourceBaseName}-sg          (tenant-scoped, create out-of-band)
//   goal template        -> {ResourceBaseName}-gt
//   goal assignment      -> {ResourceBaseName}-ga
//   drill                -> {ResourceBaseName}-drill
//   recovery plan        -> {ResourceBaseName}-rp
//   usage plan           -> {ResourceBaseName}             (created by test-resources.bicep)
//   usage plan (create)  -> {ResourceBaseName}-up2
//   enrollment           -> {ResourceBaseName}-enr
//
// The service-group family (goal templates/assignments/resources, drills, recovery plans) targets a
// tenant-level Microsoft.Management/serviceGroups resource that the resource-group-scoped
// test-resources.bicep cannot create. Provision that service group and its children out-of-band before
// recording. The "create" commands use CreateOrUpdate semantics, so re-recording with the same names
// is safe.
public class ResilienceManagementCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private string ServiceGroup => $"{Settings.ResourceBaseName}-sg";
    private string GoalTemplate => $"{Settings.ResourceBaseName}-gt";
    private string GoalAssignment => $"{Settings.ResourceBaseName}-ga";
    // A service group can hold only one goal assignment, and ServiceGroup already has GoalAssignment.
    // The create-assignment test therefore targets a dedicated empty service group + its own template.
    private string GoalAssignmentServiceGroup => $"{Settings.ResourceBaseName}-sg3";
    private string GoalAssignmentTemplate => $"{Settings.ResourceBaseName}-gt3";
    private string GoalAssignmentToCreate => $"{Settings.ResourceBaseName}-ga2";
    private string Drill => $"{Settings.ResourceBaseName}-drill";
    private string RecoveryPlan => $"{Settings.ResourceBaseName}-rp";
    private string UsagePlan => Settings.ResourceBaseName;
    private string UsagePlanToCreate => $"{Settings.ResourceBaseName}-up2";
    private string Enrollment => $"{Settings.ResourceBaseName}-enr";

    // The deploy script names the resource group "{accountName}-{ResourceBaseName}" (e.g. "alias-mcp1234").
    // The default ResourceBaseName sanitizer only replaces the base-name part, leaving the account-name
    // prefix (the recording user's alias) in response bodies (resource ids, systemData). Strip that prefix
    // so the full resource group name is sanitized everywhere.
    private string AccountPrefix
    {
        get
        {
            var rg = Settings.ResourceGroupName;
            var suffix = $"-{Settings.ResourceBaseName}";
            return rg.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ? rg[..^suffix.Length] : rg;
        }
    }

    // Static regex (no Settings needed), so this can use the field-initializer form that the base class
    // appends its default ResourceBaseName/SubscriptionId sanitizers to. Scrubs the recording user's UPN
    // that the service returns in createdBy/lastModifiedBy for the create commands.
    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } =
    [
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = @"[A-Za-z0-9._%+-]+@microsoft\.com",
            Value = "sanitized@example.com",
        }),
    ];

    // Computed (depends on Settings, available at sanitizer-apply time). The base class never mutates
    // BodyRegexSanitizers, so the computed form is safe here. Matches the account-name prefix, which
    // survives the base-name replacement regardless of sanitizer ordering.
    public override List<BodyRegexSanitizer> BodyRegexSanitizers =>
    [
        .. base.BodyRegexSanitizers,
        new BodyRegexSanitizer(new BodyRegexSanitizerBody()
        {
            Regex = System.Text.RegularExpressions.Regex.Escape(AccountPrefix),
            Value = "Sanitized",
        }),
    ];

    // Goal-resource members have server-assigned GUID names. The default name sanitizer rewrites that
    // GUID to "Sanitized" in the list response body, but leaves it raw in the goal_resource_get request
    // URL. This sanitizer rewrites the goalResources/<member> URI segment to "Sanitized" as well, so the
    // recorded GET URL matches what the test requests during playback (where the list returns "Sanitized").
    public override List<UriRegexSanitizer> UriRegexSanitizers =>
    [
        .. base.UriRegexSanitizers,
        // Replace the entire resourceGroups/<name> URI segment with "Sanitized". The default base-name
        // sanitizer only scrubs the base-name part, leaving the account-name prefix (the recording user's
        // alias) as "alias-Sanitized" in request URLs. Matching the whole segment is order-independent and
        // ensures record and playback (where ResourceGroupName is "Sanitized") use the same URL.
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "resource[Gg]roups/(?<rg>[^/?]+)",
            Value = "Sanitized",
            GroupForReplace = "rg",
        }),
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "goalResources/(?<member>[^/?]+)",
            Value = "Sanitized",
            GroupForReplace = "member",
        }),
        // Long-running create operations poll operationStatuses URLs that carry rotating signed query
        // params (t=<timestamp>&c=<cert>&s=...&h=...). These differ per poll and break playback matching.
        // Strip everything after the api-version value so every poll collapses to one identical URL.
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "operationStatuses/[^?]+\\?api-version=[^&]+(?<sig>&.*)$",
            Value = "",
            GroupForReplace = "sig",
        }),
    ];

    // ---------------------------------------------------------------------------------------------
    // Usage plans (resource-group scoped; provisioned by test-resources.bicep)
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task Should_list_usage_plans_by_subscription()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_usageplan_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var usagePlans = result.AssertProperty("usagePlans");
        Assert.Equal(JsonValueKind.Array, usagePlans.ValueKind);
    }

    [Fact]
    public async Task Should_list_usage_plans_by_resource_group()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_usageplan_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var usagePlans = result.AssertProperty("usagePlans");
        Assert.Equal(JsonValueKind.Array, usagePlans.ValueKind);
    }

    [Fact]
    public async Task Should_get_usage_plan()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_usageplan_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "usage-plan", UsagePlan }
            });

        var usagePlan = result.AssertProperty("usagePlan");
        Assert.Equal(JsonValueKind.Object, usagePlan.ValueKind);
        // A default $..name sanitizer replaces the name with "Sanitized" in playback recordings.
        Assert.Equal(TestMode == Microsoft.Mcp.Tests.Helpers.TestMode.Playback ? "Sanitized" : UsagePlan, usagePlan.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Should_create_usage_plan()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_usageplan_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "usage-plan", UsagePlanToCreate },
                { "plan-type", "Basic" }
            });

        var usagePlan = result.AssertProperty("usagePlan");
        Assert.Equal(JsonValueKind.Object, usagePlan.ValueKind);
        // A default $..name sanitizer replaces the name with "Sanitized" in playback recordings.
        Assert.Equal(TestMode == Microsoft.Mcp.Tests.Helpers.TestMode.Playback ? "Sanitized" : UsagePlanToCreate, usagePlan.GetProperty("name").GetString());
    }

    // ---------------------------------------------------------------------------------------------
    // Usage plan enrollments
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task Should_list_usage_plan_enrollments()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_usageplan_enrollment_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "usage-plan", UsagePlan }
            });

        var enrollments = result.AssertProperty("enrollments");
        Assert.Equal(JsonValueKind.Array, enrollments.ValueKind);
    }

    [Fact]
    public async Task Should_get_usage_plan_enrollment()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_usageplan_enrollment_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "usage-plan", UsagePlan },
                { "enrollment", Enrollment }
            });

        var enrollment = result.AssertProperty("enrollment");
        Assert.Equal(JsonValueKind.Object, enrollment.ValueKind);
    }

    [Fact]
    public async Task Should_create_usage_plan_enrollment()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_usageplan_enrollment_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "usage-plan", UsagePlan },
                { "enrollment", Enrollment },
                { "service-group", ServiceGroup }
            });

        var enrollment = result.AssertProperty("enrollment");
        Assert.Equal(JsonValueKind.Object, enrollment.ValueKind);
    }

    // ---------------------------------------------------------------------------------------------
    // Goal templates (service-group scoped; service group created out-of-band)
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task Should_list_goal_templates()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_goal_template_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup }
            });

        var goalTemplates = result.AssertProperty("goalTemplates");
        Assert.Equal(JsonValueKind.Array, goalTemplates.ValueKind);
    }

    [Fact]
    public async Task Should_get_goal_template()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_goal_template_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup },
                { "goal-template", GoalTemplate }
            });

        var goalTemplate = result.AssertProperty("goalTemplate");
        Assert.Equal(JsonValueKind.Object, goalTemplate.ValueKind);
    }

    [Fact]
    public async Task Should_create_goal_template()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_goal_template_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup },
                { "goal-template", GoalTemplate },
                { "goal-type", "Resiliency" },
                { "require-high-availability", "Required" },
                { "require-disaster-recovery", "NotRequired" },
                { "regional-recovery-point-objective", "PT15M" },
                { "regional-recovery-time-objective", "PT30M" }
            });

        var goalTemplate = result.AssertProperty("goalTemplate");
        Assert.Equal(JsonValueKind.Object, goalTemplate.ValueKind);
    }

    // ---------------------------------------------------------------------------------------------
    // Goal assignments
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task Should_list_goal_assignments()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_goal_assignment_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup }
            });

        var goalAssignments = result.AssertProperty("goalAssignments");
        Assert.Equal(JsonValueKind.Array, goalAssignments.ValueKind);
    }

    [Fact]
    public async Task Should_get_goal_assignment()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_goal_assignment_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup },
                { "goal-assignment", GoalAssignment }
            });

        var goalAssignment = result.AssertProperty("goalAssignment");
        Assert.Equal(JsonValueKind.Object, goalAssignment.ValueKind);
    }

    [Fact]
    public async Task Should_create_goal_assignment()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_goal_assignment_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", GoalAssignmentServiceGroup },
                { "goal-assignment", GoalAssignmentToCreate },
                { "goal-template", GoalAssignmentTemplate },
                { "goal-assignment-type", "Resiliency" }
            });

        var goalAssignment = result.AssertProperty("goalAssignment");
        Assert.Equal(JsonValueKind.Object, goalAssignment.ValueKind);
    }

    // ---------------------------------------------------------------------------------------------
    // Goal resources (members of a goal assignment)
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task Should_list_goal_resources()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_goal_resource_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup },
                { "goal-assignment", GoalAssignment }
            });

        var goalResources = result.AssertProperty("goalResources");
        Assert.Equal(JsonValueKind.Array, goalResources.ValueKind);
    }

    [Fact]
    public async Task Should_get_goal_resource()
    {
        // The goal resource name is a member of the assignment, discovered from the list call.
        var listResult = await CallToolAsync(
            "resiliencemanagement_goal_resource_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup },
                { "goal-assignment", GoalAssignment }
            });

        var goalResources = listResult.AssertProperty("goalResources");
        Assert.Equal(JsonValueKind.Array, goalResources.ValueKind);

        var first = goalResources.EnumerateArray().FirstOrDefault();
        Assert.Equal(JsonValueKind.Object, first.ValueKind);
        var name = first.GetProperty("name").GetString();
        Assert.NotNull(name);

        var result = await CallToolAsync(
            "resiliencemanagement_goal_resource_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup },
                { "goal-assignment", GoalAssignment },
                { "name", name! }
            });

        var goalResource = result.AssertProperty("goalResource");
        Assert.Equal(JsonValueKind.Object, goalResource.ValueKind);
    }

    // ---------------------------------------------------------------------------------------------
    // Drills
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task Should_list_drills()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_drill_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup }
            });

        var drills = result.AssertProperty("drills");
        Assert.Equal(JsonValueKind.Array, drills.ValueKind);
    }

    [Fact]
    public async Task Should_get_drill()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_drill_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup },
                { "drill", Drill }
            });

        var drill = result.AssertProperty("drill");
        Assert.Equal(JsonValueKind.Object, drill.ValueKind);
    }

    // ---------------------------------------------------------------------------------------------
    // Recovery plans
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task Should_list_recovery_plans()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_recovery_plan_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup }
            });

        var recoveryPlans = result.AssertProperty("recoveryPlans");
        Assert.Equal(JsonValueKind.Array, recoveryPlans.ValueKind);
    }

    [Fact]
    public async Task Should_get_recovery_plan()
    {
        var result = await CallToolAsync(
            "resiliencemanagement_recovery_plan_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "service-group", ServiceGroup },
                { "recovery-plan", RecoveryPlan }
            });

        var recoveryPlan = result.AssertProperty("recoveryPlan");
        Assert.Equal(JsonValueKind.Object, recoveryPlan.ValueKind);
    }
}
