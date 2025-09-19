// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.PublicApi.Commands.BestPractices;
using Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fabric.Mcp.Tools.PublicApi;

public class FabricPublicApiSetup : IAreaSetup
{
    public string Name => "publicapis";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IResourceProviderService, EmbeddedResourceProviderService>();
        services.AddSingleton<IFabricPublicApiService, FabricPublicApiService>();
        services.AddHttpClient<FabricPublicApiService>();

        services.AddSingleton<ListWorkloadsCommand>();
        services.AddSingleton<GetWorkloadApisCommand>();

        services.AddSingleton<GetPlatformApisCommand>();

        services.AddSingleton<GetBestPracticesCommand>();

        services.AddSingleton<GetExamplesCommand>();

        services.AddSingleton<GetWorkloadDefinitionCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var fabricPublicApis = new CommandGroup(Name,
            """
            Microsoft Fabric Public API Access - Retrieve OpenAPI specifications and example files 
            from Microsoft Fabric's official API documentation. Use this tool when you need to:
            - Build applications that integrate with Microsoft Fabric APIs
            - Understand API endpoints, request/response schemas, and authentication requirements
            - Generate client SDKs or API wrappers for Fabric services
            - Review example API calls and responses for development guidance
            - Explore available APIs across different Fabric workloads
            This tool provides read-only access to Microsoft Fabric's public API documentation 
            repository on GitHub. It does NOT interact with live Fabric resources or require 
            authentication - it only retrieves API specifications and examples.
            """);

        // Create public apis subgroups
        var platform = new CommandGroup("platform", "Platform API Operations - Commands for accessing Microsoft Fabric platform-level APIs, including authentication, user management, and tenant configuration.");
        fabricPublicApis.AddSubGroup(platform);

        var bestPractices = new CommandGroup("bestpractices", "API Examples and Best Practices - Commands for retrieving example API request/response files and implementation guidance from Microsoft Fabric's documentation repository.");
        fabricPublicApis.AddSubGroup(bestPractices);

        // Create Best Practices subgroups
        var examples = new CommandGroup("examples", "Example API Files - Commands for retrieving example API request/response files for specific Microsoft Fabric workloads from the official documentation repository.");
        bestPractices.AddSubGroup(examples);

        var itemDefinition = new CommandGroup("itemdefinition", "Workload API Definitions - Commands for retrieving OpenAPI definitions and schema details for specific Microsoft Fabric workloads from the official documentation repository.");
        bestPractices.AddSubGroup(itemDefinition);

        var listWorkloads = serviceProvider.GetRequiredService<ListWorkloadsCommand>();
        fabricPublicApis.AddCommand(listWorkloads.Name, listWorkloads);
        var getWorkloads = serviceProvider.GetRequiredService<GetWorkloadApisCommand>();
        fabricPublicApis.AddCommand(getWorkloads.Name, getWorkloads);

        var getPlatform = serviceProvider.GetRequiredService<GetPlatformApisCommand>();
        platform.AddCommand(getPlatform.Name, getPlatform);

        var getBestPractices = serviceProvider.GetRequiredService<GetBestPracticesCommand>();
        bestPractices.AddCommand(getBestPractices.Name, getBestPractices);

        var getExamples = serviceProvider.GetRequiredService<GetExamplesCommand>();
        examples.AddCommand(getExamples.Name, getExamples);

        var getItemDefinition = serviceProvider.GetRequiredService<GetWorkloadDefinitionCommand>();
        itemDefinition.AddCommand(getItemDefinition.Name, getItemDefinition);

        return fabricPublicApis;
    }
}
