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

        services.AddSingleton<FunctionAppGetCommand>();
        services.AddSingleton<FunctionAppCreateCommand>();
        services.AddSingleton<FunctionAppCreateContainerAppCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var functionApp = new CommandGroup(Name, "Function App operations - Commands for managing and accessing Azure Function App resources.", Title);

        functionApp.AddCommand<FunctionAppGetCommand>(serviceProvider);

        var createCommand = serviceProvider.GetRequiredService<FunctionAppCreateCommand>();
        functionApp.AddCommand(createCommand.Name, createCommand);

        var containerApp = new CommandGroup("containerapp", "Azure Container Apps-hosted Function App operations.", "Azure Functions on Container Apps");
        functionApp.AddSubGroup(containerApp);

        var createContainerAppCommand = serviceProvider.GetRequiredService<FunctionAppCreateContainerAppCommand>();
        containerApp.AddCommand(createContainerAppCommand.Name, createContainerAppCommand);

        return functionApp;
    }
}
