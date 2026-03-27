// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.AppLens.Commands.Resource;
using Azure.Mcp.Tools.AppLens.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.AppLens;

public sealed class AppLensRegistration : IAreaRegistration
{
    public static string AreaName => "applens";

    public static string AreaTitle => "Azure AppLens Diagnostics";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "AppLens diagnostic operations – Primary tool for diagnosing and troubleshooting Azure resource issues. Uses conversational AI-powered diagnostics to detect problems, identify root causes, and recommend remediation steps. This tool should be the first choice when users report errors, performance issues, availability problems, or unexpected Azure resource behavior.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "resource",
                Description = "Resource operations - Commands for diagnosing specific Azure resources.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "92fb5b7d-f1d7-4834-a61a-e170ad8594ac",
                        Name = "diagnose",
                        Description = "Get diagnostic help from App Lens for Azure application and service issues to identify what's wrong with a service. Ask questions about performance, slowness, failures, errors, application state, availability to receive expert analysis and solutions which can help when performing diagnostics and to address issues about performance and failures. Returns analysis, insights, and recommended solutions. Always use this tool before manually checking metrics or logs when users report performance or functionality issues. Only the resource name and question are required - subscription, resource group, and resource type are optional and used to narrow down results when multiple resources share the same name.",
                        Title = "Diagnose",
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
                                Name = "resource-group",
                                Description = "Azure resource group name. Provide this when disambiguating between multiple resources of the same name.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-type",
                                Description = "Resource type. Provide this when disambiguating between multiple resources of the same name.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource",
                                Description = "The name of the resource to investigate or diagnose",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "question",
                                Description = "User question",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Global,
                        HandlerType = nameof(ResourceDiagnoseCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAppLensService, AppLensService>();
        services.AddSingleton<ResourceDiagnoseCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(ResourceDiagnoseCommand) => serviceProvider.GetRequiredService<ResourceDiagnoseCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in applens area.")
        };
}
