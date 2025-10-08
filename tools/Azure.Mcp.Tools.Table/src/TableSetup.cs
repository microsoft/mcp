// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Table.Commands;
using Azure.Mcp.Tools.Table.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Table;

public class TableSetup : IAreaSetup
{
    public string Name => "table";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITableService, TableService>();

        services.AddSingleton<TableListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var table = new CommandGroup(Name,
            """
            Table operations - Commands for managing and accessing Azure Table. Use this tool when you need to list or
            get an Azure Table. Note that this tool requires appropriate Table account permissions and will only access
            table resources accessible to the authenticated user.
            """);

        var tableList = serviceProvider.GetRequiredService<TableListCommand>();
        table.AddCommand(tableList.Name, tableList);

        return table;
    }
}
