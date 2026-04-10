// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Cosmos;

public class CosmosSetup : IAreaSetup
{
    public string Name => "cosmos";

    public string Title => "Azure Cosmos DB";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICosmosService, CosmosService>();

        services.AddSingleton<CosmosListCommand>();
        services.AddSingleton<ItemQueryCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Cosmos command group
        var cosmos = new CommandGroup(Name, "Cosmos DB operations - Commands for managing and querying Azure Cosmos DB resources. Includes operations for accounts, databases, containers, and document queries.", Title);

        // Consolidated hierarchical list command
        var cosmosList = serviceProvider.GetRequiredService<CosmosListCommand>();
        cosmos.AddCommand(cosmosList.Name, cosmosList);

        // Create Cosmos subgroups for item query
        var databases = new CommandGroup("database", "Cosmos DB database operations - Commands for managing databases within your Cosmos DB accounts.");
        cosmos.AddSubGroup(databases);

        var cosmosContainer = new CommandGroup("container", "Cosmos DB container operations - Commands for managing containers within your Cosmos DB databases.");
        databases.AddSubGroup(cosmosContainer);

        // Create items subgroup for Cosmos
        var cosmosItem = new CommandGroup("item", "Cosmos DB item operations - Commands for querying, creating, updating, and deleting documents within your Cosmos DB containers.");
        cosmosContainer.AddSubGroup(cosmosItem);

        var itemQuery = serviceProvider.GetRequiredService<ItemQueryCommand>();
        cosmosItem.AddCommand(itemQuery.Name, itemQuery);

        return cosmos;
    }
}
