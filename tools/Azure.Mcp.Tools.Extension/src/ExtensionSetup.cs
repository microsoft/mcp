// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Extension.Commands;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
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
        var extension = new CommandGroup(Name,
            """
            Extension operations – Commands providing additional Azure tooling
            functionality, including running Azure Quick Review (azqr) for service
            recommendations, generating Azure CLI commands from user intent, and
            retrieving installation guidance for Azure CLI, Azure Developer CLI, and
            Azure Functions Core Tools.
            """, Title);

        // Azure CLI and Azure Developer CLI tools are hidden
        // extension.AddCommand("az", new AzCommand(loggerFactory.CreateLogger<AzCommand>()));
        var azqr = serviceProvider.GetRequiredService<AzqrCommand>();
        extension.AddCommand(azqr.Name, azqr);

        var cli = new CommandGroup("cli",
            """
            Commands for helping users to use CLI tools for Azure services
            operations. Includes operations for generating Azure CLI commands and
            getting installation instructions for Azure CLI, Azure Developer CLI and
            Azure Core Function Tools CLI.
            """);
        extension.AddSubGroup(cli);
        var cliGenerateCommand = serviceProvider.GetRequiredService<CliGenerateCommand>();
        cli.AddCommand(cliGenerateCommand.Name, cliGenerateCommand);

        var cliInstallCommand = serviceProvider.GetRequiredService<CliInstallCommand>();
        cli.AddCommand(cliInstallCommand.Name, cliInstallCommand);
        return extension;
    }
}
