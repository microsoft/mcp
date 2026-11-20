// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;
using Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureMigrate;

/// <summary>
/// Setup class for the Azure Catalyst toolset.
/// </summary>
public class AzureMigrateSetup : IAreaSetup
{
    /// <inheritdoc/>
    public string Name => "azure_catalyst";

    /// <inheritdoc/>
    public string Title => "Azure Catalyst";

    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
        // Register shared helpers
        services.AddSingleton<AzureHttpHelper>();
        services.AddSingleton<AzureMigrateProjectHelper>();

        // Register guidance service and command
        services.AddSingleton<IPlatformLandingZoneGuidanceService, PlatformLandingZoneGuidanceService>();
        services.AddHttpClient<PlatformLandingZoneGuidanceService>();
        services.AddSingleton<GetGuidanceCommand>();

        // Register platform landing zone service and command
        services.AddSingleton<IPlatformLandingZoneService, PlatformLandingZoneService>();
        services.AddSingleton<RequestCommand>();

        // POC commands read the caller's token from the current HTTP request.
        services.AddHttpContextAccessor();

        // Inventory (POC): create or update an inventory, then check its status, on the Discovery Service.
        services.AddSingleton<CreateOrUpdateInventoryCommand>();
        services.AddSingleton<GetInventoryStatusCommand>();
        services.AddSingleton<ExportInventoryCommand>();

        // Assessment (POC): external assessment create and operation status APIs (placeholders for now).
        services.AddSingleton<IAssessmentCreateClient, AssessmentCreateClient>();
        services.AddSingleton<IAssessmentOperationStatusClient, AssessmentOperationStatusClient>();
        // The business case skill runs on the Discovery Service at AZUREMIGRATE_HA_DS_BASE_URL; set AZUREMIGRATE_HA_MOCK=true to mock it.
        services.AddSingleton<DsHaSessionClient>();
        services.AddSingleton<MockHaSessionClient>();
        services.AddSingleton<IHaSessionClient>(sp => MockHaSessionClient.IsEnabled
            ? sp.GetRequiredService<MockHaSessionClient>()
            : sp.GetRequiredService<DsHaSessionClient>());
        services.AddSingleton<CreateAssessmentCommand>();
        services.AddSingleton<GetAssessmentStatusCommand>();
        services.AddSingleton<GetHaStatusCommand>();

        // Business case (POC): create starts the assessment; each status call moves the business case forward, statelessly.
        services.AddSingleton<IBusinessCaseEvaluationClient, BusinessCaseEvaluationClient>();
        services.AddSingleton<IInventoryExportClient, InventoryExportClient>();
        services.AddSingleton<CreateBusinessCaseCommand>();
        services.AddSingleton<GetBusinessCaseStatusCommand>();

        // MCP Apps upload widget for create-or-update; registered only when AZMIGRATE_INVENTORY_WIDGET_DOMAINS is set.
        InventoryUploadWidget.Register(CreateOrUpdateInventoryCommand.CommandId);
        // MCP Apps card that renders the ROI report (ROI.md) like a Markdown preview; set AZMIGRATE_REPORT_WIDGET_ENABLED=false to turn it off.
        // It is attached to the status tool: it shows the stage on each poll and the report once the business case succeeds.
        BusinessCaseReportWidget.Register(GetBusinessCaseStatusCommand.CommandId);
    }

    /// <inheritdoc/>
    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var azureMigrate = new CommandGroup(
            Name,
            "Azure Platform Landing Zone operations - Provides best-practice guidance and Terraform-ready generation " +
            "for Azure Platform Landing Zones. Supports policy and governance changes, naming standards, network " +
            "topology (hub/spoke/vWAN), identity and subscription design, firewall patterns, and starter module " +
            "customization aligned to Microsoft's Cloud Adoption Framework. Can generate a complete platform " +
            "landing zone with configurable parameters (regions, connectivity, security, subscriptions). Use this " +
            "tool any time a user mentions the words \"Platform Landing Zone\" or needs guidance on modifying or " +
            "generating Azure Platform Landing Zones.",
            Title);

        // Create platform landing zone subgroup
        var platformLandingZone = new CommandGroup(
            "platformlandingzone",
            "Platform Landing Zone operations - Commands for generating new platform landing zones and providing guidance on configuration and customization.");
        azureMigrate.AddSubGroup(platformLandingZone);

        // Register platform landing zone commands
        platformLandingZone.AddCommand<GetGuidanceCommand>(serviceProvider);
        platformLandingZone.AddCommand<RequestCommand>(serviceProvider);

        var inventory = new CommandGroup(
            "inventory",
            "Azure Catalyst inventory operations - Create or update a server inventory for the signed-in user from a file on the " +
            "user's machine (the user uploads it with a returned azcopy command), get the status of an inventory operation, and get " +
            "a download link for the output of a finished inventory.");
        azureMigrate.AddSubGroup(inventory);
        inventory.AddCommand<CreateOrUpdateInventoryCommand>(serviceProvider);
        inventory.AddCommand<GetInventoryStatusCommand>(serviceProvider);
        inventory.AddCommand<ExportInventoryCommand>(serviceProvider);

        // Assessment tools are hidden for now; the business case tools run the assessment themselves.
        // var assessment = new CommandGroup(
        //     "assessment",
        //     "Azure Catalyst assessment operations - Create an assessment for the signed-in user for a target region and currency, and " +
        //     "get the status of an assessment operation or of the HA assessment started after it.");
        // azureMigrate.AddSubGroup(assessment);
        // assessment.AddCommand<CreateAssessmentCommand>(serviceProvider);
        // assessment.AddCommand<GetAssessmentStatusCommand>(serviceProvider);
        // assessment.AddCommand<GetHaStatusCommand>(serviceProvider);

        var businessCase = new CommandGroup(
            "businesscase",
            "Azure Catalyst business case operations - Start a business case for the signed-in user from their offline collector ZIP, " +
            "and check its status, which also moves it to its next stage and shows the ROI report once it finishes.");
        azureMigrate.AddSubGroup(businessCase);
        businessCase.AddCommand<CreateBusinessCaseCommand>(serviceProvider);
        businessCase.AddCommand<GetBusinessCaseStatusCommand>(serviceProvider);

        return azureMigrate;
    }
}
