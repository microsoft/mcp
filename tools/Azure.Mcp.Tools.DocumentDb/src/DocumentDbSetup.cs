// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Mcp.Tools.DocumentDb.Commands.Collection;
using Azure.Mcp.Tools.DocumentDb.Commands.Database;
using Azure.Mcp.Tools.DocumentDb.Commands.Document;
using Azure.Mcp.Tools.DocumentDb.Commands.Index;
using Azure.Mcp.Tools.DocumentDb.Commands.Others;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.DocumentDb;

public sealed class DocumentDbSetup : IAreaSetup
{
    public string Name => "documentdb";
    public string Title => "Azure DocumentDB (with MongoDB compatibility)";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDocumentDbService, DocumentDbService>();

        // Index commands
        services.AddSingleton<CreateIndexCommand>();
        services.AddSingleton<ListIndexesCommand>();
        services.AddSingleton<DropIndexCommand>();

        // Database commands
        services.AddSingleton<ListDatabasesCommand>();
        services.AddSingleton<DropDatabaseCommand>();

        // Collection commands
        services.AddSingleton<RenameCollectionCommand>();
        services.AddSingleton<DropCollectionCommand>();
        services.AddSingleton<SampleDocumentsCommand>();

        // Document Commands
        services.AddSingleton<FindDocumentsCommand>();
        services.AddSingleton<CountDocumentsCommand>();
        services.AddSingleton<InsertDocumentsCommand>();
        services.AddSingleton<UpdateDocumentsCommand>();
        services.AddSingleton<DeleteDocumentsCommand>();
        services.AddSingleton<AggregateCommand>();
        services.AddSingleton<FindAndModifyCommand>();
        services.AddSingleton<ExplainQueryCommand>();

        // Other commands
        services.AddSingleton<GetStatsCommand>();
        services.AddSingleton<CurrentOpsCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var documentDb = new CommandGroup(
            Name,
            "Index, database, collection, document, and diagnostics operations for Azure DocumentDB (with MongoDB compatibility).",
            Title);

        var index = new CommandGroup(
            "index",
            "Manage indexes and inspect index-related diagnostics by providing an Azure DocumentDB connection string per request.");
        var database = new CommandGroup(
            "database",
            "Inspect and manage Azure DocumentDB databases by providing an Azure DocumentDB connection string per request.");
        var collection = new CommandGroup(
            "collection",
            "Manage Azure DocumentDB collections by providing an Azure DocumentDB connection string per request.");
        var document = new CommandGroup(
            "document",
            "Query and manipulate documents in Azure DocumentDB collections.");
        var others = new CommandGroup(
            "others",
            "Inspect Azure DocumentDB statistics and diagnostic operations by providing an Azure DocumentDB connection string per request.");

        documentDb.AddSubGroup(index);
        documentDb.AddSubGroup(database);
        documentDb.AddSubGroup(collection);
        documentDb.AddSubGroup(document);
        documentDb.AddSubGroup(others);

        // Index commands
        var createIndexCommand = serviceProvider.GetRequiredService<CreateIndexCommand>();
        var listIndexesCommand = serviceProvider.GetRequiredService<ListIndexesCommand>();
        var dropIndexCommand = serviceProvider.GetRequiredService<DropIndexCommand>();
        index.AddCommand(createIndexCommand.Name, createIndexCommand);
        index.AddCommand(listIndexesCommand.Name, listIndexesCommand);
        index.AddCommand(dropIndexCommand.Name, dropIndexCommand);

        // Database commands
        var listDatabasesCommand = serviceProvider.GetRequiredService<ListDatabasesCommand>();
        var dropDatabaseCommand = serviceProvider.GetRequiredService<DropDatabaseCommand>();
        database.AddCommand(listDatabasesCommand.Name, listDatabasesCommand);
        database.AddCommand(dropDatabaseCommand.Name, dropDatabaseCommand);

        // Collection commands
        var renameCollectionCommand = serviceProvider.GetRequiredService<RenameCollectionCommand>();
        var dropCollectionCommand = serviceProvider.GetRequiredService<DropCollectionCommand>();
        var sampleDocumentsCommand = serviceProvider.GetRequiredService<SampleDocumentsCommand>();
        collection.AddCommand(renameCollectionCommand.Name, renameCollectionCommand);
        collection.AddCommand(dropCollectionCommand.Name, dropCollectionCommand);
        collection.AddCommand(sampleDocumentsCommand.Name, sampleDocumentsCommand);

        // Document commands
        var findDocumentsCommand = serviceProvider.GetRequiredService<FindDocumentsCommand>();
        var countDocumentsCommand = serviceProvider.GetRequiredService<CountDocumentsCommand>();
        var insertDocumentsCommand = serviceProvider.GetRequiredService<InsertDocumentsCommand>();
        var updateDocumentsCommand = serviceProvider.GetRequiredService<UpdateDocumentsCommand>();
        var deleteDocumentsCommand = serviceProvider.GetRequiredService<DeleteDocumentsCommand>();
        var aggregateCommand = serviceProvider.GetRequiredService<AggregateCommand>();
        var findAndModifyCommand = serviceProvider.GetRequiredService<FindAndModifyCommand>();
        var explainQueryCommand = serviceProvider.GetRequiredService<ExplainQueryCommand>();
        document.AddCommand(findDocumentsCommand.Name, findDocumentsCommand);
        document.AddCommand(countDocumentsCommand.Name, countDocumentsCommand);
        document.AddCommand(insertDocumentsCommand.Name, insertDocumentsCommand);
        document.AddCommand(updateDocumentsCommand.Name, updateDocumentsCommand);
        document.AddCommand(deleteDocumentsCommand.Name, deleteDocumentsCommand);
        document.AddCommand(aggregateCommand.Name, aggregateCommand);
        document.AddCommand(findAndModifyCommand.Name, findAndModifyCommand);
        document.AddCommand(explainQueryCommand.Name, explainQueryCommand);

        // Other commands
        var getStatsCommand = serviceProvider.GetRequiredService<GetStatsCommand>();
        var currentOpsCommand = serviceProvider.GetRequiredService<CurrentOpsCommand>();
        others.AddCommand(getStatsCommand.Name, getStatsCommand);
        others.AddCommand(currentOpsCommand.Name, currentOpsCommand);

        return documentDb;
    }
}
