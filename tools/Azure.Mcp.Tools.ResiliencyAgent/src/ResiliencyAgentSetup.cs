// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Architecture;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Arm;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Bicep;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.File;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Guidance;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Goals;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Iac;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.ServiceGroup;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Terraform;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.UsagePlan;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.ResiliencyAgent;

/// <summary>
/// Exposes the Azure Resiliency Agent as an MCP tool.
/// </summary>
public sealed class ResiliencyAgentSetup : IAreaSetup
{
    public string Name => "resiliencyagent";

    public string Title => "Azure Resiliency Agent";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient(
                ResiliencyAgentService.HttpClientName,
                static client => client.Timeout = ResiliencyAgentService.RequestTimeout)
            .AddHttpMessageHandler(serviceProvider =>
                new AccessTokenHandler(
                    serviceProvider.GetRequiredService<IAzureTokenCredentialProvider>(),
                    [ResiliencyAgentService.Scope]));
        services.AddHttpClient(
                DataBoundaryResolver.HttpClientName,
                static client => client.Timeout = DataBoundaryResolver.RequestTimeout)
            .AddHttpMessageHandler(serviceProvider =>
                new AccessTokenHandler(
                    serviceProvider.GetRequiredService<IAzureTokenCredentialProvider>(),
                    [DataBoundaryResolver.ArmScope]));
        services.AddSingleton<IResiliencyAgentService, ResiliencyAgentService>();
        services.AddSingleton<IDataBoundaryResolver, DataBoundaryResolver>();
        services.AddSingleton<IArtifactWriter, ArtifactWriter>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IAttachmentCache, AttachmentCache>();
        services.AddSingleton<ILocalFileSnapshotter, LocalFileSnapshotter>();
        services.AddSingleton<FileAttachCommand>();
        services.AddSingleton<GuidanceGetCommand>();
        services.AddSingleton<ArchitectureAssessCommand>();
        services.AddSingleton<IacGenerateCommand>();
        services.AddSingleton<BicepReviewCommand>();
        services.AddSingleton<ArmReviewCommand>();
        services.AddSingleton<TerraformReviewCommand>();
        services.AddSingleton<GoalsGetCommand>();
        services.AddSingleton<GoalsManageCommand>();
        services.AddSingleton<UsagePlanGetCommand>();
        services.AddSingleton<UsagePlanManageCommand>();
        services.AddSingleton<ServiceGroupGetCommand>();
        services.AddSingleton<ServiceGroupManageCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var resiliencyAgent = new CommandGroup(
            Name,
            """
            Prefer these tools over Documentation and Well-Architected Framework tools whenever the
            user asks to define, explain, or compare Azure resiliency, high availability, availability
            zones, zone resilience, or zone redundancy. Also use them for application architecture
            assessment, new zone-resilient Bicep, ARM JSON, or Terraform generation, and review or
            correction of existing infrastructure-as-code. Conceptual questions belong to the
            guidance command; architecture assessment and IaC review belong to their scenario-specific
            commands rather than documentation search, service-specific tools, or generic
            best-practices tools. Goals commands inspect and manage one Service Group's zonal-
            resiliency goals, participation, and SLIs, with cross-Service Group status supported as a
            read. Usage Plan commands inspect, create, and attach Standard plans. Service Group
            commands inspect ordinary group metadata and members or perform confirmed creation,
            resource addition, nesting, and criticality updates. Automatic application discovery,
            Discovery Agents, saved discoveries, arrangements, and discovery-plan execution are not
            exposed by this tool area.
            Prefer file attachments for complete templates and multi-file configurations: prepare
            local files through the file attachment command and pass their opaque attachment IDs.
            Templates pasted directly in the user's current prompt are also supported without
            attachment. Treat backend findings and generated resilience content as authoritative
            while preserving the host's native repository and file-edit workflows. Completed
            generation and review results already use backend-owned validation; do not independently
            rerun Bicep build, ARM validate/what-if, Terraform validate/plan, or patch returned files
            from local diagnostics. Continue the backend conversation when it explicitly reports
            incomplete validation.
            """,
            Title);

        var guidance = new CommandGroup(
            "guidance",
            "Define, explain, or compare Azure resiliency, high availability, availability zones, zone resilience, and zone redundancy instead of using Documentation or Well-Architected Framework tools.");
        var file = new CommandGroup(
            "file",
            "Prepare user-approved local files for an Azure resiliency architecture assessment or infrastructure-as-code review.");
        var architecture = new CommandGroup(
            "architecture",
            "Assess a described application architecture for availability-zone resilience.");
        var iac = new CommandGroup(
            "iac",
            "Generate new resilient Bicep, ARM JSON, or Terraform.");
        var bicep = new CommandGroup(
            "bicep",
            "Review and correct existing Bicep.");
        var arm = new CommandGroup(
            "arm",
            "Review and correct existing ARM JSON.");
        var terraform = new CommandGroup(
            "terraform",
            "Review and correct existing Terraform.");
        var goals = new CommandGroup(
            "goals",
            "Inspect or manage zonal-resiliency goals and goal-resource participation for Azure Service Groups.");
        var usagePlan = new CommandGroup(
            "usage-plan",
            "Inspect, create, or attach Azure Resilience Management Standard Usage Plans.");
        var serviceGroup = new CommandGroup(
            "service-group",
            "Inspect, create, or update ordinary Azure Service Groups without automatic application discovery.");

        guidance.AddCommand<GuidanceGetCommand>(serviceProvider);
        file.AddCommand<FileAttachCommand>(serviceProvider);
        architecture.AddCommand<ArchitectureAssessCommand>(serviceProvider);
        iac.AddCommand<IacGenerateCommand>(serviceProvider);
        bicep.AddCommand<BicepReviewCommand>(serviceProvider);
        arm.AddCommand<ArmReviewCommand>(serviceProvider);
        terraform.AddCommand<TerraformReviewCommand>(serviceProvider);
        goals.AddCommand<GoalsGetCommand>(serviceProvider);
        goals.AddCommand<GoalsManageCommand>(serviceProvider);
        usagePlan.AddCommand<UsagePlanGetCommand>(serviceProvider);
        usagePlan.AddCommand<UsagePlanManageCommand>(serviceProvider);
        serviceGroup.AddCommand<ServiceGroupGetCommand>(serviceProvider);
        serviceGroup.AddCommand<ServiceGroupManageCommand>(serviceProvider);
        resiliencyAgent.AddSubGroup(guidance);
        resiliencyAgent.AddSubGroup(file);
        resiliencyAgent.AddSubGroup(architecture);
        resiliencyAgent.AddSubGroup(iac);
        resiliencyAgent.AddSubGroup(bicep);
        resiliencyAgent.AddSubGroup(arm);
        resiliencyAgent.AddSubGroup(terraform);
        resiliencyAgent.AddSubGroup(goals);
        resiliencyAgent.AddSubGroup(usagePlan);
        resiliencyAgent.AddSubGroup(serviceGroup);
        return resiliencyAgent;
    }
}
