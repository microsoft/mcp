// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.AzureIsv.Commands.Datadog;
using Azure.Mcp.Tools.AzureIsv.Services;
using Azure.Mcp.Tools.AzureIsv.Services.Datadog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.AzureIsv;

public sealed class AzureIsvRegistration : IAreaRegistration
{
    public static string AreaName => "datadog";

    public static string AreaTitle => "Datadog Integration";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Datadog operations - Commands for managing and monitoring Azure resources through Datadog integration. Includes operations for listing Datadog monitors and retrieving information about monitored Azure resources and their health status.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "monitoredresources",
                Description = "Datadog monitored resources operations - Commands for listing monitored resources in a specific Datadog monitor.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "bbd026b6-df96-4c52-8b72-13734984a600",
                        Name = "list",
                        Description = "List monitored resources in Datadog for a datadog resource taken as input from the user. This command retrieves all monitored azure resources available. Requires `datadog-resource`, `resource-group` and `subscription`. Result is a list of monitored resources as a JSON array.",
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
                                Name = "datadog-resource",
                                Description = "The name of the Datadog resource to use. This is the unique name you chose for your Datadog resource in Azure.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(MonitoredResourcesListCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IDatadogService, DatadogService>();
        services.AddSingleton<MonitoredResourcesListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(MonitoredResourcesListCommand) => serviceProvider.GetRequiredService<MonitoredResourcesListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in datadog area.")
        };
}
