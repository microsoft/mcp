// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Mcp.Tools.DocumentDb.Commands.Database;
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
        services.AddSingleton<ListDatabasesCommand>();
        services.AddSingleton<DbStatsCommand>();
        services.AddSingleton<DropDatabaseCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create DocumentDB root command group
        var documentDb = new CommandGroup(
            Name,
            "Azure DocumentDB index, database, and diagnostics operations for Azure DocumentDB.",
            Title);

        var index = new CommandGroup(
            "index",
            "Manage indexes and inspect index-related diagnostics by providing a DocumentDB connection string per request.");
        var database = new CommandGroup(
            "database",
            "Inspect and manage DocumentDB databases by providing a DocumentDB connection string per request.");
        documentDb.AddSubGroup(index);
        documentDb.AddSubGroup(database);

        var createIndexCommand = serviceProvider.GetRequiredService<CreateIndexCommand>();
        var listIndexesCommand = serviceProvider.GetRequiredService<ListIndexesCommand>();
        var dropIndexCommand = serviceProvider.GetRequiredService<DropIndexCommand>();
        var indexStatsCommand = serviceProvider.GetRequiredService<IndexStatsCommand>();
        var currentOpsCommand = serviceProvider.GetRequiredService<CurrentOpsCommand>();
        var listDatabasesCommand = serviceProvider.GetRequiredService<ListDatabasesCommand>();
        var dbStatsCommand = serviceProvider.GetRequiredService<DbStatsCommand>();
        var dropDatabaseCommand = serviceProvider.GetRequiredService<DropDatabaseCommand>();

        index.AddCommand(createIndexCommand.Name, createIndexCommand);
        index.AddCommand(listIndexesCommand.Name, listIndexesCommand);
        index.AddCommand(dropIndexCommand.Name, dropIndexCommand);
        index.AddCommand(indexStatsCommand.Name, indexStatsCommand);
        index.AddCommand(currentOpsCommand.Name, currentOpsCommand);

        database.AddCommand(listDatabasesCommand.Name, listDatabasesCommand);
        database.AddCommand(dbStatsCommand.Name, dbStatsCommand);
        database.AddCommand(dropDatabaseCommand.Name, dropDatabaseCommand);

        return documentDb;
    }
}
