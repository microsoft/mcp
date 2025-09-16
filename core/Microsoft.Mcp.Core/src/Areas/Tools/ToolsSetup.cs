// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Tools.Commands;
using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Areas.Tools;

public sealed class ToolsSetup : IAreaSetup
{
    public string Name => "tools";

    public void ConfigureServices(IServiceCollection services)
    {
        // No additional services needed for Tools area
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Tools command group
        var tools = new CommandGroup(Name, "CLI tools operations - Commands for discovering and exploring the functionality available in this CLI tool.");
        rootGroup.AddSubGroup(tools);

        tools.AddCommand("list", new ToolsListCommand(loggerFactory.CreateLogger<ToolsListCommand>()));
    }
}
