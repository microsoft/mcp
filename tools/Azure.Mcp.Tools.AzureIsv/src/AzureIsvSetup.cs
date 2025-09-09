// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureIsv.Commands.Datadog;
using Azure.Mcp.Tools.AzureIsv.Services;
using Azure.Mcp.Tools.AzureIsv.Services.Datadog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;

namespace Azure.Mcp.Tools.AzureIsv;

public class AzureIsvSetup : IAreaSetup
{
    public string Name => "datadog";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDatadogService, DatadogService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var datadog = new CommandGroup(Name, "Datadog operations - Commands for managing and monitoring Azure resources through Datadog integration. Includes operations for listing Datadog monitors and retrieving information about monitored Azure resources and their health status.");
        rootGroup.AddSubGroup(datadog);

        var monitoredResources = new CommandGroup("monitoredresources", "Datadog monitored resources operations - Commands for listing monitored resources in a specific Datadog monitor.");
        datadog.AddSubGroup(monitoredResources);

        monitoredResources.AddCommand("list", new MonitoredResourcesListCommand(loggerFactory.CreateLogger<MonitoredResourcesListCommand>()));
    }
}
