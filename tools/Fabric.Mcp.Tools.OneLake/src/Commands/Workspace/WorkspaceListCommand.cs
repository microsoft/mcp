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

public sealed class WorkspaceListCommand(
    ILogger<WorkspaceListCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<WorkspaceListOptions>()
{
    private readonly ILogger<WorkspaceListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "workspace-list";
    public override string Title => "List Microsoft Fabric Workspaces";
    public override string Description => "List all Microsoft Fabric workspaces accessible to the user.";

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

    protected override WorkspaceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ContinuationToken = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ContinuationTokenName);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            var workspaces = await _oneLakeService.ListWorkspacesAsync(
                options.ContinuationToken,
                CancellationToken.None);

            var result = new WorkspaceListCommandResult(workspaces);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.WorkspaceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing workspaces. Options: {@Options}", options);
            HandleException(context, ex);
        }
        
        return context.Response;
    }

    public sealed record WorkspaceListCommandResult(
        IEnumerable<Models.Workspace> Workspaces);
}

public sealed class WorkspaceListOptions : GlobalOptions
{
    public string? ContinuationToken { get; set; }
    public string? Format { get; set; }
}