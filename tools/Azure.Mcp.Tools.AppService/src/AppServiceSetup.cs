// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Commands.Database;
using Azure.Mcp.Tools.AppService.Commands.Webapp;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;
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
        services.AddSingleton<WebappDiagnosticCategoryGetCommand>();
        services.AddSingleton<WebappDetectorGetCommand>();
        services.AddSingleton<WebappAnalysisGetCommand>();
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
        var webappsGet = serviceProvider.GetRequiredService<WebappGetCommand>();
        webapp.AddCommand(webappsGet.Name, webappsGet);

        // Create diagnostic subgroup under webapps
        var diagnostic = new CommandGroup("diagnostic", "Operations for retrieving diagnostics information for Azure App Service web apps");
        webapp.AddSubGroup(diagnostic);

        // Add diagnostic commands
        var diagnosticCategoryGet = serviceProvider.GetRequiredService<WebappDiagnosticCategoryGetCommand>();
        diagnostic.AddCommand(diagnosticCategoryGet.Name, diagnosticCategoryGet);

        var detectorGet = serviceProvider.GetRequiredService<WebappDetectorGetCommand>();
        diagnostic.AddCommand(detectorGet.Name, detectorGet);

        var analysisGet = serviceProvider.GetRequiredService<WebappAnalysisGetCommand>();
        diagnostic.AddCommand(analysisGet.Name, analysisGet);

        return appService;
    }
}
