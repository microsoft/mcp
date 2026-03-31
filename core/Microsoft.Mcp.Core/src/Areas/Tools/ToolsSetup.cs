// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Tools.Commands;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Microsoft.Mcp.Core.Areas.Tools;

public sealed class ToolsSetup : AreaRegistrationInfo
{
    public override CommandGroupDescriptor CommandDescriptors { get; } = new()
    {
        Name = "tools",
        Description = "CLI tools operations - Commands for discovering and exploring the functionality available in this CLI tool.",
        Title = "MCP Tools Discovery",
        Category = "CLI",
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
                HandlerType = nameof(ToolsListCommand)
            },
        ],
    };

    public override void ConfigureServices(IServiceCollection services)
    {
        AddCommand<ToolsListCommand>(services);
    }
}
