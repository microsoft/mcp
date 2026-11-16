// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;
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
/// Setup class for Azure Migrate toolset.
/// </summary>
public class AzureMigrateSetup : IAreaSetup
{
    /// <inheritdoc/>
    public string Name => "azuremigrate";

    /// <inheritdoc/>
    public string Title => "Azure Landing Zone Generation and Guidance";

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

        // Assessment (POC): the Business Case Service machine assessment from a collector ZIP when its URL is configured,
        // otherwise a mock.
        services.AddSingleton<IAssessmentServiceClient>(sp =>
            BusinessCaseAssessmentServiceClient.ConfiguredBaseUrl is { } baseUrl
                ? new BusinessCaseAssessmentServiceClient(sp.GetRequiredService<ILogger<BusinessCaseAssessmentServiceClient>>(), baseUrl)
                : new MockAssessmentServiceClient(TimeProvider.System));
        services.AddSingleton<IHaSessionClient, DsHaSessionClient>();
        services.AddSingleton<CreateAssessmentCommand>();
        services.AddSingleton<GetAssessmentStatusCommand>();
        services.AddSingleton<GetHaStatusCommand>();
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
            "Azure Migrate inventory operations - Create or update a server inventory for the signed-in user from a file on the " +
            "user's machine (the user uploads it with a returned azcopy command), get the status of an inventory operation, and get " +
            "a download link for the output of a finished inventory.");
        azureMigrate.AddSubGroup(inventory);
        inventory.AddCommand<CreateOrUpdateInventoryCommand>(serviceProvider);
        inventory.AddCommand<GetInventoryStatusCommand>(serviceProvider);
        inventory.AddCommand<ExportInventoryCommand>(serviceProvider);

        var assessment = new CommandGroup(
            "assessment",
            "Azure Migrate assessment operations - Create an assessment for the signed-in user for a target region and currency, and " +
            "get the status of an assessment operation or of the HA assessment started after it.");
        azureMigrate.AddSubGroup(assessment);
        assessment.AddCommand<CreateAssessmentCommand>(serviceProvider);
        assessment.AddCommand<GetAssessmentStatusCommand>(serviceProvider);
        assessment.AddCommand<GetHaStatusCommand>(serviceProvider);

        return azureMigrate;
    }
}
