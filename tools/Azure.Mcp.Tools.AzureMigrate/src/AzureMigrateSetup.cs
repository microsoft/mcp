// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;
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
    public string Title => "Azure Landing Zone Guidance";

    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
        // Register guidance service and command
        services.AddSingleton<IPlatformLandingZoneGuidanceService, PlatformLandingZoneGuidanceService>();
        services.AddHttpClient<PlatformLandingZoneGuidanceService>();
        services.AddSingleton<GetModificationGuidanceCommand>();

        // Register landing zone service and command
        services.AddSingleton<IPlatformLandingZoneService, PlatformLandingZoneService>();
        services.AddHttpClient<PlatformLandingZoneService>();
        services.AddSingleton<GenerateLandingZoneCommand>();
    }

    /// <inheritdoc/>
    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var azureMigrate = new CommandGroup(
            Name,
            """
            Azure Landing Zone operations - Provides guidance and documentation for modifying Azure Landing Zone
            templates and configurations. Helps with policy changes, service configuration, resource naming,
            network topology, identity management, governance, and starter module customizations. This tool focuses
            on providing best practices and recommendations for both platform and application landing zones across
            different deployment methods (Bicep, Terraform, Portal). Use this tool when you need guidance on
            implementing or customizing Azure Landing Zones according to Microsoft's Cloud Adoption Framework.
            """,
            Title);

        // Create platform landing zone subgroup
        var platformLandingZone = new CommandGroup(
            "platformlandingzone",
            "Platform landing zone operations - Commands for generating guidance on platform landing zone configuration and customization.");
        azureMigrate.AddSubGroup(platformLandingZone);

        // Register platform landing zone commands
        var platformLandingZoneGetModificationGuidance = serviceProvider.GetRequiredService<GetModificationGuidanceCommand>();
        platformLandingZone.AddCommand(platformLandingZoneGetModificationGuidance.Name, platformLandingZoneGetModificationGuidance);

        var platformLandingZoneGenerateLandingZone = serviceProvider.GetRequiredService<GenerateLandingZoneCommand>();
        platformLandingZone.AddCommand(platformLandingZoneGenerateLandingZone.Name, platformLandingZoneGenerateLandingZone);

        return azureMigrate;
    }
}
