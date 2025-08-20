// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Tools.Commands;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Areas.Tools;

public sealed class ToolsSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // No additional services needed for Tools area
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Tools command group
        var tools = new CommandGroup("tools", "CLI tools operations - Commands for discovering and exploring the functionality available in this CLI tool.");
        rootGroup.AddSubGroup(tools);

        tools.AddCommand("list", new ToolsListCommand(loggerFactory.CreateLogger<ToolsListCommand>()));
    }
}
