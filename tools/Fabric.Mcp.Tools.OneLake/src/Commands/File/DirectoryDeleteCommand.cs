// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

public sealed class DirectoryDeleteCommand(
    ILogger<DirectoryDeleteCommand> logger,
    IOneLakeService oneLakeService) : BaseItemCommand<DirectoryDeleteOptions>()
{
    private readonly ILogger<DirectoryDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "86991cd6-75fa-4870-9d99-f986ba9f5f73";
    public override string Name => "delete-directory";
    public override string Title => "Delete OneLake Directory";
    public override string Description => "Deletes a directory from OneLake storage. Use this when the user wants to remove a folder. Use recursive flag to delete non-empty directories.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        LocalRequired = false,
        OpenWorld = false,
        ReadOnly = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.DirectoryPath);
        command.Options.Add(FabricOptionDefinitions.Recursive);
    }

    protected override DirectoryDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DirectoryPath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.DirectoryPath.Name) ?? string.Empty;
        options.Recursive = parseResult.GetValueOrDefault<bool>(FabricOptionDefinitions.Recursive.Name);
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
            await _oneLakeService.DeleteDirectoryAsync(
                options.WorkspaceId!,
                options.ItemId!,
                options.DirectoryPath,
                options.Recursive,
                cancellationToken);

            var message = options.Recursive
                ? "Directory and all contents deleted successfully"
                : "Directory deleted successfully";
            var result = new DirectoryDeleteCommandResult(options.DirectoryPath, message);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DirectoryDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting directory {DirectoryPath} from workspace {WorkspaceId}, item {ItemId}. Options: {@Options}",
                options.DirectoryPath, options.WorkspaceId, options.ItemId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DirectoryDeleteCommandResult(
        string DirectoryPath,
        string Message);
}

public sealed class DirectoryDeleteOptions : BaseItemOptions
{
    public string DirectoryPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = false;
}
