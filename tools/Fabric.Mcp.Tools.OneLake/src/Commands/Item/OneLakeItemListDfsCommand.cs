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
    Id = "7e7566ab-0984-4f1e-a8be-45a0184a59e5",
    Name = "onelake-item-list-dfs",
    Title = "List OneLake Items (DFS)",
    Description = "List OneLake items in a workspace using the OneLake DFS (Data Lake File System) API",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class OneLakeItemListDfsCommand(
    ILogger<OneLakeItemListDfsCommand> logger,
    IOneLakeService oneLakeService) : AuthenticatedCommand<OneLakeItemListDfsOptions, OneLakeItemListDfsCommand.OneLakeItemListDfsCommandResult>
{
    private readonly ILogger<OneLakeItemListDfsCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(OneLakeItemListDfsOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (string.IsNullOrWhiteSpace(options.WorkspaceId) && string.IsNullOrWhiteSpace(options.Workspace))
        {
            validationResult.Errors.Add("Workspace identifier is required. Provide --workspace or --workspace-id.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, OneLakeItemListDfsOptions options, CancellationToken cancellationToken)
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

            var result = new OneLakeItemListDfsCommandResult { JsonResponse = jsonResponse };
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeItemListDfsCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing OneLake items (DFS) in workspace {WorkspaceId}.", options.WorkspaceId);
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

    public sealed record OneLakeItemListDfsCommandResult
    {
        public string? JsonResponse { get; init; }
    }
}

public sealed class OneLakeItemListDfsOptions
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
