// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core;

public class FabricCoreSetup : IAreaSetup
{
    public string Name => "core";
    public string Title => "Microsoft Fabric Core";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFabricItemDefinitionFileService, FabricItemDefinitionFileService>();
        services.AddSingleton<IFabricItemDefinitionInputService, FabricItemDefinitionInputService>();
        services.AddSingleton<IFabricItemDefinitionExportService, FabricItemDefinitionExportService>();
        services.AddHttpClient<IFabricCoreService, FabricCoreService>()
            .ConfigurePrimaryHttpMessageHandler(static (handler, _) =>
            {
                // Retain configured wrappers and proxy settings while disabling transport-level redirects.
                while (handler is DelegatingHandler { InnerHandler: { } innerHandler })
                {
                    handler = innerHandler;
                }

                if (handler is HttpClientHandler httpHandler)
                {
                    httpHandler.AllowAutoRedirect = false;
                }
                else if (handler is SocketsHttpHandler socketsHandler)
                {
                    socketsHandler.AllowAutoRedirect = false;
                }
            });
        services.AddSingleton<CapacityGetCommand>();
        services.AddSingleton<CapacityListCommand>();
        services.AddSingleton<ItemCreateCommand>(CreateItemCreateCommand);
        services.AddSingleton<ItemDeleteCommand>(CreateItemDeleteCommand);
        services.AddSingleton<ItemDefinitionGetCommand>(CreateItemDefinitionGetCommand);
        services.AddSingleton<ItemGetCommand>();
        services.AddSingleton<ItemListCommand>();
        services.AddSingleton<ItemUpdateCommand>();
        services.AddSingleton<OperationStateGetCommand>();
        services.AddSingleton<OperationResultGetCommand>();
        services.AddSingleton<CatalogSearchCommand>();
        services.AddSingleton<WorkspaceAssignToCapacityCommand>();
        services.AddSingleton<WorkspaceCreateCommand>();
        services.AddSingleton<WorkspaceDeleteCommand>();
        services.AddSingleton<WorkspaceGetCommand>();
        services.AddSingleton<WorkspaceListCommand>();
        services.AddSingleton<WorkspaceUpdateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var fabricCore = new CommandGroup(Name,
            "Microsoft Fabric Core Operations - Inspect capacities, discover, create, and update workspaces, and search, create, and manage Fabric items.\n" +
            "Use this tool when you need to:\n" +
            "- Get metadata for a known Fabric capacity\n" +
            "- List accessible Fabric capacities and their metadata\n" +
            "- Search the OneLake catalog to discover Fabric items across workspaces\n" +
            "- Get metadata for an existing Fabric workspace by ID\n" +
            "- Get metadata for one existing Fabric item by workspace ID and item ID\n" +
            "- Retrieve an item's definition inline or explicitly export completed parts to a new local directory\n" +
            "- List accessible workspaces and their management metadata, optionally filtered by the caller's workspace roles\n" +
            "- List item metadata within a known workspace or folder, optionally filtered by type\n" +
            "- Create new Fabric items, optionally from inline definitions or explicitly selected local definition files\n" +
            "- Track accepted long-running operations and read their JSON or empty results\n" +
            "- Create Fabric workspaces, optionally assigning an existing capacity and domain\n" +
            "- Rename a known workspace or update or clear its description\n" +
            "- Delete a known Fabric item, with permanent deletion only by explicit opt-in\n" +
            "- Delete an explicitly identified workspace and the items under it\n" +
            "- Manage core Fabric workspace items\n" +
            "- Submit workspace capacity assignments without waiting for completion\n" +
            "This tool provides core operations for working with Fabric resources.");

