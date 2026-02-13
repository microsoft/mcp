// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.BicepSchema.Commands;
using Azure.Mcp.Tools.BicepSchema.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.BicepSchema;

public class BicepSchemaSetup : IAreaSetup
{
    public string Name => "bicepschema";

    public string Title => "Azure Bicep Schema";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IBicepSchemaService, BicepSchemaService>();

        services.AddSingleton<BicepSchemaGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var bicepschema = new CommandGroup(Name, "Bicep schema operations – Commands to work with Azure Bicep Infrastructure as Code (IaC) schemas and templates, including retrieving Bicep schema and resource definitions to support infrastructure deployment automation.", Title);

        // Register Bicep Schema command

        var bicepSchemaGet = serviceProvider.GetRequiredService<BicepSchemaGetCommand>();
        bicepschema.AddCommand(bicepSchemaGet.Name, bicepSchemaGet);

        return bicepschema;
    }
}
