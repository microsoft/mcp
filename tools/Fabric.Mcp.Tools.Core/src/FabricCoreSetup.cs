// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591

using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Fabric.Mcp.Tools.Core;

public sealed class FabricCoreSetup : AreaRegistrationInfo
{
    public override CommandGroupDescriptor CommandDescriptors { get; } = new()
    {
        Name = "core",
        Description = "Microsoft Fabric Core Operations - Create and manage Fabric items. Use this tool when you need to create new Fabric items (Lakehouses, Notebooks, etc.) or manage core Fabric workspace items.",
        Title = "Microsoft Fabric Core",
        Category = "Azure Services",
        Commands =
        [
            new CommandDescriptor
            {
                Id = "d1e2f3a4-b5c6-7890-abcd-ef1234567003",
                Name = "create-item",
                Description = "Creates a new item in a Fabric workspace such as a Lakehouse, Notebook, or other supported item type.",
                Title = "Create Fabric Item",
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = false,
                    OpenWorld = false,
                    ReadOnly = false,
                    LocalRequired = false,
                    Secret = false,
                },
                Options =
                [
                    new OptionDescriptor
                    {
                        Name = "workspace-id",
                        Description = "The workspace ID.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "workspace",
                        Description = "The workspace name.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "display-name",
                        Description = "The display name for the new item.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "item-type",
                        Description = "The type of item to create.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "description",
                        Description = "Description for the new item.",
                        TypeName = "string",
                    },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(ItemCreateCommand)
            },
        ],
    };

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<IFabricCoreService, FabricCoreService>();
        AddCommand<ItemCreateCommand>(services);
    }
}

