// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.BicepSchema.Commands;
using Azure.Mcp.Tools.BicepSchema.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.BicepSchema;

public class BicepSchemaSetup : IAreaSetup
{
    public string Name => "bicepschema";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IBicepSchemaService, BicepSchemaService>();

        services.AddSingleton<BicepSchemaGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var bicepschema = new CommandGroup(Name, "Bicep schema operations - Commands for working with Azure Bicep Infrastructure as Code (IaC) generation and schema management. Includes operations for retrieving Bicep schemas, templates, and resource definitions to support infrastructure deployment automation.");

        // Register Bicep Schema command

        var bicepSchemaGet = serviceProvider.GetRequiredService<BicepSchemaGetCommand>();
        bicepschema.AddCommand(bicepSchemaGet.Name, bicepSchemaGet);

        return bicepschema;
    }
}
