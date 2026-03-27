// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Workbooks;

public sealed class WorkbooksRegistration : IAreaRegistration
{
    public static string AreaName => "workbooks";

    public static string AreaTitle => "Azure Workbooks";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Workbooks operations - Commands for managing Azure Workbooks resources and interactive data visualization dashboards. Includes operations for listing, creating, updating, and deleting workbooks, as well as managing workbook configurations and content.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "a49c650d-8568-4b63-8bad-35eb6d9ab0a7",
                Name = "create",
                Description = "Create a new workbook in the specified resource group and subscription. You can set the display name and serialized data JSON content for the workbook. Returns the created workbook information upon successful completion.",
                Title = "Create",
                Annotations = new ToolAnnotations
                {
                    Destructive = true,
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
                        Name = "resource-group",
                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "display-name",
                        Description = "The display name of the workbook.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "serialized-content",
                        Description = "The serialized JSON content of the workbook.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "source-id",
                        Description = "The linked resource ID for the workbook. By default, this is 'azure monitor'.",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(CreateWorkbooksCommand)
            },
            new CommandDescriptor
            {
                Id = "17bb94ef-9df1-45d2-a1a0-ed57656ca067",
                Name = "delete",
                Description = "Delete one or more workbooks by their Azure resource IDs. This command soft deletes workbooks: they will be retained for 90 days. If needed, you can restore them from the Recycle Bin through the Azure Portal. BATCH: Accepts multiple --workbook-ids values. Partial failures are reported per-workbook. Individual failures do not fail the entire batch operation. To learn more, visit: https://learn.microsoft.com/azure/azure-monitor/visualize/workbooks-manage",
                Title = "Delete",
                Annotations = new ToolAnnotations
                {
                    Destructive = true,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = false,
                    LocalRequired = false,
                    Secret = false,
                },
                Options =
                [
                    new OptionDescriptor
                    {
                        Name = "workbook-ids",
                        Description = "The Azure Resource IDs of the workbooks to operate on (supports multiple values for batch operations).",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Global,
                HandlerType = nameof(DeleteWorkbooksCommand)
            },
            new CommandDescriptor
            {
                Id = "c4c90435-fbc0-4598-ba82-3b9213d58b26",
                Name = "list",
                Description = "Search Azure Workbooks using Resource Graph (fast metadata query). USE FOR: Discovery, filtering, counting workbooks across scopes. RETURNS: Workbook metadata (id, name, location, category, timestamps). DOES NOT RETURN: Full workbook content (serializedData) by default - use 'show' for that or set --output-format=full. SCOPE: By default searches workbooks in your current Azure context (tenant/subscription). Use --subscription and --resource-group to explicitly control scope. TOTAL COUNT: Returns server-side total count by default (not just returned items). MAX RESULTS: Default 50, max 1000. Use --max-results to adjust. OUTPUT FORMAT: Use --output-format=summary for minimal tokens, --output-format=full for serializedData. FILTERS: --name-contains, --category, --kind, --source-id, --modified-after for semantic filtering.",
                Title = "List",
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
                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "kind",
                        Description = "Filter workbooks by kind (e.g., 'shared', 'user'). If not specified, all kinds will be returned.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "category",
                        Description = "Filter workbooks by category (e.g., 'workbook', 'sentinel', 'TSG'). If not specified, all categories will be returned.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "source-id",
                        Description = "Filter workbooks by source resource ID (e.g., Application Insights resource, Log Analytics workspace). If not specified, all workbooks will be returned.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "name-contains",
                        Description = "Filter workbooks where display name contains this text (case-insensitive).",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "modified-after",
                        Description = "Filter workbooks modified after this date (ISO 8601 format, e.g., '2024-01-15').",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "output-format",
                        Description = "Output format: 'summary' (id+name only, minimal tokens), 'standard' (metadata without content, default), 'full' (includes serializedData).",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "max-results",
                        Description = "Maximum number of results to return (default: 50, max: 1000).",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "include-total-count",
                        Description = "Include total count of all matching workbooks in the response (default: true).",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(ListWorkbooksCommand)
            },
            new CommandDescriptor
            {
                Id = "a7a882cd-1729-49ed-b349-2a79f8c7de56",
                Name = "show",
                Description = "Retrieve full workbook details via ARM API (includes serializedData content). USE FOR: Getting complete workbook definition including visualization JSON. RETURNS: Full workbook properties, serializedData, tags, etag. BATCH: Accepts multiple --workbook-ids values. Partial failures reported per-workbook. PERFORMANCE: Use 'list' first for discovery, then 'show' for specific workbooks.",
                Title = "Show",
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
                        Name = "workbook-ids",
                        Description = "The Azure Resource IDs of the workbooks to operate on (supports multiple values for batch operations).",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Global,
                HandlerType = nameof(ShowWorkbooksCommand)
            },
            new CommandDescriptor
            {
                Id = "9efdc32c-22bc-4b85-8b5c-2fbefc0e927e",
                Name = "update",
                Description = "Updates properties of an existing Azure Workbook by adding new steps, modifying content, or changing the display name. Returns the updated workbook details. Requires the workbook resource ID and either new serialized content or a new display name.",
                Title = "Update",
                Annotations = new ToolAnnotations
                {
                    Destructive = true,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = false,
                    LocalRequired = false,
                    Secret = false,
                },
                Options =
                [
                    new OptionDescriptor
                    {
                        Name = "workbook-id",
                        Description = "The Azure Resource ID of the workbook to retrieve.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "display-name",
                        Description = "The display name of the workbook.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "serialized-content",
                        Description = "The JSON serialized content/data of the workbook.",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Global,
                HandlerType = nameof(UpdateWorkbooksCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IWorkbooksService, WorkbooksService>();
        services.AddSingleton<ListWorkbooksCommand>();
        services.AddSingleton<ShowWorkbooksCommand>();
        services.AddSingleton<UpdateWorkbooksCommand>();
        services.AddSingleton<CreateWorkbooksCommand>();
        services.AddSingleton<DeleteWorkbooksCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(CreateWorkbooksCommand) => serviceProvider.GetRequiredService<CreateWorkbooksCommand>(),
            nameof(DeleteWorkbooksCommand) => serviceProvider.GetRequiredService<DeleteWorkbooksCommand>(),
            nameof(ListWorkbooksCommand) => serviceProvider.GetRequiredService<ListWorkbooksCommand>(),
            nameof(ShowWorkbooksCommand) => serviceProvider.GetRequiredService<ShowWorkbooksCommand>(),
            nameof(UpdateWorkbooksCommand) => serviceProvider.GetRequiredService<UpdateWorkbooksCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in workbooks area.")
        };
}
