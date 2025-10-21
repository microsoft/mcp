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
/// Command to list OneLake items in a workspace using the OneLake data plane API.
/// </summary>
public sealed class OneLakeItemListCommand(
    ILogger<OneLakeItemListCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<OneLakeItemListOptions>()
{
    private readonly ILogger<OneLakeItemListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "onelake-item-list";
    public override string Title => "List OneLake Items";
    public override string Description => "List OneLake items in a workspace using the OneLake data plane API";

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
        command.Options.Add(FabricOptionDefinitions.WorkspaceId);
        command.Options.Add(FabricOptionDefinitions.ContinuationToken);
    }

    protected override OneLakeItemListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name) ?? string.Empty;
        options.ContinuationToken = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ContinuationToken.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            var items = await _oneLakeService.ListOneLakeItemsAsync(
                options.WorkspaceId,
                options.ContinuationToken,
                CancellationToken.None);

            var result = new OneLakeItemListCommandResult(items.ToList());
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeItemListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing OneLake items in workspace {WorkspaceId}. Options: {@Options}", options.WorkspaceId, options);
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

    public sealed record OneLakeItemListCommandResult(List<OneLakeItem> Items);
}

public sealed class OneLakeItemListOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string? ContinuationToken { get; set; }
}