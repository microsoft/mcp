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
/// Command to list OneLake items in a workspace using the OneLake DFS (Data Lake File System) API.
/// </summary>
[CommandMetadata(
    Id = "8925d0c4-becf-4b5a-8af1-3e998c1058ec",
    Name = "list_items_dfs",
    Title = "List OneLake Items (Data API)",
    Description = "List OneLake items in a workspace using the OneLake DFS (Data Lake File System) data API.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class OneLakeItemDataListCommand(
    ILogger<OneLakeItemDataListCommand> logger,
    IOneLakeService oneLakeService) : AuthenticatedCommand<OneLakeItemDataListOptions, OneLakeItemDataListCommand.OneLakeItemDataListCommandResult>
{
    private readonly ILogger<OneLakeItemDataListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(OneLakeItemDataListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (string.IsNullOrWhiteSpace(options.WorkspaceId) && string.IsNullOrWhiteSpace(options.Workspace))
        {
            validationResult.Errors.Add("Workspace identifier is required. Provide --workspace or --workspace-id.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, OneLakeItemDataListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace!;

            var jsonResponse = await _oneLakeService.ListOneLakeItemsDfsJsonAsync(
                workspaceIdentifier,
                recursive: options.Recursive,
                continuationToken: options.ContinuationToken,
                cancellationToken);

            var result = new OneLakeItemDataListCommandResult { JsonResponse = jsonResponse };
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeItemDataListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing OneLake items (data API) in workspace {WorkspaceId}.", options.WorkspaceId);
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

public sealed class OneLakeItemDataListOptions
{
    [Option(Description = "The ID of the Microsoft Fabric workspace.")]
    public string? WorkspaceId { get; set; }

    [Option(Description = "The name or ID of the Microsoft Fabric workspace.")]
    public string? Workspace { get; set; }

    [Option(Description = "Whether to perform the operation recursively.")]
    public bool Recursive { get; set; }

    [Option(Description = "Token for retrieving the next page of results.")]
    public string? ContinuationToken { get; set; }
}
