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

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

/// <summary>
/// Command to create a directory in OneLake storage.
/// </summary>
public sealed class DirectoryCreateCommand(
    ILogger<DirectoryCreateCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<DirectoryCreateOptions>()
{
    private readonly ILogger<DirectoryCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "directory-create";
    public override string Title => "Create OneLake Directory";
    public override string Description => "Create a directory in OneLake storage. Can create nested directory structures.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        LocalRequired = false,
        OpenWorld = false,
        ReadOnly = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId);
        command.Options.Add(FabricOptionDefinitions.ItemId);
        command.Options.Add(FabricOptionDefinitions.DirectoryPath);
    }

    protected override DirectoryCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name) ?? string.Empty;
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name) ?? string.Empty;
        options.DirectoryPath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.DirectoryPath.Name) ?? string.Empty;
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            await _oneLakeService.CreateDirectoryAsync(
                options.WorkspaceId,
                options.ItemId,
                options.DirectoryPath,
                CancellationToken.None);

            var result = new DirectoryCreateCommandResult
            {
                WorkspaceId = options.WorkspaceId,
                ItemId = options.ItemId,
                DirectoryPath = options.DirectoryPath,
                Success = true,
                Message = $"Directory '{options.DirectoryPath}' created successfully"
            };

            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DirectoryCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory {DirectoryPath} in item {ItemId}. Options: {@Options}", 
                options.DirectoryPath, options.ItemId, options);
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

    public sealed record DirectoryCreateCommandResult
    {
        public string WorkspaceId { get; init; } = string.Empty;
        public string ItemId { get; init; } = string.Empty;
        public string DirectoryPath { get; init; } = string.Empty;
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}

public sealed class DirectoryCreateOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string DirectoryPath { get; set; } = string.Empty;
}