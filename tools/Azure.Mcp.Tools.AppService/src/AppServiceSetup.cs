// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AppService.Commands.Database;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppService;

public class AppServiceSetup : IAreaSetup
{
    public string Name => "appservice";
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
