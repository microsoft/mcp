// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.FunctionApp;

public class FunctionAppSetup : IAreaSetup
{
    public string Name => "functionapp";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFunctionAppService, FunctionAppService>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var functionApp = new CommandGroup(Name, "Function App operations - Commands for managing and accessing Azure Function App resources.");

        functionApp.AddCommand("get", serviceProvider.GetRequiredService<FunctionAppGetCommand>());

        return functionApp;
    }
}
