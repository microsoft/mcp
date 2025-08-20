// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Cosmos;

public class CosmosSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICosmosService, CosmosService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Cosmos command group
        var cosmos = new CommandGroup("cosmos", "Cosmos DB operations - Commands for managing and querying Azure Cosmos DB resources. Includes operations for databases, containers, and document queries.");
        rootGroup.AddSubGroup(cosmos);

        // Create Cosmos subgroups
        var databases = new CommandGroup("database", "Cosmos DB database operations - Commands for listing, creating, and managing database within your Cosmos DB accounts.");
        cosmos.AddSubGroup(databases);

        var cosmosContainer = new CommandGroup("container", "Cosmos DB container operations - Commands for listing, creating, and managing container (collection) within your Cosmos DB databases.");
        databases.AddSubGroup(cosmosContainer);

        var cosmosAccount = new CommandGroup("account", "Cosmos DB account operations - Commands for listing and managing Cosmos DB account in your Azure subscription.");
        cosmos.AddSubGroup(cosmosAccount);

        // Create items subgroup for Cosmos
        var cosmosItem = new CommandGroup("item", "Cosmos DB item operations - Commands for querying, creating, updating, and deleting document within your Cosmos DB containers.");
        cosmosContainer.AddSubGroup(cosmosItem);        // Register Cosmos commands
        databases.AddCommand("list", new DatabaseListCommand(
            loggerFactory.CreateLogger<DatabaseListCommand>()));
        cosmosContainer.AddCommand("list", new ContainerListCommand(
            loggerFactory.CreateLogger<ContainerListCommand>()));
        cosmosAccount.AddCommand("list", new AccountListCommand(
            loggerFactory.CreateLogger<AccountListCommand>()));
        cosmosItem.AddCommand("query", new ItemQueryCommand(
            loggerFactory.CreateLogger<ItemQueryCommand>()));
    }
}
