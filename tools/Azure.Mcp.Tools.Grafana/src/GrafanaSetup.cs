// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Grafana.Commands.Workspace;
using Azure.Mcp.Tools.Grafana.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Grafana;

public class GrafanaSetup : IAreaSetup
{
    public string Name => "grafana";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IGrafanaService, GrafanaService>();

        services.AddSingleton<WorkspaceListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var grafana = new CommandGroup(Name, "Grafana workspace operations - Commands for managing and accessing Azure Managed Grafana resources and monitoring dashboards. Includes operations for listing Grafana workspaces and managing data visualization and monitoring capabilities.");

        var workspace = serviceProvider.GetRequiredService<WorkspaceListCommand>();
        grafana.AddCommand(workspace.Name, workspace);

        return grafana;
    }
}
