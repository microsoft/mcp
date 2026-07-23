// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Areas.Server;

/// <summary>
/// Initializes and configures the Server area for the MCP application.
/// </summary>
public sealed class ServerSetup : IAreaSetup
{
    public string Name => "server";

    public string Title => "MCP Server Management";

    public CommandCategory Category => CommandCategory.Mcp;

    /// <summary>
    /// Configures services required for the Server area.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ServerStartCommand>();
        services.AddSingleton<ServerInfoCommand>();
        services.AddSingleton<PluginTelemetryCommand>();
    }

    /// <summary>
    /// Registers command groups and commands related to MCP Server operations.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve command instances.</param>
    /// <returns>A CommandGroup containing all registered commands for the Server area.</returns>
    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create MCP Server command group
        var mcpServer = new CommandGroup(Name, "MCP Server operations - Commands for managing and interacting with the MCP Server.", Title);

        // Register MCP Server commands
        mcpServer.AddCommand<ServerStartCommand>(serviceProvider);
        mcpServer.AddCommand<ServerInfoCommand>(serviceProvider);
        mcpServer.AddCommand<PluginTelemetryCommand>(serviceProvider);

        return mcpServer;
    }
}
