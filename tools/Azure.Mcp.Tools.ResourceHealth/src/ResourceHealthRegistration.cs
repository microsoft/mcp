// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Commands.ServiceHealthEvents;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.ResourceHealth;

public sealed class ResourceHealthRegistration : IAreaRegistration
{
    public static string AreaName => "resourcehealth";

    public static string AreaTitle => "Azure Resource Health";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Resource Health operations – Commands to monitor and diagnose Azure resource health, including availability status and service health events for troubleshooting and monitoring purposes.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "availability-status",
                Description = "Resource availability status operations - Commands for retrieving current and historical availability status of Azure resources.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "3b388cc7-4b16-4919-9e90-f592247d9891",
                        Name = "get",
                        Description = "Get the Azure Resource Health availability status for a specific resource or all resources in a subscription or resource group. Use this tool when asked about the availability status, health status, or Resource Health of an Azure resource (e.g. virtual machine, storage account). Reports whether a resource is Available, Unavailable, Degraded, or Unknown, including the reason and details. This is the correct tool for questions like 'What is the availability status of VM X?' or 'Is resource Y healthy?'.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resourceId",
                                Description = "The Azure resource ID to get health status for (e.g., /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Compute/virtualMachines/{vm}).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(AvailabilityStatusGetCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "health-events",
                Description = "Service health events operations - Commands for retrieving Azure service health events affecting Azure services and subscriptions.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "c3211c73-af20-4d8d-bed2-4f181e0e4c92",
                        Name = "list",
                        Description = "List Azure service health events to track service issues that occurred in recent timeframes (last 30 days, weeks, months). Query subscription for planned maintenance, past or ongoing service incidents, advisories, and security events. Provides detailed information about resource availability state, potential issues, and timestamps. Returns: trackingId, title, summary, eventType, status, startTime, endTime, impactedServices. Access Azure Service Health portal data programmatically.",
                        Title = "List",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "event-type",
                                Description = "Filter by event type (ServiceIssue, PlannedMaintenance, HealthAdvisory, Security). If not specified, all event types are included.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "status",
                                Description = "Filter by status (Active, Resolved). If not specified, all statuses are included.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tracking-id",
                                Description = "Filter by tracking ID to get a specific service health event.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "filter",
                                Description = "Additional OData filter expression to apply to the service health events query.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "query-start-time",
                                Description = "Start time for the query in ISO 8601 format (e.g., 2024-01-01T00:00:00Z). Events from this time onwards will be included.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "query-end-time",
                                Description = "End time for the query in ISO 8601 format (e.g., 2024-01-31T23:59:59Z). Events up to this time will be included.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ServiceHealthEventsListCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IResourceHealthService, ResourceHealthService>();
        services.AddSingleton<AvailabilityStatusGetCommand>();
        services.AddSingleton<ServiceHealthEventsListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(AvailabilityStatusGetCommand) => serviceProvider.GetRequiredService<AvailabilityStatusGetCommand>(),
            nameof(ServiceHealthEventsListCommand) => serviceProvider.GetRequiredService<ServiceHealthEventsListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in resourcehealth area.")
        };
}
