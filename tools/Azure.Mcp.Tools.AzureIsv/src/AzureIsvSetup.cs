// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureIsv.Commands.Datadog;
using Azure.Mcp.Tools.AzureIsv.Services;
using Azure.Mcp.Tools.AzureIsv.Services.Datadog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureIsv;

public class AzureIsvSetup : IAreaSetup
{
    public string Name => "datadog";

    public string Title => "Datadog Integration";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDatadogService, DatadogService>();

        services.AddSingleton<MonitoredResourcesListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var datadog = new CommandGroup(Name, "Datadog operations – Commands to manage and monitor Azure resources through Datadog integration, including listing Datadog monitors and retrieving health and monitoring information for Azure resources.", Title);

        var monitoredResources = new CommandGroup("monitoredresources", "Datadog monitored resources operations - Commands for listing monitored resources in a specific Datadog monitor.");
        datadog.AddSubGroup(monitoredResources);

        var monitoredResourcesList = serviceProvider.GetRequiredService<MonitoredResourcesListCommand>();
        monitoredResources.AddCommand(monitoredResourcesList.Name, monitoredResourcesList);

        return datadog;
    }
}
