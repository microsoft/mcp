// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.PublicApi.Commands.BestPractices;
using Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.PublicApi;

public class FabricDocsSetup : IAreaSetup
{
    public string Name => "docs";

    public string Title => "Microsoft Fabric Documentation";

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
        var fabricDocs = new CommandGroup(Name,
            """
            Microsoft Fabric Documentation Tools - Access OpenAPI specifications, best practices,
            and example files for Microsoft Fabric APIs. Use this tool when you need to:
            - Discover available Fabric workload types and their API specifications
            - Retrieve detailed OpenAPI documentation for specific workloads
            - Access best practice guidance for Fabric development
            - Get example API request/response files for implementation reference
            This tool provides read-only access to Microsoft Fabric documentation and does NOT
            interact with live Fabric resources or require authentication.
            """, Title);

        // Register all commands directly at the docs level (flat structure)
        var listWorkloads = serviceProvider.GetRequiredService<ListWorkloadsCommand>();
        fabricDocs.AddCommand(listWorkloads.Name, listWorkloads);

        var getApiSpec = serviceProvider.GetRequiredService<GetWorkloadApisCommand>();
        fabricDocs.AddCommand(getApiSpec.Name, getApiSpec);

        var getPlatformApiSpec = serviceProvider.GetRequiredService<GetPlatformApisCommand>();
        fabricDocs.AddCommand(getPlatformApiSpec.Name, getPlatformApiSpec);

        var getItemDefinition = serviceProvider.GetRequiredService<GetWorkloadDefinitionCommand>();
        fabricDocs.AddCommand(getItemDefinition.Name, getItemDefinition);

        var getBestPractices = serviceProvider.GetRequiredService<GetBestPracticesCommand>();
        fabricDocs.AddCommand(getBestPractices.Name, getBestPractices);

        var getExamples = serviceProvider.GetRequiredService<GetExamplesCommand>();
        fabricDocs.AddCommand(getExamples.Name, getExamples);

        return fabricDocs;
    }
}