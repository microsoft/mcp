// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591

using Fabric.Mcp.Tools.Docs.Commands.BestPractices;
using Fabric.Mcp.Tools.Docs.Commands.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Fabric.Mcp.Tools.Docs;

public sealed class FabricDocsRegistration : IAreaRegistration
{
    public static string AreaName => "docs";

    public static string AreaTitle => "Microsoft Fabric Documentation";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Microsoft Fabric Documentation Tools - Access OpenAPI specifications, best practices, and example files for Microsoft Fabric APIs. Use this tool when you need to discover available Fabric workload types and their API specifications, retrieve detailed OpenAPI documentation for specific workloads, access best practice guidance for Fabric development, or get example API request/response files for implementation reference. This tool provides read-only access to Microsoft Fabric documentation and does NOT interact with live Fabric resources or require authentication.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "e1f2a3b4-c5d6-7890-abcd-ef1234567004",
                Name = "workloads",
                Description = "Lists Fabric workload types that have public API specifications available.",
                Title = "Available Fabric Workloads",
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = true,
                    LocalRequired = false,
                    Secret = false,
                },
                Options = [],
                Kind = CommandKind.Global,
                HandlerType = nameof(ListWorkloadsCommand)
            },
            new CommandDescriptor
            {
                Id = "e1f2a3b4-c5d6-7890-abcd-ef1234567005",
                Name = "workload-api-spec",
                Description = "Retrieves the complete OpenAPI specification for a specific Fabric workload.",
                Title = "Workload API Specification",
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = true,
                    LocalRequired = false,
                    Secret = false,
                },
                Options =
                [
                    new OptionDescriptor
                    {
                        Name = "workload-type",
                        Description = "The Fabric workload type.",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Global,
                HandlerType = nameof(GetWorkloadApisCommand)
            },
            new CommandDescriptor
            {
                Id = "e1f2a3b4-c5d6-7890-abcd-ef1234567006",
                Name = "platform-api-spec",
                Description = "Retrieves OpenAPI specification for core Fabric platform APIs.",
                Title = "Platform API Specification",
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = true,
                    LocalRequired = false,
                    Secret = false,
                },
                Options = [],
                Kind = CommandKind.Global,
                HandlerType = nameof(GetPlatformApisCommand)
            },
            new CommandDescriptor
            {
                Id = "e1f2a3b4-c5d6-7890-abcd-ef1234567007",
                Name = "item-definitions",
                Description = "Retrieves JSON schema definitions for items in a Fabric workload API.",
                Title = "Item Definitions",
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = true,
                    LocalRequired = false,
                    Secret = false,
                },
                Options =
                [
                    new OptionDescriptor
                    {
                        Name = "workload-type",
                        Description = "The Fabric workload type.",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Global,
                HandlerType = nameof(GetWorkloadDefinitionCommand)
            },
            new CommandDescriptor
            {
                Id = "e1f2a3b4-c5d6-7890-abcd-ef1234567008",
                Name = "best-practices",
                Description = "Retrieves embedded best practice documentation for a specific Fabric topic.",
                Title = "Best Practices",
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = true,
                    LocalRequired = false,
                    Secret = false,
                },
                Options =
                [
                    new OptionDescriptor
                    {
                        Name = "topic",
                        Description = "The Fabric topic to get best practices for.",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Global,
                HandlerType = nameof(GetBestPracticesCommand)
            },
            new CommandDescriptor
            {
                Id = "e1f2a3b4-c5d6-7890-abcd-ef1234567009",
                Name = "api-examples",
                Description = "Retrieves example API request and response files for a Fabric workload.",
                Title = "API Examples",
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = true,
                    LocalRequired = false,
                    Secret = false,
                },
                Options =
                [
                    new OptionDescriptor
                    {
                        Name = "workload-type",
                        Description = "The Fabric workload type.",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Global,
                HandlerType = nameof(GetExamplesCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
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

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(ListWorkloadsCommand) => serviceProvider.GetRequiredService<ListWorkloadsCommand>(),
            nameof(GetWorkloadApisCommand) => serviceProvider.GetRequiredService<GetWorkloadApisCommand>(),
            nameof(GetPlatformApisCommand) => serviceProvider.GetRequiredService<GetPlatformApisCommand>(),
            nameof(GetBestPracticesCommand) => serviceProvider.GetRequiredService<GetBestPracticesCommand>(),
            nameof(GetExamplesCommand) => serviceProvider.GetRequiredService<GetExamplesCommand>(),
            nameof(GetWorkloadDefinitionCommand) => serviceProvider.GetRequiredService<GetWorkloadDefinitionCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{handlerTypeName}' in docs area.")
        };
}