        fabricCore.AddCommand<CapacityGetCommand>(serviceProvider);
        fabricCore.AddCommand<CapacityListCommand>(serviceProvider);
        fabricCore.AddCommand<ItemCreateCommand>(serviceProvider);
        fabricCore.AddCommand<ItemDeleteCommand>(serviceProvider);
        fabricCore.AddCommand<ItemDefinitionGetCommand>(serviceProvider);
        fabricCore.AddCommand<ItemGetCommand>(serviceProvider);
        fabricCore.AddCommand<ItemListCommand>(serviceProvider);
        fabricCore.AddCommand<ItemUpdateCommand>(serviceProvider);
        fabricCore.AddCommand<OperationStateGetCommand>(serviceProvider);
        fabricCore.AddCommand<OperationResultGetCommand>(serviceProvider);
        fabricCore.AddCommand<CatalogSearchCommand>(serviceProvider);
        fabricCore.AddCommand<WorkspaceAssignToCapacityCommand>(serviceProvider);
        fabricCore.AddCommand<WorkspaceCreateCommand>(serviceProvider);
        fabricCore.AddCommand<WorkspaceDeleteCommand>(serviceProvider);
        fabricCore.AddCommand<WorkspaceGetCommand>(serviceProvider);
        fabricCore.AddCommand<WorkspaceListCommand>(serviceProvider);
        fabricCore.AddCommand<WorkspaceUpdateCommand>(serviceProvider);

        return fabricCore;
    }

    private static ItemCreateCommand CreateItemCreateCommand(IServiceProvider serviceProvider)
    {
        var definitionInput = serviceProvider.GetRequiredService<IFabricItemDefinitionInputService>();
        var command = new ItemCreateCommand(
            serviceProvider.GetRequiredService<ILogger<ItemCreateCommand>>(),
            serviceProvider.GetRequiredService<IFabricCoreService>(),
            definitionInput);
        var definition = command.GetCommand();
        var inputOptions = definition.Options
            .Where(static option => option.Name is "--definition" or "--definition-directory" or "--definition-files" or "--definition-format" or
                "--folder-id" or "--creation-payload" or "--options" or "--sensitivity-label-settings")
            .ToArray();
        if (!definitionInput.SupportsLocalFiles)
        {
            // Remove options from this command's collection, not the cached OptionBinder
            // options shared by local and HTTP hosts in the same process.
            foreach (var option in inputOptions.Where(static option =>
                option.Name is "--definition-directory" or "--definition-files" or "--definition-format"))
            {
                definition.Options.Remove(option);
            }
        }
        definition.Validators.Add(result =>
        {
            if (inputOptions.Any(option => result.GetResult(option) is { IdentifierTokenCount: > 1 }))
            {
                result.AddError("Specify each item-creation input option at most once.");
            }
        });
        return command;
    }

    private static ItemDefinitionGetCommand CreateItemDefinitionGetCommand(IServiceProvider serviceProvider)
    {
        var export = serviceProvider.GetRequiredService<IFabricItemDefinitionExportService>();
        var command = new ItemDefinitionGetCommand(
            serviceProvider.GetRequiredService<ILogger<ItemDefinitionGetCommand>>(),
            serviceProvider.GetRequiredService<IFabricCoreService>(),
            export);
        var definition = command.GetCommand();
        var options = definition.Options.ToArray();
        if (!export.SupportsLocalFiles)
        {
            // Do not change cached OptionBinder options shared by other hosts.
            definition.Options.Remove(options.Single(static option => option.Name == "--output-directory"));
        }
        definition.Validators.Add(result =>
        {
            if (options.Any(option => result.GetResult(option) is { IdentifierTokenCount: > 1 }))
            {
                result.AddError("Specify each get-item-definition option at most once.");
            }
        });
        return command;
    }

    private static ItemDeleteCommand CreateItemDeleteCommand(IServiceProvider serviceProvider)
    {
        var command = new ItemDeleteCommand(
            serviceProvider.GetRequiredService<ILogger<ItemDeleteCommand>>(),
            serviceProvider.GetRequiredService<IFabricCoreService>());
        var definition = command.GetCommand();
        if (definition.Options.SingleOrDefault(option => option.Name == "--hard-delete") is not Option<bool?> hardDelete ||
            hardDelete.Required || hardDelete.HasDefaultValue)
        {
            throw new InvalidOperationException("The delete-item command requires an optional Boolean --hard-delete option without a default.");
        }

        hardDelete.Arity = ArgumentArity.ExactlyOne;
        // Keep validation on this command, not on the option cached by OptionBinder.
        definition.Validators.Add(result =>
        {
            var option = result.GetResult(hardDelete);
            if (option is null || option.Implicit)
            {
                return;
            }

            if (option.IdentifierTokenCount != 1 || option.Tokens.Count != 1 ||
                !bool.TryParse(option.Tokens[0].Value, out _))
            {
                result.AddError("Specify --hard-delete at most once with an explicit true or false value.");
            }
        });

        return command;
    }
}
