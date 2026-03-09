// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// using Azure.Mcp.Tools.DocumentDb.Commands.Collection;
using Azure.Mcp.Tools.DocumentDb.Commands.Connection;
// using Azure.Mcp.Tools.DocumentDb.Commands.Database;
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
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create DocumentDB root command group
        var documentDb = new CommandGroup(
            Name,
            "Azure DocumentDB operations for Azure Cosmos DB for MongoDB (vCore), including connection sessions and database commands.",
            Title);

        var connection = new CommandGroup(
            "connection",
            "Connection session commands for opening, closing, reconnecting, and checking the active DocumentDB connection.");
        documentDb.AddSubGroup(connection);

        connection.AddCommand(
            serviceProvider.GetRequiredService<ConnectionToggleCommand>().Name,
            serviceProvider.GetRequiredService<ConnectionToggleCommand>());
        connection.AddCommand(
            serviceProvider.GetRequiredService<GetConnectionStatusCommand>().Name,
            serviceProvider.GetRequiredService<GetConnectionStatusCommand>());

        return documentDb;
    }
}