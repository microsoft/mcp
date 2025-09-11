// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.PublicApi.Commands.BestPractices;
using Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi;

public class FabricPublicApiSetup : IAreaSetup
{
    public string Name => "publicapis";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IResourceProviderService, EmbeddedResourceProviderService>();
        services.AddSingleton<IFabricPublicApiService, FabricPublicApiService>();
        services.AddHttpClient<FabricPublicApiService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
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
        rootGroup.AddSubGroup(fabricPublicApis);

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

        fabricPublicApis.AddCommand("list", new ListWorkloadsCommand(loggerFactory.CreateLogger<ListWorkloadsCommand>()));
        fabricPublicApis.AddCommand("get", new GetWorkloadApisCommand(loggerFactory.CreateLogger<GetWorkloadApisCommand>()));

        platform.AddCommand("get", new GetPlatformApisCommand(loggerFactory.CreateLogger<GetPlatformApisCommand>()));

        bestPractices.AddCommand("get", new GetBestPracticesCommand(loggerFactory.CreateLogger<GetBestPracticesCommand>()));

        examples.AddCommand("get", new GetExamplesCommand(loggerFactory.CreateLogger<GetExamplesCommand>()));

        itemDefinition.AddCommand("get", new GetWorkloadDefinitionCommand(loggerFactory.CreateLogger<GetWorkloadDefinitionCommand>()));
    }
}
