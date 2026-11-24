// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Azure.Mcp.Tools.AzureMigrate.Commands.Session;
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
        services.AddSingleton<WaitInventoryCommand>();

        // Assessment (POC): external assessment create and operation status APIs.
        services.AddSingleton<IAssessmentService, AssessmentService>();
        // The business case skill runs on the Discovery Service at AZUREMIGRATE_HA_DS_BASE_URL; set AZUREMIGRATE_HA_MOCK=true to mock it.
        services.AddSingleton<HaSessionService>();
        services.AddSingleton<MockHaSessionService>();
        services.AddSingleton<IHaSessionService>(sp => MockHaSessionService.IsEnabled
            ? sp.GetRequiredService<MockHaSessionService>()
            : sp.GetRequiredService<HaSessionService>());

        // Business case (POC): create starts the assessment; each status call moves the business case forward, statelessly.
        services.AddSingleton<IBusinessCaseEvaluationService, BusinessCaseEvaluationService>();
        services.AddSingleton<IInventoryExportService, InventoryExportService>();
        services.AddSingleton<CreateBusinessCaseCommand>();
        services.AddSingleton<GetBusinessCaseStatusCommand>();
        services.AddSingleton<WaitBusinessCaseCommand>();
        services.AddSingleton<ArchiveSessionCommand>();

        // MCP Apps upload widget for create-or-update; registered only when AZMIGRATE_INVENTORY_WIDGET_DOMAINS is set.
        InventoryUploadWidget.Register(CreateOrUpdateInventoryCommand.CommandId);
        // MCP Apps card that renders the ROI report (ROI.md) like a Markdown preview; set AZMIGRATE_REPORT_WIDGET_ENABLED=false to turn
        // it off. Shown only with status results: the agent tracks the business case with the wait tool and then calls the status tool
        // once, so the card appears together with the agent's summary and never shows a stale stage. With storage connect domains
        // (AZMIGRATE_REPORT_WIDGET_DOMAINS, else AZMIGRATE_INVENTORY_WIDGET_DOMAINS) the card reads ROI.md in the browser.
        BusinessCaseReportWidget.Register(GetBusinessCaseStatusCommand.CommandId);
        // The same card in inventory mode, shown with inventory status results: the agent waits with the inventory wait tool and
        // then calls the status tool once, so the summary card appears together with the agent's reply.
        InventorySummaryWidget.Register(GetInventoryStatusCommand.CommandId);
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
        // No card: the agent calls it repeatedly to wait quietly until the inventory completes.
        inventory.AddCommand<WaitInventoryCommand>(serviceProvider);

        var businessCase = new CommandGroup(
            "businesscase",
            "Azure Catalyst business case operations - Start a business case for the signed-in user from their offline collector ZIP, " +
            "and check its status, which also moves it to its next stage and shows the ROI report once it finishes.");
        azureMigrate.AddSubGroup(businessCase);
        businessCase.AddCommand<CreateBusinessCaseCommand>(serviceProvider);
        businessCase.AddCommand<GetBusinessCaseStatusCommand>(serviceProvider);
        // No card: the agent calls it repeatedly to wait quietly until the business case finishes.
        businessCase.AddCommand<WaitBusinessCaseCommand>(serviceProvider);
        var session = new CommandGroup(
            "session",
            "Azure Catalyst session operations - Close the signed-in user's current inventory and business case sessions together " +
            "when the user explicitly asks to archive or reset their session.");
        azureMigrate.AddSubGroup(session);
        // Closes both the inventory and business case sessions when the user chooses to start a new session.
        session.AddCommand<ArchiveSessionCommand>(serviceProvider);

        return azureMigrate;
    }
}
