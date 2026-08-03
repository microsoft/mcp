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

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

/// <summary>
/// Command to create a directory in OneLake storage.
/// </summary>
[CommandMetadata(
    Id = "0c4cf0f4-2ef4-4f1d-9f80-24fd7636d5fe",
    Name = "create_directory",
    Title = "Create OneLake Directory",
    Description = "Creates a directory in OneLake storage. Use this when the user needs to organize files or prepare folder structures. Can create nested directory paths.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class DirectoryCreateCommand(ILogger<DirectoryCreateCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<DirectoryCreateOptions, DirectoryCreateCommand.DirectoryCreateCommandResult>
{
    private readonly ILogger<DirectoryCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(DirectoryCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (string.IsNullOrWhiteSpace(options.WorkspaceId) && string.IsNullOrWhiteSpace(options.Workspace))
        {
            validationResult.Errors.Add("Workspace identifier is required. Provide --workspace or --workspace-id.");
        }

        if (string.IsNullOrWhiteSpace(options.ItemId) && string.IsNullOrWhiteSpace(options.Item))
        {
            validationResult.Errors.Add("Item identifier is required. Provide --item or --item-id.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DirectoryCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace!;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item!;

            await _oneLakeService.CreateDirectoryAsync(
                workspaceIdentifier,
                itemIdentifier,
                options.DirectoryPath,
                cancellationToken);

            var result = new DirectoryCreateCommandResult(
                WorkspaceId: options.WorkspaceId ?? options.Workspace ?? string.Empty,
                ItemId: options.ItemId ?? options.Item ?? string.Empty,
                DirectoryPath: options.DirectoryPath,
                Success: true,
                Message: $"Directory '{options.DirectoryPath}' created successfully");

            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DirectoryCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory {DirectoryPath} in item {ItemId}.",
                options.DirectoryPath, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) =>
        OneLakeCommandValidators.GetErrorMessage(ex, base.GetErrorMessage);

    protected override HttpStatusCode GetStatusCode(Exception ex) =>
        OneLakeCommandValidators.GetStatusCode(ex, base.GetStatusCode);

    public sealed record DirectoryCreateCommandResult(string WorkspaceId, string ItemId, string DirectoryPath, bool Success, string Message);
}

public sealed class DirectoryCreateOptions
{
    [Option(Description = OneLakeOptionDescriptions.WorkspaceId)]
    public string? WorkspaceId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Workspace)]
    public string? Workspace { get; set; }

    [Option(Description = OneLakeOptionDescriptions.ItemId)]
    public string? ItemId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Item)]
    public string? Item { get; set; }

    [Option(Description = OneLakeOptionDescriptions.DirectoryPath)]
    public required string DirectoryPath { get; set; }
}
