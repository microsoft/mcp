// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Tools.Commands;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Core.Areas.Tools;

public sealed class ToolsSetup : IAreaSetup
{
    public string Name => "tools";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ToolsListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Tools command group
        var tools = new CommandGroup(Name, "CLI tools operations - Commands for discovering and exploring the functionality available in this CLI tool.");

        var list = serviceProvider.GetRequiredService<ToolsListCommand>();
        tools.AddCommand(list.Name, list);

        return tools;
    }
}
