// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.FunctionApp;

public class FunctionAppSetup : IAreaSetup
{
    public string Name => "functionapp";

    public string Title => "Azure Functions";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFunctionAppService, FunctionAppService>();

        services.AddSingleton<FunctionAppCreateCommand>();
        services.AddSingleton<FunctionAppCreateContainerAppCommand>();
        services.AddSingleton<FunctionAppGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var functionApp = new CommandGroup(Name, "Function App operations - Commands for creating, managing, and accessing Azure Function App resources.", Title);

        functionApp.AddCommand<FunctionAppCreateCommand>(serviceProvider);
        functionApp.AddCommand<FunctionAppGetCommand>(serviceProvider);

        var containerApp = new CommandGroup("containerapp", "Container Apps-hosted Function App operations - Commands for creating Azure Function Apps hosted in Azure Container Apps.", "Azure Functions on Container Apps");
        functionApp.AddSubGroup(containerApp);

        containerApp.AddCommand<FunctionAppCreateContainerAppCommand>(serviceProvider);

        return functionApp;
    }
}
