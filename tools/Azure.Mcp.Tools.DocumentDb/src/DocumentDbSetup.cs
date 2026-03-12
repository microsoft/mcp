// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// using Azure.Mcp.Tools.DocumentDb.Commands.Collection;
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

        database.AddCommand(
            serviceProvider.GetRequiredService<ListDatabasesCommand>().Name,
            serviceProvider.GetRequiredService<ListDatabasesCommand>());
        database.AddCommand(
            serviceProvider.GetRequiredService<DbStatsCommand>().Name,
            serviceProvider.GetRequiredService<DbStatsCommand>());
        database.AddCommand(
            serviceProvider.GetRequiredService<DropDatabaseCommand>().Name,
            serviceProvider.GetRequiredService<DropDatabaseCommand>());

        return documentDb;
    }
}
