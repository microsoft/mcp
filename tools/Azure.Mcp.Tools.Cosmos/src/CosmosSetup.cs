// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Cosmos;

public class CosmosSetup : IAreaSetup
{
    public string Name => "cosmos";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICosmosService, CosmosService>();

        services.AddSingleton<DatabaseListCommand>();
        services.AddSingleton<ContainerListCommand>();
        services.AddSingleton<AccountListCommand>();
        services.AddSingleton<ItemQueryCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Cosmos command group
        var cosmos = new CommandGroup(Name, "Cosmos DB operations - Commands for managing and querying Azure Cosmos DB resources. Includes operations for databases, containers, and document queries.");

        // Create Cosmos subgroups
        var databases = new CommandGroup("database", "Cosmos DB database operations - Commands for listing, creating, and managing database within your Cosmos DB accounts.");
        cosmos.AddSubGroup(databases);

        var cosmosContainer = new CommandGroup("container", "Cosmos DB container operations - Commands for listing, creating, and managing container (collection) within your Cosmos DB databases.");
        databases.AddSubGroup(cosmosContainer);

        var cosmosAccount = new CommandGroup("account", "Cosmos DB account operations - Commands for listing and managing Cosmos DB account in your Azure subscription.");
        cosmos.AddSubGroup(cosmosAccount);

        // Create items subgroup for Cosmos
        var cosmosItem = new CommandGroup("item", "Cosmos DB item operations - Commands for querying, creating, updating, and deleting document within your Cosmos DB containers.");
        cosmosContainer.AddSubGroup(cosmosItem);

        var databaseList = serviceProvider.GetRequiredService<DatabaseListCommand>();
        databases.AddCommand(databaseList.Name, databaseList);

        var containerList = serviceProvider.GetRequiredService<ContainerListCommand>();
        cosmosContainer.AddCommand(containerList.Name, containerList);

        var accountList = serviceProvider.GetRequiredService<AccountListCommand>();
        cosmosAccount.AddCommand(accountList.Name, accountList);

        var itemQuery = serviceProvider.GetRequiredService<ItemQueryCommand>();
        cosmosItem.AddCommand(itemQuery.Name, itemQuery);

        return cosmos;
    }
}
