// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.DependencyInjection;
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
    }

    /// <inheritdoc/>
    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var azureMigrate = new CommandGroup(
            Name,
            """
            Azure Landing Zone operations - Provides best-practice guidance and Terraform-ready generation for Azure Platform Landing Zones. Supports policy and governance changes, naming standards, network topology (hub/spoke/vWAN), identity and subscription design, firewall patterns, and starter module customization—aligned to Microsoft's Cloud Adoption Framework. Can generate a complete platform landing zone with configurable parameters (regions, connectivity, security, subscriptions).
            """,
            Title);

        // Create platform landing zone subgroup
        var platformLandingZone = new CommandGroup(
            "platformlandingzone",
            "Platform landing zone operations - Commands for generating new platform landing zones and providing guidance on configuration and customization.");
        azureMigrate.AddSubGroup(platformLandingZone);

        // Register platform landing zone commands
        var platformLandingZoneGetGuidance = serviceProvider.GetRequiredService<GetGuidanceCommand>();
        platformLandingZone.AddCommand(platformLandingZoneGetGuidance.Name, platformLandingZoneGetGuidance);

        var platformLandingZoneRequest = serviceProvider.GetRequiredService<RequestCommand>();
        platformLandingZone.AddCommand(platformLandingZoneRequest.Name, platformLandingZoneRequest);

        return azureMigrate;
    }
}
