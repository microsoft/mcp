// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

[HiddenCommand]
public sealed class BlobDeleteCommand(
    ILogger<BlobDeleteCommand> logger,
    IOneLakeService oneLakeService) : BaseItemCommand<BlobDeleteOptions>()
{
    private readonly ILogger<BlobDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "48561b8d-6f19-45ae-86fa-9feeb8f75e8e";
    public override string Name => "delete";
    public override string Title => "Delete OneLake Blob";
    public override string Description => "Delete a blob from OneLake using the blob endpoint while returning request metadata for auditing.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        LocalRequired = false,
        OpenWorld = false,
        ReadOnly = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.FilePath);
    }

    protected override BlobDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FilePath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.FilePath.Name) ?? string.Empty;
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
            var result = await _oneLakeService.DeleteBlobAsync(
                options.WorkspaceId!,
                options.ItemId!,
                options.FilePath,
                cancellationToken);

            var commandResult = new BlobDeleteCommandResult(
                result,
                "Blob deleted successfully.");

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(commandResult, OneLakeJsonContext.Default.BlobDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blob {BlobPath} in workspace {WorkspaceId}, item {ItemId}. Options: {@Options}",
                options.FilePath, options.WorkspaceId, options.ItemId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record BlobDeleteCommandResult(BlobDeleteResult Result, string Message);

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };
}

public sealed class BlobDeleteOptions : BaseItemOptions
{
    public string FilePath { get; set; } = string.Empty;
}
