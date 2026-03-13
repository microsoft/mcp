// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DocumentDb.Commands.Collection;
using Azure.Mcp.Tools.DocumentDb.Commands.Connection;
using Azure.Mcp.Tools.DocumentDb.Commands.Database;
// using Azure.Mcp.Tools.DocumentDb.Commands.Document;
// using Azure.Mcp.Tools.DocumentDb.Commands.Index;
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

        // Connection Commands
        services.AddSingleton<ConnectionToggleCommand>();
        services.AddSingleton<GetConnectionStatusCommand>();

        // Database Commands
        services.AddSingleton<ListDatabasesCommand>();
        services.AddSingleton<DbStatsCommand>();
        services.AddSingleton<DropDatabaseCommand>();

        // Collection Commands
        services.AddSingleton<CollectionStatsCommand>();
        services.AddSingleton<RenameCollectionCommand>();
        services.AddSingleton<DropCollectionCommand>();
        services.AddSingleton<SampleDocumentsCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create DocumentDB root command group
        var documentDb = new CommandGroup(
            Name,
            "Azure DocumentDB operations for Azure Cosmos DB for MongoDB (vCore), including connection sessions and database commands.",
            Title);

        // Connection subgroup
        var connection = new CommandGroup(
            "connection",
            "Connection session commands for opening, closing, reconnecting, and checking the active DocumentDB connection.");
        documentDb.AddSubGroup(connection);

        var connectionToggleCommand = serviceProvider.GetRequiredService<ConnectionToggleCommand>();
        var getConnectionStatusCommand = serviceProvider.GetRequiredService<GetConnectionStatusCommand>();

        connection.AddCommand(connectionToggleCommand.Name, connectionToggleCommand);
        connection.AddCommand(getConnectionStatusCommand.Name, getConnectionStatusCommand);

        // Database subgroup
        var database = new CommandGroup(
            "database",
            "Database operations - Commands for managing DocumentDB databases.");
        documentDb.AddSubGroup(database);

        var listDatabasesCommand = serviceProvider.GetRequiredService<ListDatabasesCommand>();
        var dbStatsCommand = serviceProvider.GetRequiredService<DbStatsCommand>();
        var dropDatabaseCommand = serviceProvider.GetRequiredService<DropDatabaseCommand>();

        database.AddCommand(listDatabasesCommand.Name, listDatabasesCommand);
        database.AddCommand(dbStatsCommand.Name, dbStatsCommand);
        database.AddCommand(dropDatabaseCommand.Name, dropDatabaseCommand);

        // Collection subgroup
        var collection = new CommandGroup(
            "collection",
            "Collection operations - Commands for managing DocumentDB collections.");
        documentDb.AddSubGroup(collection);

        var collectionStatsCommand = serviceProvider.GetRequiredService<CollectionStatsCommand>();
        var renameCollectionCommand = serviceProvider.GetRequiredService<RenameCollectionCommand>();
        var dropCollectionCommand = serviceProvider.GetRequiredService<DropCollectionCommand>();
        var sampleDocumentsCommand = serviceProvider.GetRequiredService<SampleDocumentsCommand>();

        collection.AddCommand(collectionStatsCommand.Name, collectionStatsCommand);
        collection.AddCommand(renameCollectionCommand.Name, renameCollectionCommand);
        collection.AddCommand(dropCollectionCommand.Name, dropCollectionCommand);
        collection.AddCommand(sampleDocumentsCommand.Name, sampleDocumentsCommand);

        return documentDb;
    }
}
