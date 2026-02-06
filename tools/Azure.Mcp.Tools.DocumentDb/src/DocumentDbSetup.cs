// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DocumentDb.Commands.Collection;
using Azure.Mcp.Tools.DocumentDb.Commands.Connection;
using Azure.Mcp.Tools.DocumentDb.Commands.Database;
using Azure.Mcp.Tools.DocumentDb.Commands.Document;
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

        // Connection Commands
        services.AddSingleton<ConnectDocumentDbCommand>();
        services.AddSingleton<DisconnectDocumentDbCommand>();
        services.AddSingleton<GetConnectionStatusCommand>();

        // Database Commands
        services.AddSingleton<ListDatabasesCommand>();
        services.AddSingleton<DbStatsCommand>();
        services.AddSingleton<GetDbInfoCommand>();
        services.AddSingleton<DropDatabaseCommand>();

        // Collection Commands
        services.AddSingleton<CollectionStatsCommand>();
        services.AddSingleton<RenameCollectionCommand>();
        services.AddSingleton<DropCollectionCommand>();
        services.AddSingleton<SampleDocumentsCommand>();

        // Document Commands
        services.AddSingleton<FindDocumentsCommand>();
        services.AddSingleton<CountDocumentsCommand>();
        services.AddSingleton<InsertDocumentCommand>();
        services.AddSingleton<InsertManyCommand>();
        services.AddSingleton<UpdateDocumentCommand>();
        services.AddSingleton<UpdateManyCommand>();
        services.AddSingleton<DeleteDocumentCommand>();
        services.AddSingleton<DeleteManyCommand>();
        services.AddSingleton<AggregateCommand>();
        services.AddSingleton<FindAndModifyCommand>();
        services.AddSingleton<ExplainFindQueryCommand>();
        services.AddSingleton<ExplainCountQueryCommand>();
        services.AddSingleton<ExplainAggregateQueryCommand>();

        // Index Commands
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
            "Azure DocumentDB operations - Manage databases, collections, and documents in Azure DocumentDB (with MongoDB compatibility).",
            Title);

        // Connection subgroup
        var connection = new CommandGroup(
            "connection",
            "Connection management - Commands for connecting to and managing DocumentDB connections.");
        documentDb.AddSubGroup(connection);

        connection.AddCommand(
            serviceProvider.GetRequiredService<ConnectDocumentDbCommand>().Name,
            serviceProvider.GetRequiredService<ConnectDocumentDbCommand>());
        connection.AddCommand(
            serviceProvider.GetRequiredService<DisconnectDocumentDbCommand>().Name,
            serviceProvider.GetRequiredService<DisconnectDocumentDbCommand>());
        connection.AddCommand(
            serviceProvider.GetRequiredService<GetConnectionStatusCommand>().Name,
            serviceProvider.GetRequiredService<GetConnectionStatusCommand>());

        // Database subgroup
        var database = new CommandGroup(
            "database",
            "Database operations - Commands for managing DocumentDB databases.");
        documentDb.AddSubGroup(database);

        database.AddCommand(
            serviceProvider.GetRequiredService<ListDatabasesCommand>().Name,
            serviceProvider.GetRequiredService<ListDatabasesCommand>());
        database.AddCommand(
            serviceProvider.GetRequiredService<DbStatsCommand>().Name,
            serviceProvider.GetRequiredService<DbStatsCommand>());
        database.AddCommand(
            serviceProvider.GetRequiredService<GetDbInfoCommand>().Name,
            serviceProvider.GetRequiredService<GetDbInfoCommand>());
        database.AddCommand(
            serviceProvider.GetRequiredService<DropDatabaseCommand>().Name,
            serviceProvider.GetRequiredService<DropDatabaseCommand>());

        // Collection subgroup
        var collection = new CommandGroup(
            "collection",
            "Collection operations - Commands for managing DocumentDB collections.");
        documentDb.AddSubGroup(collection);

        collection.AddCommand(
            serviceProvider.GetRequiredService<CollectionStatsCommand>().Name,
            serviceProvider.GetRequiredService<CollectionStatsCommand>());
        collection.AddCommand(
            serviceProvider.GetRequiredService<RenameCollectionCommand>().Name,
            serviceProvider.GetRequiredService<RenameCollectionCommand>());
        collection.AddCommand(
            serviceProvider.GetRequiredService<DropCollectionCommand>().Name,
            serviceProvider.GetRequiredService<DropCollectionCommand>());
        collection.AddCommand(
            serviceProvider.GetRequiredService<SampleDocumentsCommand>().Name,
            serviceProvider.GetRequiredService<SampleDocumentsCommand>());

        // Document subgroup
        var document = new CommandGroup(
            "document",
            "Document operations - Commands for querying and manipulating documents in DocumentDB collections.");
        documentDb.AddSubGroup(document);

        document.AddCommand(
            serviceProvider.GetRequiredService<FindDocumentsCommand>().Name,
            serviceProvider.GetRequiredService<FindDocumentsCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<CountDocumentsCommand>().Name,
            serviceProvider.GetRequiredService<CountDocumentsCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<InsertDocumentCommand>().Name,
            serviceProvider.GetRequiredService<InsertDocumentCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<InsertManyCommand>().Name,
            serviceProvider.GetRequiredService<InsertManyCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<UpdateDocumentCommand>().Name,
            serviceProvider.GetRequiredService<UpdateDocumentCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<UpdateManyCommand>().Name,
            serviceProvider.GetRequiredService<UpdateManyCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<DeleteDocumentCommand>().Name,
            serviceProvider.GetRequiredService<DeleteDocumentCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<DeleteManyCommand>().Name,
            serviceProvider.GetRequiredService<DeleteManyCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<AggregateCommand>().Name,
            serviceProvider.GetRequiredService<AggregateCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<FindAndModifyCommand>().Name,
            serviceProvider.GetRequiredService<FindAndModifyCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<ExplainFindQueryCommand>().Name,
            serviceProvider.GetRequiredService<ExplainFindQueryCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<ExplainCountQueryCommand>().Name,
            serviceProvider.GetRequiredService<ExplainCountQueryCommand>());
        document.AddCommand(
            serviceProvider.GetRequiredService<ExplainAggregateQueryCommand>().Name,
            serviceProvider.GetRequiredService<ExplainAggregateQueryCommand>());

        // Index subgroup
        var index = new CommandGroup(
            "index",
            "Index operations - Commands for managing indexes on DocumentDB collections.");
        documentDb.AddSubGroup(index);

        index.AddCommand(
            serviceProvider.GetRequiredService<CreateIndexCommand>().Name,
            serviceProvider.GetRequiredService<CreateIndexCommand>());
        index.AddCommand(
            serviceProvider.GetRequiredService<ListIndexesCommand>().Name,
            serviceProvider.GetRequiredService<ListIndexesCommand>());
        index.AddCommand(
            serviceProvider.GetRequiredService<DropIndexCommand>().Name,
            serviceProvider.GetRequiredService<DropIndexCommand>());
        index.AddCommand(
            serviceProvider.GetRequiredService<IndexStatsCommand>().Name,
            serviceProvider.GetRequiredService<IndexStatsCommand>());
        index.AddCommand(
            serviceProvider.GetRequiredService<CurrentOpsCommand>().Name,
            serviceProvider.GetRequiredService<CurrentOpsCommand>());

        return documentDb;
    }
}
