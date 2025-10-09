// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Tables.Commands;
using Azure.Mcp.Tools.Tables.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Tables;

public class TablesSetup : IAreaSetup
{
    public string Name => "tables";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITablesService, TablesService>();

        services.AddSingleton<TablesListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var tables = new CommandGroup(Name,
            """
            Azure Table storage operations - Commands for managing and accessing Azure Table storage. Use this tool
            when you need to list Azure Table storage tables in either a Storage or Cosmos DB account. Note that this
            tool requires appropriate Azure Table storage account permissions and will only access table storage
            resources accessible to the authenticated user.
            """);

        var tablesList = serviceProvider.GetRequiredService<TablesListCommand>();
        tables.AddCommand(tablesList.Name, tablesList);

        return tables;
    }
}
