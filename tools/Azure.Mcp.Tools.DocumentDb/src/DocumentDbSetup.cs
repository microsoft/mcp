// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Mcp.Tools.DocumentDb.Commands.Collection;
using Azure.Mcp.Tools.DocumentDb.Commands.Database;
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

        // Other commands
        services.AddSingleton<GetStatsCommand>();
        services.AddSingleton<CurrentOpsCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create DocumentDB root command group
        var documentDb = new CommandGroup(
            Name,
            "Azure DocumentDB index, database, collection, and diagnostics operations for Azure DocumentDB.",
            Title);

        var index = new CommandGroup(
            "index",
            "Manage indexes and inspect index-related diagnostics by providing a DocumentDB connection string per request.");
        var database = new CommandGroup(
            "database",
            "Inspect and manage DocumentDB databases by providing a DocumentDB connection string per request.");
        var collection = new CommandGroup(
            "collection",
            "Manage DocumentDB collections by providing a DocumentDB connection string per request.");
        var others = new CommandGroup(
            "others",
            "Inspect DocumentDB statistics and diagnostic operations by providing a DocumentDB connection string per request.");

        documentDb.AddSubGroup(index);
        documentDb.AddSubGroup(database);
        documentDb.AddSubGroup(collection);
        documentDb.AddSubGroup(others);

        // Index commands
        var createIndexCommand = serviceProvider.GetRequiredService<CreateIndexCommand>();
        var listIndexesCommand = serviceProvider.GetRequiredService<ListIndexesCommand>();
        var dropIndexCommand = serviceProvider.GetRequiredService<DropIndexCommand>();

        // Database commands
        var listDatabasesCommand = serviceProvider.GetRequiredService<ListDatabasesCommand>();
        var dropDatabaseCommand = serviceProvider.GetRequiredService<DropDatabaseCommand>();

        // Collection commands
        var renameCollectionCommand = serviceProvider.GetRequiredService<RenameCollectionCommand>();
        var dropCollectionCommand = serviceProvider.GetRequiredService<DropCollectionCommand>();
        var sampleDocumentsCommand = serviceProvider.GetRequiredService<SampleDocumentsCommand>();

        // Other commands
        var getStatsCommand = serviceProvider.GetRequiredService<GetStatsCommand>();
        var currentOpsCommand = serviceProvider.GetRequiredService<CurrentOpsCommand>();

        index.AddCommand(createIndexCommand.Name, createIndexCommand);
        index.AddCommand(listIndexesCommand.Name, listIndexesCommand);
        index.AddCommand(dropIndexCommand.Name, dropIndexCommand);

        database.AddCommand(listDatabasesCommand.Name, listDatabasesCommand);
        database.AddCommand(dropDatabaseCommand.Name, dropDatabaseCommand);

        collection.AddCommand(renameCollectionCommand.Name, renameCollectionCommand);
        collection.AddCommand(dropCollectionCommand.Name, dropCollectionCommand);
        collection.AddCommand(sampleDocumentsCommand.Name, sampleDocumentsCommand);

        others.AddCommand(getStatsCommand.Name, getStatsCommand);
        others.AddCommand(currentOpsCommand.Name, currentOpsCommand);

        return documentDb;
    }
}
