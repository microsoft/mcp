// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Commands.Item;

/// <summary>
/// Command to list OneLake items in a workspace using the OneLake data plane API.
/// </summary>
[CommandMetadata(
    Id = "61eb86d8-3879-4d2d-969a-6c96f2e0ce0d",
    Name = "list_items",
    Title = "List OneLake Items",
    Description = "Lists OneLake items in a Fabric workspace using the high-level OneLake API. Use this when the user needs to see what items exist in a workspace. Returns item names, types, and metadata.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class OneLakeItemListCommand(
    ILogger<OneLakeItemListCommand> logger,
    IOneLakeService oneLakeService) : AuthenticatedCommand<OneLakeItemListOptions, OneLakeItemListCommand.OneLakeItemListCommandResult>
{
    private readonly ILogger<OneLakeItemListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(OneLakeItemListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (string.IsNullOrWhiteSpace(options.WorkspaceId) && string.IsNullOrWhiteSpace(options.Workspace))
        {
            validationResult.Errors.Add("Workspace identifier is required. Provide --workspace or --workspace-id.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, OneLakeItemListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace!;

            var xmlResponse = await _oneLakeService.ListOneLakeItemsXmlAsync(
                workspaceIdentifier,
                continuationToken: options.ContinuationToken,
                cancellationToken);

            var result = new OneLakeItemListCommandResult { XmlResponse = xmlResponse };
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeItemListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing OneLake items in workspace {WorkspaceId}.", options.WorkspaceId);
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

    public sealed record OneLakeItemListCommandResult
    {
        public List<OneLakeItem>? Items { get; init; }
        public string? XmlResponse { get; init; }

        public OneLakeItemListCommandResult() { }
        public OneLakeItemListCommandResult(List<OneLakeItem> items) { Items = items; }
    }
}

public sealed class OneLakeItemListOptions
{
    [Option(Description = "The ID of the Microsoft Fabric workspace.")]
    public string? WorkspaceId { get; set; }

    [Option(Description = "The name or ID of the Microsoft Fabric workspace.")]
    public string? Workspace { get; set; }

    [Option(Description = "Token for retrieving the next page of results.")]
    public string? ContinuationToken { get; set; }
}
