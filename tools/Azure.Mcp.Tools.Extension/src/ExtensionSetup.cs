// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Extension.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Extension;

public sealed class ExtensionSetup : IAreaSetup
{
    public string Name => "extension";

    public void ConfigureServices(IServiceCollection services)
    {
        // No additional services needed for Extension area
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var extension = new CommandGroup(Name, "Extension commands for additional Azure tooling functionality. Includes operations for Azure Quick Review (azqr) commands directly from the MCP server to extend capabilities beyond native Azure service operations.");
        rootGroup.AddSubGroup(extension);

        // Azure CLI and Azure Developer CLI tools are hidden
        // extension.AddCommand("az", new AzCommand(loggerFactory.CreateLogger<AzCommand>()));
        // extension.AddCommand("azd", new AzdCommand(loggerFactory.CreateLogger<AzdCommand>()));
        extension.AddCommand("azqr", new AzqrCommand(loggerFactory.CreateLogger<AzqrCommand>()));
    }
}
