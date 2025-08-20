// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Areas.Server;

/// <summary>
/// Initializes and configures the Server area for the Azure MCP application.
/// </summary>
public sealed class ServerSetup : IAreaSetup
{
    /// <summary>
    /// Configures services required for the Server area.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        // No additional services needed for Server area
    }

    /// <summary>
    /// Registers command groups and commands related to MCP Server operations.
    /// </summary>
    /// <param name="rootGroup">The root command group to add server commands to.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create MCP Server command group
        var mcpServer = new CommandGroup("server", "MCP Server operations - Commands for managing and interacting with the MCP Server.");
        rootGroup.AddSubGroup(mcpServer);

        // Register MCP Server commands
        var startServer = new ServiceStartCommand();
        mcpServer.AddCommand("start", startServer);
    }
}
