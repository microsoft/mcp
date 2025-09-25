// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AppService.Commands.Database;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.AppService;

public class AppServiceSetup : IAreaSetup
{
    public string Name => "appservice";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAppServiceService, AppServiceService>();
        services.AddSingleton<DatabaseAddCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create AppService command group
        var appService = new CommandGroup("appservice", "App Service operations - Commands for managing Azure App Service resources including web apps, databases, and configurations.");

        // Create database subgroup
        var database = new CommandGroup("database", "Operations for configuring database connections for Azure App Service web apps");
        appService.AddSubGroup(database);

        // Add database commands
        // Register the 'add' command for database connections, allowing users to configure a new database connection for an App Service web app.
        var databaseAdd = serviceProvider.GetRequiredService<DatabaseAddCommand>();
        database.AddCommand(databaseAdd.Name, databaseAdd);

        return appService;
    }
}
