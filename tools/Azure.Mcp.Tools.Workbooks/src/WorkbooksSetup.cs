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

        var workbooksList = serviceProvider.GetRequiredService<ListWorkbooksCommand>();
        workbooks.AddCommand(workbooksList.Name, workbooksList);

        var workbooksShow = serviceProvider.GetRequiredService<ShowWorkbooksCommand>();
        workbooks.AddCommand(workbooksShow.Name, workbooksShow);

        var workbooksUpdate = serviceProvider.GetRequiredService<UpdateWorkbooksCommand>();
        workbooks.AddCommand(workbooksUpdate.Name, workbooksUpdate);

        var workbooksCreate = serviceProvider.GetRequiredService<CreateWorkbooksCommand>();
        workbooks.AddCommand(workbooksCreate.Name, workbooksCreate);

        var workbooksDelete = serviceProvider.GetRequiredService<DeleteWorkbooksCommand>();
        workbooks.AddCommand(workbooksDelete.Name, workbooksDelete);

        return workbooks;
    }
}
