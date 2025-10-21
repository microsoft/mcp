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

namespace Fabric.Mcp.Tools.OneLake.Commands.Workspace;

public sealed class WorkspaceGetCommand(
    ILogger<WorkspaceGetCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<WorkspaceGetOptions>()
{
    private readonly ILogger<WorkspaceGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "workspace-get";
    public override string Title => "Get Microsoft Fabric Workspace";
    public override string Description => "Get details of a specific Microsoft Fabric workspace by its ID.";

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
    }

    protected override WorkspaceGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceIdName)!;
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            var workspace = await _oneLakeService.GetWorkspaceAsync(
                options.WorkspaceId,
                CancellationToken.None);

            var result = new WorkspaceGetCommandResult(workspace);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.WorkspaceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workspace {WorkspaceId}. Options: {@Options}", options.WorkspaceId, options);
            HandleException(context, ex);
        }
        
        return context.Response;
    }

    public sealed record WorkspaceGetCommandResult(
        Models.Workspace Workspace);
}

public sealed class WorkspaceGetOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
}