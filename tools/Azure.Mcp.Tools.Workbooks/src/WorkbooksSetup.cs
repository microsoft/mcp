// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Workbooks;

public class WorkbooksSetup : IAreaSetup
{
    public string Name => "workbooks";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IWorkbooksService, WorkbooksService>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var workbooks = new CommandGroup(Name, "Workbooks operations - Commands for managing Azure Workbooks resources and interactive data visualization dashboards. Includes operations for listing, creating, updating, and deleting workbooks, as well as managing workbook configurations and content.");
        rootGroup.AddSubGroup(workbooks);

        workbooks.AddCommand("list", new ListWorkbooksCommand(
            loggerFactory.CreateLogger<ListWorkbooksCommand>()));

        workbooks.AddCommand("show", new ShowWorkbooksCommand(
            loggerFactory.CreateLogger<ShowWorkbooksCommand>()));

        workbooks.AddCommand("update", new UpdateWorkbooksCommand(
            loggerFactory.CreateLogger<UpdateWorkbooksCommand>()));

        workbooks.AddCommand("create", new CreateWorkbooksCommand(
            loggerFactory.CreateLogger<CreateWorkbooksCommand>()));

        workbooks.AddCommand("delete", new DeleteWorkbooksCommand(
            loggerFactory.CreateLogger<DeleteWorkbooksCommand>()));
    }
}
