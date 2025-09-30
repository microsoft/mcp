// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Core.Areas.Server;

/// <summary>
/// Initializes and configures the Server area for the Azure MCP application.
/// </summary>
public sealed class ServerSetup : IAreaSetup
{
    public string Name => "server";

    /// <summary>
    /// Configures services required for the Server area.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ServiceStartCommand>();
    }

    /// <summary>
    /// Registers command groups and commands related to MCP Server operations.
    /// </summary>
    /// <param name="rootGroup">The root command group to add server commands to.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create MCP Server command group
        var mcpServer = new CommandGroup(Name, "MCP Server operations - Commands for managing and interacting with the MCP Server.");

        // Register MCP Server commands
        var startCommand = serviceProvider.GetRequiredService<ServiceStartCommand>();
        mcpServer.AddCommand(startCommand.Name, startCommand);

        return mcpServer;
    }
}
