// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Commands.CopyJob;
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
        services.AddSingleton<ICopyJobService, CopyJobService>();

        services.AddSingleton<CosmosListCommand>();
        services.AddSingleton<ItemQueryCommand>();

        services.AddSingleton<CopyJobCreateCommand>();
        services.AddSingleton<CopyJobGetCommand>();
        services.AddSingleton<CopyJobListCommand>();
        services.AddSingleton<CopyJobCancelCommand>();
        services.AddSingleton<CopyJobPauseCommand>();
        services.AddSingleton<CopyJobResumeCommand>();
        services.AddSingleton<CopyJobCompleteCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Cosmos command group
        var cosmos = new CommandGroup(Name, "Cosmos DB operations - Commands for managing and querying Azure Cosmos DB resources. Includes operations for accounts, databases, containers, document queries, and container copy jobs.", Title);

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

        // Copy Job command group
        var copyJob = new CommandGroup("copyjob",
            "Cosmos DB Copy Job operations - Create, monitor, and manage container copy jobs " +
            "(same as 'az cosmosdb copy'). Supports NoSQL, Cassandra, MongoDB, and Azure Blob " +
            "source/destination types with multi-task support.");
        cosmos.AddSubGroup(copyJob);

        var cjCreate = serviceProvider.GetRequiredService<CopyJobCreateCommand>();
        copyJob.AddCommand(cjCreate.Name, cjCreate);

        var cjGet = serviceProvider.GetRequiredService<CopyJobGetCommand>();
        copyJob.AddCommand(cjGet.Name, cjGet);

        var cjList = serviceProvider.GetRequiredService<CopyJobListCommand>();
        copyJob.AddCommand(cjList.Name, cjList);

        var cjCancel = serviceProvider.GetRequiredService<CopyJobCancelCommand>();
        copyJob.AddCommand(cjCancel.Name, cjCancel);

        var cjPause = serviceProvider.GetRequiredService<CopyJobPauseCommand>();
        copyJob.AddCommand(cjPause.Name, cjPause);

        var cjResume = serviceProvider.GetRequiredService<CopyJobResumeCommand>();
        copyJob.AddCommand(cjResume.Name, cjResume);

        var cjComplete = serviceProvider.GetRequiredService<CopyJobCompleteCommand>();
        copyJob.AddCommand(cjComplete.Name, cjComplete);

        return cosmos;
    }
}
