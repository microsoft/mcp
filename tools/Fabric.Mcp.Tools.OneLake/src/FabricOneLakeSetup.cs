// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591

using Fabric.Mcp.Tools.OneLake.Commands;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Commands.Item;
using Fabric.Mcp.Tools.OneLake.Commands.Table;
using Fabric.Mcp.Tools.OneLake.Commands.Workspace;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Fabric.Mcp.Tools.OneLake;

public sealed class FabricOneLakeSetup : AreaRegistrationInfo
{
    public override CommandGroupDescriptor CommandDescriptors { get; } = new()
    {
        Name = "onelake",
        Description = "Microsoft Fabric OneLake Operations - Manage and interact with OneLake data lake storage. OneLake is Microsoft Fabric's built-in data lake that provides unified storage for all analytics workloads.",
        Title = "Microsoft Fabric OneLake",
        Category = "Azure Services",
        Commands =
        [
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567010",
                Name = "list_workspaces",
                Description = "Lists all Fabric workspaces accessible via OneLake data plane API.",
                Title = "List OneLake Workspaces",
                Annotations = new ToolAnnotations { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "continuation-token", Description = "Continuation token for paging.", TypeName = "string" },
                    new OptionDescriptor { Name = "format", Description = "Output format.", TypeName = "string" },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(OneLakeWorkspaceListCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567011",
                Name = "list_items",
                Description = "Lists OneLake items in a Fabric workspace using the high-level OneLake API.",
                Title = "List OneLake Items",
                Annotations = new ToolAnnotations { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "continuation-token", Description = "Continuation token for paging.", TypeName = "string" },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(OneLakeItemListCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567012",
                Name = "list_items_dfs",
                Description = "Lists OneLake items using the DFS (Data Lake File System) data API.",
                Title = "List OneLake Items (Data API)",
                Annotations = new ToolAnnotations { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "recursive", Description = "Whether to list recursively.", TypeName = "string" },
                    new OptionDescriptor { Name = "continuation-token", Description = "Continuation token for paging.", TypeName = "string" },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(OneLakeItemDataListCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567013",
                Name = "list_files",
                Description = "List files and directories in OneLake storage using filesystem-style hierarchical view.",
                Title = "List OneLake Path Structure",
                Annotations = new ToolAnnotations { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "item-id", Description = "The item ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "item", Description = "The item name.", TypeName = "string" },
                    new OptionDescriptor { Name = "path", Description = "Path within the item.", TypeName = "string" },
                    new OptionDescriptor { Name = "recursive", Description = "Whether to list recursively.", TypeName = "string" },
                    new OptionDescriptor { Name = "format", Description = "Output format.", TypeName = "string" },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(PathListCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567014",
                Name = "download_file",
                Description = "Downloads a file from OneLake storage.",
                Title = "Download OneLake File",
                Annotations = new ToolAnnotations { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "item-id", Description = "The item ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "item", Description = "The item name.", TypeName = "string" },
                    new OptionDescriptor { Name = "file-path", Description = "Path to the file in OneLake.", TypeName = "string", Required = true },
                    new OptionDescriptor { Name = "download-file-path", Description = "Local path to save the file.", TypeName = "string" },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(FileReadCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567015",
                Name = "upload_file",
                Description = "Uploads a file to OneLake storage from inline content or local file path.",
                Title = "Upload OneLake File",
                Annotations = new ToolAnnotations { Destructive = true, Idempotent = false, OpenWorld = false, ReadOnly = false, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "item-id", Description = "The item ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "item", Description = "The item name.", TypeName = "string" },
                    new OptionDescriptor { Name = "file-path", Description = "Path to the file in OneLake.", TypeName = "string", Required = true },
                    new OptionDescriptor { Name = "content", Description = "Inline content to upload.", TypeName = "string" },
                    new OptionDescriptor { Name = "local-file-path", Description = "Local file path to upload.", TypeName = "string" },
                    new OptionDescriptor { Name = "overwrite", Description = "Whether to overwrite existing file.", TypeName = "string" },
                    new OptionDescriptor { Name = "content-type", Description = "Content type of the file.", TypeName = "string" },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(FileWriteCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567016",
                Name = "delete_file",
                Description = "Deletes a file from OneLake storage.",
                Title = "Delete OneLake File",
                Annotations = new ToolAnnotations { Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "item-id", Description = "The item ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "item", Description = "The item name.", TypeName = "string" },
                    new OptionDescriptor { Name = "file-path", Description = "Path to the file in OneLake.", TypeName = "string", Required = true },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(FileDeleteCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567017",
                Name = "create_directory",
                Description = "Creates a directory in OneLake storage.",
                Title = "Create OneLake Directory",
                Annotations = new ToolAnnotations { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = false, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "item-id", Description = "The item ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "item", Description = "The item name.", TypeName = "string" },
                    new OptionDescriptor { Name = "directory-path", Description = "Path of the directory to create.", TypeName = "string", Required = true },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(DirectoryCreateCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567018",
                Name = "delete_directory",
                Description = "Deletes a directory from OneLake storage.",
                Title = "Delete OneLake Directory",
                Annotations = new ToolAnnotations { Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "item-id", Description = "The item ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "item", Description = "The item name.", TypeName = "string" },
                    new OptionDescriptor { Name = "directory-path", Description = "Path of the directory to delete.", TypeName = "string", Required = true },
                    new OptionDescriptor { Name = "recursive", Description = "Whether to delete recursively.", TypeName = "string" },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(DirectoryDeleteCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567019",
                Name = "get_table_config",
                Description = "Retrieves table API configuration for OneLake.",
                Title = "Get OneLake Table Configuration",
                Annotations = new ToolAnnotations { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "item-id", Description = "The item ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "item", Description = "The item name.", TypeName = "string" },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(TableConfigGetCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567020",
                Name = "list_table_namespaces",
                Description = "Lists table namespaces in OneLake.",
                Title = "List OneLake Table Namespaces",
                Annotations = new ToolAnnotations { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "item-id", Description = "The item ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "item", Description = "The item name.", TypeName = "string" },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(TableNamespaceListCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567021",
                Name = "get_table_namespace",
                Description = "Retrieves metadata for a specific table namespace.",
                Title = "Get OneLake Table Namespace",
                Annotations = new ToolAnnotations { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "item-id", Description = "The item ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "item", Description = "The item name.", TypeName = "string" },
                    new OptionDescriptor { Name = "namespace", Description = "The table namespace.", TypeName = "string", Required = true },
                    new OptionDescriptor { Name = "schema", Description = "The schema name (alternative to namespace).", TypeName = "string" },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(TableNamespaceGetCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567022",
                Name = "list_tables",
                Description = "Lists tables in OneLake.",
                Title = "List OneLake Tables",
                Annotations = new ToolAnnotations { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "item-id", Description = "The item ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "item", Description = "The item name.", TypeName = "string" },
                    new OptionDescriptor { Name = "namespace", Description = "The table namespace.", TypeName = "string", Required = true },
                    new OptionDescriptor { Name = "schema", Description = "The schema name (alternative to namespace).", TypeName = "string" },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(TableListCommand)
            },
            new CommandDescriptor
            {
                Id = "f1a2b3c4-d5e6-7890-abcd-ef1234567023",
                Name = "get_table",
                Description = "Retrieves table definition from OneLake.",
                Title = "Get OneLake Table",
                Annotations = new ToolAnnotations { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false },
                Options =
                [
                    new OptionDescriptor { Name = "workspace-id", Description = "The workspace ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "workspace", Description = "The workspace name.", TypeName = "string" },
                    new OptionDescriptor { Name = "item-id", Description = "The item ID.", TypeName = "string" },
                    new OptionDescriptor { Name = "item", Description = "The item name.", TypeName = "string" },
                    new OptionDescriptor { Name = "namespace", Description = "The table namespace.", TypeName = "string", Required = true },
                    new OptionDescriptor { Name = "schema", Description = "The schema name (alternative to namespace).", TypeName = "string" },
                    new OptionDescriptor { Name = "table", Description = "The table name.", TypeName = "string", Required = true },
                ],
                InheritOptions = InheritOptions.Global,
                HandlerType = nameof(TableGetCommand)
            },
        ],
    };

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IOneLakeService, OneLakeService>();
        services.AddHttpClient<OneLakeService>();
        AddCommand<OneLakeWorkspaceListCommand>(services);
        AddCommand<OneLakeItemListCommand>(services);
        AddCommand<OneLakeItemDataListCommand>(services);
        AddCommand<FileReadCommand>(services);
        AddCommand<FileWriteCommand>(services);
        AddCommand<FileDeleteCommand>(services);
        AddCommand<PathListCommand>(services);
        AddCommand<BlobPutCommand>(services);
        AddCommand<BlobGetCommand>(services);
        AddCommand<BlobDeleteCommand>(services);
        AddCommand<BlobListCommand>(services);
        AddCommand<DirectoryCreateCommand>(services);
        AddCommand<DirectoryDeleteCommand>(services);
        AddCommand<TableConfigGetCommand>(services);
        AddCommand<TableListCommand>(services);
        AddCommand<TableGetCommand>(services);
        AddCommand<TableNamespaceListCommand>(services);
        AddCommand<TableNamespaceGetCommand>(services);
    }
}

