// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Extension.Commands;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Extension;

public sealed class ExtensionSetup : IAreaSetup
{
    public string Name => "extension";

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
        var extension = new CommandGroup(Name, "Extension commands for additional Azure tooling functionality. Includes running Azure Quick Review (azqr) commands directly from the MCP server to get service recommendations, generating Azure CLI commands from user intent, and getting installation instructions for Azure CLI, Azure Developer CLI and Azure Core Function Tools CLI.");

        // Azure CLI and Azure Developer CLI tools are hidden
        // extension.AddCommand("az", new AzCommand(loggerFactory.CreateLogger<AzCommand>()));
        var azqr = serviceProvider.GetRequiredService<AzqrCommand>();
        extension.AddCommand(azqr.Name, azqr);

        var cli = new CommandGroup("cli", "Commands for helping users to use CLI tools for Azure services operations. Includes operations for generating Azure CLI commands and getting installation instructions for Azure CLI, Azure Developer CLI and Azure Core Function Tools CLI.");
        extension.AddSubGroup(cli);
        var cliGenerateCommand = serviceProvider.GetRequiredService<CliGenerateCommand>();
        cli.AddCommand(cliGenerateCommand.Name, cliGenerateCommand);

        var cliInstallCommand = serviceProvider.GetRequiredService<CliInstallCommand>();
        cli.AddCommand(cliInstallCommand.Name, cliInstallCommand);
        return extension;
    }
}
