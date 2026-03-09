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
        services.AddSingleton<ConnectDocumentDbCommand>();
        services.AddSingleton<DisconnectDocumentDbCommand>();
        services.AddSingleton<GetConnectionStatusCommand>();
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

        
        return documentDb;
    }
}