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
/// Command to list OneLake items in a workspace using the OneLake DFS (Data Lake File System) API.
/// </summary>
public sealed class OneLakeItemListDfsCommand(
    ILogger<OneLakeItemListDfsCommand> logger,
    IOneLakeService oneLakeService) : BaseWorkspaceCommand<OneLakeItemListDfsOptions>()
{
    private readonly ILogger<OneLakeItemListDfsCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "7e7566ab-0984-4f1e-a8be-45a0184a59e5";
    public override string Name => "onelake-item-list-dfs";
    public override string Title => "List OneLake Items (DFS)";
    public override string Description => "List OneLake items in a workspace using the OneLake DFS (Data Lake File System) API";

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
        command.Options.Add(FabricOptionDefinitions.Recursive);
        command.Options.Add(FabricOptionDefinitions.ContinuationToken);
    }

    protected override OneLakeItemListDfsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Recursive = parseResult.GetValueOrDefault<bool>(FabricOptionDefinitions.Recursive.Name);
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
            var jsonResponse = await _oneLakeService.ListOneLakeItemsDfsJsonAsync(
                options.WorkspaceId!,
                recursive: options.Recursive,
                continuationToken: options.ContinuationToken,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(jsonResponse), OneLakeJsonContext.Default.OneLakeItemListDfsCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing OneLake items (DFS) in workspace {WorkspaceId}. Options: {@Options}", options.WorkspaceId, options);
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

    public sealed record OneLakeItemListDfsCommandResult(string? JsonResponse);
}

public sealed class OneLakeItemListDfsOptions : BaseWorkspaceOptions
{
    public bool Recursive { get; set; }
    public string? ContinuationToken { get; set; }
}
