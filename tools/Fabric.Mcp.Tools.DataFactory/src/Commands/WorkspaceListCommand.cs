// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DataFactory.MCP.Abstractions.Interfaces;
using Fabric.Mcp.Tools.DataFactory.Models;
using Fabric.Mcp.Tools.DataFactory.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Commands;

[CommandMetadata(
    Id = "6cafae77-8092-49bf-980b-da2393775d96",
    Name = "list_workspaces",
    Title = "List Workspaces",
    Description = "Lists all Microsoft Fabric workspaces the user has permission for. Returns workspaces filtered by the specified roles if provided. Use this when you need to find workspace IDs for other Data Factory operations.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class WorkspaceListCommand(
    ILogger<WorkspaceListCommand> logger,
    IFabricWorkspaceService workspaceService) : GlobalCommand<WorkspaceListCommand.WorkspaceListOptions>()
{
    private readonly ILogger<WorkspaceListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricWorkspaceService _workspaceService = workspaceService ?? throw new ArgumentNullException(nameof(workspaceService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DataFactoryOptionDefinitions.Roles.AsOptional());
        command.Options.Add(DataFactoryOptionDefinitions.ContinuationToken.AsOptional());
    }

    protected override WorkspaceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Roles = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.Roles.Name);
        options.ContinuationToken = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.ContinuationToken.Name);
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
            var response = await _workspaceService.ListWorkspacesAsync(
                options.Roles,
                options.ContinuationToken);

            _logger.LogInformation("Retrieved {Count} workspaces", response.Value.Count);

            var result = new WorkspaceListCommandResult
            {
                Workspaces = response.Value.Select(w => new WorkspaceInfo
                {
                    Id = w.Id,
                    DisplayName = w.DisplayName,
                    Description = w.Description,
                    Type = w.Type.ToString(),
                    CapacityId = w.CapacityId
                }).ToList(),
                ContinuationToken = response.ContinuationToken,
                TotalCount = response.Value.Count
            };

            context.Response.Results = ResponseResult.Create(result, DataFactoryJsonContext.Default.WorkspaceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing workspaces.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class WorkspaceListOptions : GlobalOptions
    {
        public string? Roles { get; set; }
        public string? ContinuationToken { get; set; }
    }

    public sealed class WorkspaceListCommandResult
    {
        public List<WorkspaceInfo> Workspaces { get; set; } = [];
        public string? ContinuationToken { get; set; }
        public int TotalCount { get; set; }
    }

    public sealed class WorkspaceInfo
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? CapacityId { get; set; }
    }
}
