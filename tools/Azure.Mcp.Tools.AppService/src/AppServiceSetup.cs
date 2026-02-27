// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Commands.Database;
using Azure.Mcp.Tools.AppService.Commands.Webapp;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Settings;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AppService;

public class AppServiceSetup : IAreaSetup
{
    public string Name => "appservice";

    public string Title => "Azure App Service";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAppServiceService, AppServiceService>();
        services.AddSingleton<DatabaseAddCommand>();
        services.AddSingleton<WebappGetCommand>();
        services.AddSingleton<AppSettingsGetCommand>();
        services.AddSingleton<AppSettingsUpdateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create AppService command group
        var appService = new CommandGroup("appservice", "App Service operations - Commands for managing Azure App Service resources including web apps, databases, and configurations.", Title);

        // Create database subgroup
        var database = new CommandGroup("database", "Operations for configuring database connections for Azure App Service web apps");
        appService.AddSubGroup(database);

        // Add database commands
        // Register the 'add' command for database connections, allowing users to configure a new database connection for an App Service web app.
        var databaseAdd = serviceProvider.GetRequiredService<DatabaseAddCommand>();
        database.AddCommand(databaseAdd.Name, databaseAdd);

        // Create webapp subgroup
        var webapp = new CommandGroup("webapp", "Operations for managing Azure App Service web apps");
        appService.AddSubGroup(webapp);

        // Add webapp commands
        var webappGet = serviceProvider.GetRequiredService<WebappGetCommand>();
        webapp.AddCommand(webappGet.Name, webappGet);

        // Add settings subgroup under webapp
        var settings = new CommandGroup("settings", "Operations for managing Azure App Service web settings");
        webapp.AddSubGroup(settings);

        // Add settings commands
        var appSettingsGet = serviceProvider.GetRequiredService<AppSettingsGetCommand>();
        settings.AddCommand(appSettingsGet.Name, appSettingsGet);

        var appSettingsUpdate = serviceProvider.GetRequiredService<AppSettingsUpdateCommand>();
        settings.AddCommand(appSettingsUpdate.Name, appSettingsUpdate);

        return appService;
    }
}
