// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Extension.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Extension;

public sealed class ExtensionSetup : IAreaSetup
{
    public string Name => "extension";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<AzqrCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var extension = new CommandGroup(Name, "Extension commands for additional Azure tooling functionality. Includes operations for Azure Quick Review (azqr) commands directly from the MCP server to extend capabilities beyond native Azure service operations.");

        // Azure CLI and Azure Developer CLI tools are hidden
        // extension.AddCommand("az", new AzCommand(loggerFactory.CreateLogger<AzCommand>()));
        var azqr = serviceProvider.GetRequiredService<AzqrCommand>();
        extension.AddCommand(azqr.Name, azqr);

        return extension;
    }
}
