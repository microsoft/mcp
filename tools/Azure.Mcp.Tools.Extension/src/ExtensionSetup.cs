// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Extension.Commands;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Extension;

public sealed class ExtensionSetup : IAreaSetup
{
    public string Name => "extension";

    public string Title => "Azure VM Extensions";

    public CommandCategory Category => CommandCategory.RecommendedTools;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClientServices();
        services.AddSingleton<ICliGenerateService, CliGenerateService>();
        services.AddSingleton<AzqrCommand>();
        services.AddSingleton<CliGenerateCommand>();
        services.AddSingleton<ICliInstallService, CliInstallService>();
        services.AddSingleton<CliInstallCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        bool exposeExternalProcessCommands = ShouldExposeExternalProcessCommands(serviceProvider);

        string description = exposeExternalProcessCommands
            ? "Extension commands for additional Azure tooling functionality. Includes running Azure Quick Review (azqr) commands directly from the MCP server to get service recommendations, generating Azure CLI commands from user intent, and getting installation instructions for Azure CLI, Azure Developer CLI and Azure Core Function Tools CLI."
            : "Extension commands for additional Azure tooling functionality. Includes generating Azure CLI commands from user intent, and getting installation instructions for Azure CLI, Azure Developer CLI and Azure Core Function Tools CLI.";

        var extension = new CommandGroup(Name, description, Title);

        if (exposeExternalProcessCommands)
        {
            var azqr = serviceProvider.GetRequiredService<AzqrCommand>();
            extension.AddCommand(azqr.Name, azqr);
        }

        var cli = new CommandGroup("cli", "Commands for helping users to use CLI tools for Azure services operations. Includes operations for generating Azure CLI commands and getting installation instructions for Azure CLI, Azure Developer CLI and Azure Core Function Tools CLI.");
        extension.AddSubGroup(cli);
        var cliGenerateCommand = serviceProvider.GetRequiredService<CliGenerateCommand>();
        cli.AddCommand(cliGenerateCommand.Name, cliGenerateCommand);

        var cliInstallCommand = serviceProvider.GetRequiredService<CliInstallCommand>();
        cli.AddCommand(cliInstallCommand.Name, cliInstallCommand);
        return extension;
    }

    /// <summary>
    /// Determines whether extension commands that use external process execution should be exposed.
    /// External process commands (like azqr) use <see cref="IExternalProcessService"/> to spawn
    /// local processes. In HTTP + On-Behalf-Of mode, spawning child processes on a remote server is a security
    /// risk: processes run under the server's host identity (not the OBO user's context), and malicious or
    /// excessive requests could exhaust resources.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve ServiceStartOptions from.</param>
    /// <returns>True if external process commands should be exposed; false otherwise.</returns>
    private static bool ShouldExposeExternalProcessCommands(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService<ServiceStartOptions>() is ServiceStartOptions startOptions)
        {
            return !startOptions.IsHttpOnBehalfOfMode;
        }

        // ServiceStartOptions is unavailable in the first DI container (CLI routing), where all commands
        // are exposed. See: ConfigureServices method in https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/src/Program.cs
        return true;
    }
}
