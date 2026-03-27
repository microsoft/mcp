// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.AppService.Commands.Database;
using Azure.Mcp.Tools.AppService.Commands.Webapp;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Deployment;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Settings;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.AppService;

public sealed class AppServiceRegistration : IAreaRegistration
{
    public static string AreaName => "appservice";

    public static string AreaTitle => "Azure App Service";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure App Service operations.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "database",
                Description = "Operations for configuring database connections for Azure App Service web apps",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "14be1264-82c8-4a4c-8271-7cfe1fbebbc8",
                        Name = "add",
                        Description = "Add a database connection for an App Service using connection string for an existing database. This command configures database connection settings for the specified App Service, allowing it to connect to a database server name. You must specify the App Service name, database name, database type, database server name, connection string, resource group name and subscription.",
                        Title = "Add",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = true,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "app",
                                Description = "The name of the Azure App Service (e.g., my-webapp).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "database-type",
                                Description = "The type of database (e.g., SqlServer, MySQL, PostgreSQL, CosmosDB).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "database-server",
                                Description = "The server name or endpoint for the database (e.g., myserver.database.windows.net).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "database",
                                Description = "The name of the database to connect to (e.g., mydb).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "connection-string",
                                Description = "The connection string for the database. If not provided, a default will be generated.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(DatabaseAddCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "webapp",
                Description = "Operations for managing Azure App Service web apps",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "4412f1af-16e7-46db-8305-33e3d7ae06de",
                        Name = "get",
                        Description = "Retrieves detailed information about Azure App Service web apps, including app name, resource group, location, state, hostnames, etc. If a specific app name is not provided, the command will return details for all web apps in a subscription or resource group in a subscription. You can specify the app name, resource group name, and subscription to get details for a specific web app.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "app",
                                Description = "The name of the Azure App Service (e.g., my-webapp).",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(WebappGetCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "deployment",
                        Description = "Operations for managing Azure App Service web app deployments",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "17c59409-5382-4419-aef4-0058ffe2c6ec",
                                Name = "get",
                                Description = "Retrieves detailed information about Azure App Service web app deployments, including deployment name, if deployment is actively happening, when the deployment started and ended, who authored and deployed the deployment, and the type of deployment. If a specific deployment ID is not provided, the command will return details for all deployments in the web app. You can specify a deployment ID to get details for a specific deployment.",
                                Title = "Get",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "resource-group",
                                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "app",
                                        Description = "The name of the Azure App Service (e.g., my-webapp).",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "deployment-id",
                                        Description = "The ID of the deployment.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(WebappGetCommand)
                            },
                        ],
                    },
                    new CommandGroupDescriptor
                    {
                        Name = "diagnostic",
                        Description = "Operations for diagnosing Azure App Service web apps",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "a8aa0966-4c0c-4e22-8854-cced583f0fb2",
                                Name = "diagnose",
                                Description = "Diagnoses an App Service Web App with the specified detector, returning the diagnostic results of the detector.",
                                Title = "Diagnose",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "resource-group",
                                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "app",
                                        Description = "The name of the Azure App Service (e.g., my-webapp).",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "detector-name",
                                        Description = "The name of the diagnostic detector to run (e.g., Availability, CpuAnalysis, MemoryAnalysis).",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "start-time",
                                        Description = "The start time in ISO format (e.g., 2023-01-01T00:00:00Z).",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "end-time",
                                        Description = "The end time in ISO format (e.g., 2023-01-01T00:00:00Z).",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "interval",
                                        Description = "The time interval (e.g., PT1H for 1 hour, PT5M for 5 minutes).",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(DetectorDiagnoseCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "7807fdb6-4b92-4361-8042-be61dd342e17",
                                Name = "list",
                                Description = "Retrieves detailed information about detectors detector for the specified App Service Web App, returning the name, detector type, description, category, and analysis types for each detector.",
                                Title = "List",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "resource-group",
                                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "app",
                                        Description = "The name of the Azure App Service (e.g., my-webapp).",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(DetectorListCommand)
                            },
                        ],
                    },
                    new CommandGroupDescriptor
                    {
                        Name = "settings",
                        Description = "Operations for managing Azure App Service web settings",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "825ef21f-392f-4cd4-8272-7e7dce12e293",
                                Name = "get-appsettings",
                                Description = "Retrieves the application settings for an App Service web app, returning key-value pairs that represent the setting. Application settings may contain sensitive information.",
                                Title = "Get Appsettings",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = true,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "resource-group",
                                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "app",
                                        Description = "The name of the Azure App Service (e.g., my-webapp).",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(AppSettingsGetCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "08ca52a3-f766-4c62-9597-702f629efaf6",
                                Name = "update-appsettings",
                                Description = "Updates the application setting for an App Service web app. Three types of updating are available: - Add: adds a new application setting with the specified name and value. If the application setting already exists, the operation will fail and return an error message. - Set: sets the value of an application setting. If the application setting does not exist, this is equivalent to add. If the application setting already exists, the value will be overwritten. - Delete: deletes an application setting with the specified name. If the application setting does not exist, nothing happens. For add and set update types, both the application setting name and value are required. For delete update type, only the application setting name is required.",
                                Title = "Update Appsettings",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = true,
                                    Idempotent = false,
                                    OpenWorld = false,
                                    ReadOnly = false,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "resource-group",
                                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "app",
                                        Description = "The name of the Azure App Service (e.g., my-webapp).",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "setting-name",
                                        Description = "The name of the application setting.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "setting-value",
                                        Description = "The value of the application setting. Required for add and set update types.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "setting-update-type",
                                        Description = "The type of update to perform on the application setting. Valid values are: add, set, delete.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(AppSettingsUpdateCommand)
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
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

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(DatabaseAddCommand) => serviceProvider.GetRequiredService<DatabaseAddCommand>(),
            nameof(WebappGetCommand) => serviceProvider.GetRequiredService<WebappGetCommand>(),
            nameof(DetectorDiagnoseCommand) => serviceProvider.GetRequiredService<DetectorDiagnoseCommand>(),
            nameof(DetectorListCommand) => serviceProvider.GetRequiredService<DetectorListCommand>(),
            nameof(AppSettingsGetCommand) => serviceProvider.GetRequiredService<AppSettingsGetCommand>(),
            nameof(AppSettingsUpdateCommand) => serviceProvider.GetRequiredService<AppSettingsUpdateCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in appservice area.")
        };
}
