// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Commands.ServiceHealthEvents;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.ResourceHealth;

public class ResourceHealthSetup : IAreaSetup
{
    public string Name => "resourcehealth";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IResourceHealthService, ResourceHealthService>();

        services.AddSingleton<AvailabilityStatusGetCommand>();
        services.AddSingleton<AvailabilityStatusListCommand>();

        services.AddSingleton<ServiceHealthEventsListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var resourceHealth = new CommandGroup(Name,
            """
            Resource Health operations - Commands for monitoring and diagnosing Azure resource health status.
            Use this tool to check the current availability status of Azure resources and identify potential issues.
            This tool provides access to Azure Resource Health data including availability state, detailed status,
            historical health information, and service health events for troubleshooting and monitoring purposes.
            """);

        // Create availability-status subgroup
        var availabilityStatus = new CommandGroup("availability-status",
            "Resource availability status operations - Commands for retrieving current and historical availability status of Azure resources.");
        resourceHealth.AddSubGroup(availabilityStatus);

        // Create service-health-events subgroup
        var serviceHealthEvents = new CommandGroup("service-health-events",
            "Service health events operations - Commands for retrieving Azure service health events affecting Azure services and subscriptions.");
        resourceHealth.AddSubGroup(serviceHealthEvents);

        // Register commands
        var availabilityStatusGet = serviceProvider.GetRequiredService<AvailabilityStatusGetCommand>();
        availabilityStatus.AddCommand(availabilityStatusGet.Name, availabilityStatusGet);
        var availabilityStatusList = serviceProvider.GetRequiredService<AvailabilityStatusListCommand>();
        availabilityStatus.AddCommand(availabilityStatusList.Name, availabilityStatusList);

        var serviceHealthEventsList = serviceProvider.GetRequiredService<ServiceHealthEventsListCommand>();
        serviceHealthEvents.AddCommand(serviceHealthEventsList.Name, serviceHealthEventsList);

        return resourceHealth;
    }
}
