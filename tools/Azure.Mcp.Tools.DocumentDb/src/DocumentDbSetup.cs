// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// using Azure.Mcp.Tools.DocumentDb.Commands.Collection;
// using Azure.Mcp.Tools.DocumentDb.Commands.Database;
// using Azure.Mcp.Tools.DocumentDb.Commands.Document;
using Azure.Mcp.Tools.DocumentDb.Commands.Index;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.DocumentDb;

public class DocumentDbSetup : IAreaSetup
{
    public string Name => "documentdb";
    public string Title => "Azure DocumentDB (with MongoDB compatibility)";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDocumentDbService, DocumentDbService>();

        services.AddSingleton<CreateIndexCommand>();
        services.AddSingleton<ListIndexesCommand>();
        services.AddSingleton<DropIndexCommand>();
        services.AddSingleton<IndexStatsCommand>();
        services.AddSingleton<CurrentOpsCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create DocumentDB root command group
        var documentDb = new CommandGroup(
            Name,
            "Azure DocumentDB index and diagnostics operations for Azure Cosmos DB for MongoDB (vCore).",
            Title);

        var index = new CommandGroup(
            "index",
            "Manage indexes and inspect index-related diagnostics by providing a DocumentDB connection string per request.");
        documentDb.AddSubGroup(index);

        var createIndexCommand = serviceProvider.GetRequiredService<CreateIndexCommand>();
        var listIndexesCommand = serviceProvider.GetRequiredService<ListIndexesCommand>();
        var dropIndexCommand = serviceProvider.GetRequiredService<DropIndexCommand>();
        var indexStatsCommand = serviceProvider.GetRequiredService<IndexStatsCommand>();
        var currentOpsCommand = serviceProvider.GetRequiredService<CurrentOpsCommand>();

        index.AddCommand(createIndexCommand.Name, createIndexCommand);
        index.AddCommand(listIndexesCommand.Name, listIndexesCommand);
        index.AddCommand(dropIndexCommand.Name, dropIndexCommand);
        index.AddCommand(indexStatsCommand.Name, indexStatsCommand);
        index.AddCommand(currentOpsCommand.Name, currentOpsCommand);

        return documentDb;
    }
}
