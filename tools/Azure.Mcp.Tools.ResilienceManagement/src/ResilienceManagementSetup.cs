// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Templates;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Jobs;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Jobs.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans.Enrollments;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResilienceManagement;

public class ResilienceManagementSetup : IAreaSetup
{
    public string Name => "resilience";

    public string Title => "Azure Resilience Management";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IResilienceManagementService, ResilienceManagementService>();

        services.AddSingleton<GoalTemplateGetCommand>();
        services.AddSingleton<GoalAssignmentGetCommand>();
        services.AddSingleton<GoalResourceGetCommand>();
        services.AddSingleton<UsagePlanGetCommand>();
        services.AddSingleton<UsagePlanEnrollmentGetCommand>();
        services.AddSingleton<RecoveryPlanGetCommand>();
        services.AddSingleton<RecoveryResourceGetCommand>();
        services.AddSingleton<RecoveryJobGetCommand>();
        services.AddSingleton<RecoveryJobResourceGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var resilienceManagement = new CommandGroup(Name,
            """
            Azure Resilience Management operations - Commands for working with resilience goals and goal
            templates for Azure service groups. Use this tool to list the resilience goal templates available
            for a service group, including goal type, provisioning state, recovery point and time objectives,
            and high availability and disaster recovery requirements.
            """,
            Title);

        // Create goal subgroup
        var goals = new CommandGroup("goal", "Resilience goal operations - Commands for working with resilience goals and goal templates for Azure service groups.");
        resilienceManagement.AddSubGroup(goals);

        // Create template subgroup under goal
        var templates = new CommandGroup("template", "Resilience goal template operations - Commands for listing resilience goal templates for Azure service groups.");
        goals.AddSubGroup(templates);

        // Create assignment subgroup under goal
        var assignments = new CommandGroup("assignment", "Resilience goal assignment operations - Commands for listing resilience goal assignments for Azure service groups.");
        goals.AddSubGroup(assignments);

        // Create resource subgroup under goal
        var goalResources = new CommandGroup("resource", "Resilience goal resource operations - Commands for listing the resources (members) of a resilience goal assignment.");
        goals.AddSubGroup(goalResources);

        // Register commands
        templates.AddCommand<GoalTemplateGetCommand>(serviceProvider);
        assignments.AddCommand<GoalAssignmentGetCommand>(serviceProvider);
        goalResources.AddCommand<GoalResourceGetCommand>(serviceProvider);

        // Create usageplan subgroup
        var usagePlans = new CommandGroup("usageplan", "Resilience usage plan operations - Commands for listing resilience usage plans in an Azure resource group.");
        resilienceManagement.AddSubGroup(usagePlans);

        usagePlans.AddCommand<UsagePlanGetCommand>(serviceProvider);

        // Create enrollment subgroup under usageplan
        var enrollments = new CommandGroup("enrollment", "Resilience usage plan enrollment operations - Commands for listing enrollments of a resilience usage plan.");
        usagePlans.AddSubGroup(enrollments);

        enrollments.AddCommand<UsagePlanEnrollmentGetCommand>(serviceProvider);

        // Create recovery subgroup with a plan subgroup
        var recovery = new CommandGroup("recovery", "Resilience recovery operations - Commands for working with resilience recovery plans for an Azure service group.");
        resilienceManagement.AddSubGroup(recovery);

        var recoveryPlans = new CommandGroup("plan", "Resilience recovery plan operations - Commands for listing and getting resilience recovery plans for an Azure service group.");
        recovery.AddSubGroup(recoveryPlans);

        recoveryPlans.AddCommand<RecoveryPlanGetCommand>(serviceProvider);

        // Create resource subgroup under recovery plan
        var recoveryResources = new CommandGroup("resource", "Resilience recovery resource operations - Commands for listing and getting the resources (members) of a resilience recovery plan.");
        recoveryPlans.AddSubGroup(recoveryResources);

        recoveryResources.AddCommand<RecoveryResourceGetCommand>(serviceProvider);

        // Create job subgroup under recovery
        var recoveryJobs = new CommandGroup("job", "Resilience recovery job operations - Commands for listing and getting the recovery jobs of a resilience recovery plan.");
        recovery.AddSubGroup(recoveryJobs);

        recoveryJobs.AddCommand<RecoveryJobGetCommand>(serviceProvider);

        // Create resource subgroup under recovery job
        var recoveryJobResources = new CommandGroup("resource", "Resilience recovery job resource operations - Commands for listing and getting the resources (targets) of a resilience recovery job.");
        recoveryJobs.AddSubGroup(recoveryJobResources);

        recoveryJobResources.AddCommand<RecoveryJobResourceGetCommand>(serviceProvider);

        return resilienceManagement;
    }
}
