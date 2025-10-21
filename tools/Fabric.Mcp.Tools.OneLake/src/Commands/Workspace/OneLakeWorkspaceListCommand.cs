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

public sealed class OneLakeWorkspaceListCommand(
    ILogger<OneLakeWorkspaceListCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<WorkspaceListOptions>()
{
    private readonly ILogger<OneLakeWorkspaceListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "onelake-workspace-list";
    public override string Title => "List OneLake Workspaces";
    public override string Description => "List all OneLake workspaces using the OneLake data plane API.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.ContinuationToken.AsOptional());
        command.Options.Add(OneLakeOptionDefinitions.Format.AsOptional());
    }

    protected override WorkspaceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ContinuationToken = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ContinuationToken.Name);
        options.Format = parseResult.GetValueOrDefault<string>(OneLakeOptionDefinitions.Format.Name) ?? "json";
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            if (options.Format?.ToLowerInvariant() == "xml")
            {
                var xmlResponse = await _oneLakeService.ListOneLakeWorkspacesXmlAsync(
                    options.ContinuationToken,
                    CancellationToken.None);

                _logger.LogInformation("Retrieved OneLake workspaces XML response with length: {Length}", xmlResponse.Length);

                var result = new OneLakeWorkspaceListCommandResult { XmlResponse = xmlResponse };
                context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeWorkspaceListCommandResult);
            }
            else
            {
                var workspaces = await _oneLakeService.ListOneLakeWorkspacesAsync(
                    options.ContinuationToken,
                    CancellationToken.None);

                var workspaceList = workspaces.ToList();
                _logger.LogInformation("Retrieved {Count} OneLake workspaces", workspaceList.Count);

                var result = new OneLakeWorkspaceListCommandResult { Workspaces = workspaceList };
                context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeWorkspaceListCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing OneLake workspaces. Options: {@Options}", options);
            HandleException(context, ex);
        }
        
        return context.Response;
    }

    public class OneLakeWorkspaceListCommandResult
    {
        public List<Models.Workspace>? Workspaces { get; set; }
        public string? XmlResponse { get; set; }

        public OneLakeWorkspaceListCommandResult(List<Models.Workspace> workspaces)
        {
            Workspaces = workspaces ?? throw new ArgumentNullException(nameof(workspaces));
        }

        public OneLakeWorkspaceListCommandResult()
        {
        }
    }
}