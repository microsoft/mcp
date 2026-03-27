// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Core.Areas.Tools.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Core.Areas.Tools;

public sealed class ToolsRegistration : IAreaRegistration
{
    public static string AreaName => "tools";

    public static string AreaTitle => "MCP Tools Discovery";

    public static CommandCategory Category => CommandCategory.Cli;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "CLI tools operations - Commands for discovering and exploring the functionality available in this CLI tool.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "63de05a7-047d-4f8a-86ea-cebd64527e2b",
                Name = "list",
                Description = "Lists all available commands and their tools in a hierarchical structure. Supports filtering by namespace and name-only output mode.",
                Title = "List Available Tools",
                Hidden = true,
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
                        Name = "namespace-mode",
                        Description = "If specified, returns a list of top-level service namespaces instead of individual tools.",
                        TypeName = "boolean",
                    },
                    new OptionDescriptor
                    {
                        Name = "namespace",
                        Description = "Filter tools by namespace. Can be specified multiple times to include multiple namespaces.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "name-only",
                        Description = "If specified, returns only tool names without descriptions or metadata.",
                        TypeName = "boolean",
                    },
                ],
                Kind = CommandKind.Basic,
                HandlerType = nameof(ToolsListCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ToolsListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(ToolsListCommand) => serviceProvider.GetRequiredService<ToolsListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{handlerTypeName}' in tools area.")
        };
}
