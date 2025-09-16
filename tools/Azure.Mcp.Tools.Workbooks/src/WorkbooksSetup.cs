// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Workbooks;

public class WorkbooksSetup : IAreaSetup
{
    public string Name => "workbooks";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IWorkbooksService, WorkbooksService>();

        services.AddSingleton<ListWorkbooksCommand>();
        services.AddSingleton<ShowWorkbooksCommand>();
        services.AddSingleton<UpdateWorkbooksCommand>();
        services.AddSingleton<CreateWorkbooksCommand>();
        services.AddSingleton<DeleteWorkbooksCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var workbooks = new CommandGroup(Name, "Workbooks operations - Commands for managing Azure Workbooks resources and interactive data visualization dashboards. Includes operations for listing, creating, updating, and deleting workbooks, as well as managing workbook configurations and content.");

        workbooks.AddCommand("list", serviceProvider.GetRequiredService<ListWorkbooksCommand>());

        workbooks.AddCommand("show", serviceProvider.GetRequiredService<ShowWorkbooksCommand>());

        workbooks.AddCommand("update", serviceProvider.GetRequiredService<UpdateWorkbooksCommand>());

        workbooks.AddCommand("create", serviceProvider.GetRequiredService<CreateWorkbooksCommand>());

        workbooks.AddCommand("delete", serviceProvider.GetRequiredService<DeleteWorkbooksCommand>());

        return workbooks;
    }
}
