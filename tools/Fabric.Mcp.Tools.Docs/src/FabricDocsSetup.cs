// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Docs.Commands.BestPractices;
using Fabric.Mcp.Tools.Docs.Commands.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Docs;

public class FabricDocsSetup : IAreaSetup
{
    public string Name => "docs";

    public string Title => "Microsoft Fabric Documentation";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IResourceProviderService, EmbeddedResourceProviderService>();
        services.AddSingleton<IFabricPublicApiService, FabricPublicApiService>();
        services.AddHttpClient<FabricPublicApiService>();

        services.AddSingleton<ListItemTypesCommand>();
        services.AddSingleton<GetItemApisCommand>();
        services.AddSingleton<GetPlatformApisCommand>();
        services.AddSingleton<GetBestPracticesCommand>();
        services.AddSingleton<GetExamplesCommand>();
        services.AddSingleton<GetItemDefinitionCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var fabricDocs = new CommandGroup(Name,
            """
            Microsoft Fabric Documentation Tools - Access OpenAPI specifications, best practices,
            and example files for Microsoft Fabric APIs. Use this tool when you need to:
            - Discover available Fabric item types and their API specifications
            - Retrieve detailed OpenAPI documentation for specific item types
            - Access best practice guidance for Fabric development
            - Get example API request/response files for implementation reference
            This tool provides read-only access to Microsoft Fabric documentation and does NOT
            interact with live Fabric resources or require authentication.
            """, Title);

        // Register all commands directly at the docs level (flat structure)
        fabricDocs.AddCommand<ListItemTypesCommand>(serviceProvider);
        fabricDocs.AddCommand<GetItemApisCommand>(serviceProvider);
        fabricDocs.AddCommand<GetPlatformApisCommand>(serviceProvider);
        fabricDocs.AddCommand<GetItemDefinitionCommand>(serviceProvider);
        fabricDocs.AddCommand<GetBestPracticesCommand>(serviceProvider);
        fabricDocs.AddCommand<GetExamplesCommand>(serviceProvider);

        return fabricDocs;
    }
}
