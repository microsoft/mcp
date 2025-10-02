// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Services.Http;
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
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var extension = new CommandGroup(Name, "Extension commands for additional Azure tooling functionality. Includes operations for Azure Quick Review (azqr) commands directly from the MCP server to extend capabilities beyond native Azure service operations.");

        // Azure CLI and Azure Developer CLI tools are hidden
        // extension.AddCommand("az", new AzCommand(loggerFactory.CreateLogger<AzCommand>()));
        var azqr = serviceProvider.GetRequiredService<AzqrCommand>();
        extension.AddCommand(azqr.Name, azqr);

        var cli = new CommandGroup("cli", "Commands for helping users to use CLI tools for Azure services operations. Includes operations for generating Azure CLI commands.");
        extension.AddSubGroup(cli);
        var cliGenerateCommand = serviceProvider.GetRequiredService<CliGenerateCommand>();
        cli.AddCommand(cliGenerateCommand.Name, cliGenerateCommand);
        return extension;
    }
}
