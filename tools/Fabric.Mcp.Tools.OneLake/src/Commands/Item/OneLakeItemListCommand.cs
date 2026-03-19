// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Fabric.Mcp.Tools.OneLake.Commands.Item;

/// <summary>
/// Command to list OneLake items in a workspace using the OneLake data plane API.
/// </summary>
public sealed class OneLakeItemListCommand(
    ILogger<OneLakeItemListCommand> logger,
    IOneLakeService oneLakeService) : BaseWorkspaceCommand<OneLakeItemListOptions>()
{
    private readonly ILogger<OneLakeItemListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "61eb86d8-3879-4d2d-969a-6c96f2e0ce0d";
    public override string Name => "list-items";
    public override string Title => "List OneLake Items";
    public override string Description => "Lists OneLake items in a Fabric workspace using the high-level OneLake API. Use this when the user needs to see what items exist in a workspace. Returns item names, types, and metadata.";

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
        command.Options.Add(FabricOptionDefinitions.ContinuationToken);
    }

    protected override OneLakeItemListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ContinuationToken = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ContinuationToken.Name);
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
            var xmlResponse = await _oneLakeService.ListOneLakeItemsXmlAsync(
                options.WorkspaceId!,
                continuationToken: options.ContinuationToken,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(xmlResponse), OneLakeJsonContext.Default.OneLakeItemListCommandResult);
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

    public sealed record OneLakeItemListCommandResult(string? XmlResponse);
}

public sealed class OneLakeItemListOptions : BaseWorkspaceOptions
{
    public string? ContinuationToken { get; set; }
}
