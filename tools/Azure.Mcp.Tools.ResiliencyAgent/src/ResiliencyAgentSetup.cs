// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Commands.Architecture;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Arm;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Bicep;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.File;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Iac;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Terraform;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

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
        services.AddSingleton<IResiliencyAgentService, ResiliencyAgentService>();
        services.AddSingleton<IDataBoundaryResolver, DataBoundaryResolver>();
        services.AddSingleton<IArtifactWriter, ArtifactWriter>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IAttachmentCache, AttachmentCache>();
        services.AddSingleton<ILocalFileSnapshotter, LocalFileSnapshotter>();
        services.AddSingleton<FileAttachCommand>();
        services.AddSingleton<ArchitectureAssessCommand>();
        services.AddSingleton<IacGenerateCommand>();
        services.AddSingleton<BicepReviewCommand>();
        services.AddSingleton<ArmReviewCommand>();
        services.AddSingleton<TerraformReviewCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var resiliencyAgent = new CommandGroup(
            Name,
            """
            Use these tools to assess application architectures, generate new zone-resilient Bicep,
            ARM JSON, or Terraform, and review or correct existing infrastructure-as-code for
            availability-zone resilience. Existing IaC review belongs to the format-specific review
            commands here rather than service-specific or generic best-practices tools. Prefer file
            attachments for complete templates and multi-file configurations: prepare local files
            through the file attachment command and pass their opaque attachment IDs. Templates
            pasted directly in the user's current prompt are also supported without attachment.
            Treat backend findings and generated resilience content as authoritative while preserving
            the host's native repository and file-edit workflows.
            """,
            Title);

        var file = new CommandGroup(
            "file",
            "Prepare user-approved local files for a later Start Resilient scenario call.");
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

        file.AddCommand<FileAttachCommand>(serviceProvider);
        architecture.AddCommand<ArchitectureAssessCommand>(serviceProvider);
        iac.AddCommand<IacGenerateCommand>(serviceProvider);
        bicep.AddCommand<BicepReviewCommand>(serviceProvider);
        arm.AddCommand<ArmReviewCommand>(serviceProvider);
        terraform.AddCommand<TerraformReviewCommand>(serviceProvider);
        resiliencyAgent.AddSubGroup(file);
        resiliencyAgent.AddSubGroup(architecture);
        resiliencyAgent.AddSubGroup(iac);
        resiliencyAgent.AddSubGroup(bicep);
        resiliencyAgent.AddSubGroup(arm);
        resiliencyAgent.AddSubGroup(terraform);
        return resiliencyAgent;
    }
}
