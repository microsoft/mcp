// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Workbooks;

public class WorkbooksSetup : IAreaSetup
{
    public string Name => "workbooks";

    public string Title => "Azure Workbooks";

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
        var workbooks = new CommandGroup(Name, "Workbooks operations - Commands for managing Azure Workbooks resources and interactive data visualization dashboards. Includes operations for listing, creating, updating, and deleting workbooks, as well as managing workbook configurations and content.", Title);

        workbooks.AddCommand<ListWorkbooksCommand>(serviceProvider);
        workbooks.AddCommand<ShowWorkbooksCommand>(serviceProvider);
        workbooks.AddCommand<UpdateWorkbooksCommand>(serviceProvider);
        workbooks.AddCommand<CreateWorkbooksCommand>(serviceProvider);
        workbooks.AddCommand<DeleteWorkbooksCommand>(serviceProvider);

        return workbooks;
    }
}
