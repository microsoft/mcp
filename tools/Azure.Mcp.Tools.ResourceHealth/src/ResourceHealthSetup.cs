// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResourceHealth;

public class ResourceHealthSetup : IAreaSetup
{
    public string Name => "resourcehealth";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IResourceHealthService, ResourceHealthService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var resourceHealth = new CommandGroup(Name,
            """
            Resource Health operations - Commands for monitoring and diagnosing Azure resource health status.
            Use this tool to check the current availability status of Azure resources and identify potential issues.
            This tool provides access to Azure Resource Health data including availability state, detailed status,
            and historical health information for troubleshooting and monitoring purposes.
            """);
        rootGroup.AddSubGroup(resourceHealth);

        // Create availability-status subgroup
        var availabilityStatus = new CommandGroup("availability-status",
            "Resource availability status operations - Commands for retrieving current and historical availability status of Azure resources.");
        resourceHealth.AddSubGroup(availabilityStatus);

        // Register commands
        availabilityStatus.AddCommand("get", new AvailabilityStatusGetCommand(loggerFactory.CreateLogger<AvailabilityStatusGetCommand>()));
        availabilityStatus.AddCommand("list", new AvailabilityStatusListCommand(loggerFactory.CreateLogger<AvailabilityStatusListCommand>()));
    }
}
