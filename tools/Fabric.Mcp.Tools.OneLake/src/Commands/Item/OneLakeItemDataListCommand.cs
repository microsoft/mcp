// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;

namespace Fabric.Mcp.Tools.OneLake.Commands.Item;

/// <summary>
/// Command to list OneLake items in a workspace using the OneLake DFS (Data Lake File System) API.
/// </summary>
public sealed class OneLakeItemDataListCommand(
    ILogger<OneLakeItemDataListCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<OneLakeItemDataListOptions>()
{
    private readonly ILogger<OneLakeItemDataListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "onelake-item-data-list";
    public override string Title => "List OneLake Items (Data API)";
    public override string Description => "List OneLake items in a workspace using the OneLake DFS (Data Lake File System) data API.";

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
        command.Options.Add(FabricOptionDefinitions.Recursive);
        command.Options.Add(FabricOptionDefinitions.ContinuationToken);
    }

    protected override OneLakeItemDataListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        var workspaceName = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId!
            : workspaceName ?? string.Empty;
        options.Recursive = parseResult.GetValueOrDefault<bool>(FabricOptionDefinitions.Recursive.Name);
        options.ContinuationToken = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ContinuationToken.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            if (string.IsNullOrWhiteSpace(options.WorkspaceId))
            {
                throw new ArgumentException("Workspace identifier is required. Provide --workspace or --workspace-id.", nameof(options.WorkspaceId));
            }

            var jsonResponse = await _oneLakeService.ListOneLakeItemsDfsJsonAsync(
                options.WorkspaceId,
                recursive: options.Recursive,
                continuationToken: options.ContinuationToken,
                CancellationToken.None);

            var result = new OneLakeItemDataListCommandResult { JsonResponse = jsonResponse };
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeItemDataListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing OneLake items (data API) in workspace {WorkspaceId}. Options: {@Options}", options.WorkspaceId, options);
            HandleException(context, ex);
        }
        
        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => $"Invalid argument: {argEx.Message}",
        InvalidOperationException opEx => $"Operation failed: {opEx.Message}",
        HttpRequestException httpEx => $"HTTP request failed: {httpEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        InvalidOperationException => HttpStatusCode.InternalServerError,
        HttpRequestException httpEx when httpEx.Message.Contains("404") => HttpStatusCode.NotFound,
        HttpRequestException httpEx when httpEx.Message.Contains("403") => HttpStatusCode.Forbidden,
        HttpRequestException httpEx when httpEx.Message.Contains("401") => HttpStatusCode.Unauthorized,
        _ => base.GetStatusCode(ex)
    };

    public sealed record OneLakeItemDataListCommandResult
    {
        public string? JsonResponse { get; init; }
    }
}

public sealed class OneLakeItemDataListOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public bool Recursive { get; set; } = true;
    public string? ContinuationToken { get; set; }
}
