// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.OneLake.Commands.Table;

public sealed class TableNamespaceGetCommand(
    ILogger<TableNamespaceGetCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<TableNamespaceGetOptions>()
{
    private readonly ILogger<TableNamespaceGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "a86298d1-7475-4ea8-8c1b-e4c54ac2b896";
    public override string Name => "get_table_namespace";
    public override string Title => "Get OneLake Table Namespace";
    public override string Description => "Retrieves metadata for a specific table namespace. Use this when the user needs details about a namespace.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        LocalRequired = false,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Item.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Namespace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Schema.AsOptional());
    }

    protected override TableNamespaceGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.Workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.Item = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Item.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Namespace.Name);
        options.Namespace ??= parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Schema.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace;

            if (string.IsNullOrWhiteSpace(workspaceIdentifier))
            {
                context.Response.Status = System.Net.HttpStatusCode.BadRequest;
                context.Response.Message = "Workspace identifier is required. Provide --workspace or --workspace-id.";
                return context.Response;
            }

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item;

            if (string.IsNullOrWhiteSpace(itemIdentifier))
            {
                context.Response.Status = System.Net.HttpStatusCode.BadRequest;
                context.Response.Message = "Item identifier is required. Provide --item or --item-id.";
                return context.Response;
            }

            if (string.IsNullOrWhiteSpace(options.Namespace))
            {
                context.Response.Status = System.Net.HttpStatusCode.BadRequest;
                context.Response.Message = "Namespace is required. Provide --namespace.";
                return context.Response;
            }

            var namespaceResult = await _oneLakeService.GetTableNamespaceAsync(workspaceIdentifier!, itemIdentifier!, options.Namespace!, cancellationToken);
            var result = new TableNamespaceGetCommandResult(namespaceResult.Workspace, namespaceResult.Item, namespaceResult.Namespace, namespaceResult.Definition, namespaceResult.RawResponse);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableNamespaceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving table namespace. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class TableNamespaceGetCommandResult
    {
        public string Workspace { get; init; } = string.Empty;
        public string Item { get; init; } = string.Empty;
        public string Namespace { get; init; } = string.Empty;
        public JsonElement Definition { get; init; } = default;
        public string RawResponse { get; init; } = string.Empty;

        public TableNamespaceGetCommandResult()
        {
        }

        public TableNamespaceGetCommandResult(string workspace, string item, string namespaceName, JsonElement definition, string rawResponse)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Namespace = namespaceName ?? throw new ArgumentNullException(nameof(namespaceName));
            Definition = definition;
            RawResponse = rawResponse ?? string.Empty;
        }
    }
}
