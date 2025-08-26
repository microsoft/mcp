// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.AppService.Commands.Database;
using AzureMcp.AppService.Services;
using AzureMcp.Core.Areas;
using AzureMcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMcp.AppService;

public class AppServiceSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAppServiceService, AppServiceService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create AppService command group
        var appService = new CommandGroup("appservice", "App Service operations - Commands for managing Azure App Service resources including web apps, databases, and configurations.");
        rootGroup.AddSubGroup(appService);

        // Create database subgroup
        var database = new CommandGroup("database", "App Service database operations");
        appService.AddSubGroup(database);

        // Add database commands
        database.AddCommand("add", new DatabaseAddCommand(loggerFactory.CreateLogger<DatabaseAddCommand>()));
    }
}
