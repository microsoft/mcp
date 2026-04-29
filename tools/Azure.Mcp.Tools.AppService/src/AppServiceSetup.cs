// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Commands.Database;
using Azure.Mcp.Tools.AppService.Commands.Webapp;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Deployment;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;
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
        services.AddSingleton<DetectorDiagnoseCommand>();
        services.AddSingleton<DetectorListCommand>();
        services.AddSingleton<AppSettingsGetCommand>();
        services.AddSingleton<AppSettingsUpdateCommand>();
        services.AddSingleton<DeploymentGetCommand>();
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
        database.AddCommand(serviceProvider.GetRequiredService<DatabaseAddCommand>());

        // Create webapp subgroup
        var webapp = new CommandGroup("webapp", "Operations for managing Azure App Service web apps");
        appService.AddSubGroup(webapp);

        // Add webapp commands
        webapp.AddCommand(serviceProvider.GetRequiredService<WebappGetCommand>());

        // Add deployment subgroup
        var deployment = new CommandGroup("deployment", "Operations for managing Azure App Service web app deployments");
        webapp.AddSubGroup(deployment);

        // Add deployment commands
        deployment.AddCommand(serviceProvider.GetRequiredService<DeploymentGetCommand>());

        // Add diagnostic subgroup under webapp
        var diagnostic = new CommandGroup("diagnostic", "Operations for diagnosing Azure App Service web apps");
        webapp.AddSubGroup(diagnostic);

        // Add diagnostic commands
        diagnostic.AddCommand(serviceProvider.GetRequiredService<DetectorDiagnoseCommand>());
        diagnostic.AddCommand(serviceProvider.GetRequiredService<DetectorListCommand>());

        // Add settings subgroup under webapp
        var settings = new CommandGroup("settings", "Operations for managing Azure App Service web settings");
        webapp.AddSubGroup(settings);

        // Add settings commands
        settings.AddCommand(serviceProvider.GetRequiredService<AppSettingsGetCommand>());
        settings.AddCommand(serviceProvider.GetRequiredService<AppSettingsUpdateCommand>());

        return appService;
    }
}
