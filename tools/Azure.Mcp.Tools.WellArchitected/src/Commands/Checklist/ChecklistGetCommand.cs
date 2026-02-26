// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.WellArchitected.Models;
using Azure.Mcp.Tools.WellArchitected.Options;
using Azure.Mcp.Tools.WellArchitected.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.WellArchitected.Commands.Checklist;

public sealed class ChecklistGetCommand(ILogger<ChecklistGetCommand> logger)
    : GlobalCommand<ChecklistGetOptions>
{
    public override string Id => "6e1f4a8c-9b2d-4f7e-a3c9-8d5b2e7f1a6c";
    public override string Name => "get";
    public override string Description => "Get a Well-Architected Framework checklist for a specific pillar";
    public override string Title => "Get Checklist";
    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(WellArchitectedOptionDefinitions.Pillar.AsRequired());
    }

    protected override ChecklistGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Pillar = parseResult.GetValueOrDefault<string>(WellArchitectedOptionDefinitions.Pillar.Name);
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
            context.Activity?.AddTag(WellArchitectedTelemetryTags.Pillar, options.Pillar);

            if (string.IsNullOrEmpty(options.Pillar))
            {
                throw new ArgumentException("Pillar is required", nameof(options.Pillar));
            }

            var service = context.GetService<IWellArchitectedService>();
            var checklist = await service.GetChecklistAsync(options.Pillar, cancellationToken);

            var commandResult = new ChecklistGetCommandResult(checklist);
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(commandResult, WellArchitectedJsonContext.Default.ChecklistGetCommandResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting checklist. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ChecklistGetCommandResult(WafChecklist? Checklist);
}
