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

namespace Azure.Mcp.Tools.WellArchitected.Commands.Recommendation;

public sealed class RecommendationListCommand(ILogger<RecommendationListCommand> logger)
    : GlobalCommand<RecommendationListOptions>
{
    public override string Id => "8a4e7f92-1c6d-4b3a-ae9f-7d2c5e8b1a4f";
    public override string Name => "list";
    public override string Description => "List Well-Architected Framework recommendations";
    public override string Title => "List Recommendations";
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
        command.Options.Add(WellArchitectedOptionDefinitions.Pillar.AsOptional());
        command.Options.Add(WellArchitectedOptionDefinitions.Service.AsOptional());
    }

    protected override RecommendationListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Pillar = parseResult.GetValueOrDefault<string>(WellArchitectedOptionDefinitions.Pillar.Name);
        options.Service = parseResult.GetValueOrDefault<string>(WellArchitectedOptionDefinitions.Service.Name);
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
            context.Activity?
                .AddTag(WellArchitectedTelemetryTags.Pillar, options.Pillar)
                .AddTag(WellArchitectedTelemetryTags.Service, options.Service);

            var service = context.GetService<IWellArchitectedService>();
            var recommendations = await service.ListRecommendationsAsync(options.Pillar, options.Service, cancellationToken);

            var commandResult = new RecommendationListCommandResult(recommendations);
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(commandResult, WellArchitectedJsonContext.Default.RecommendationListCommandResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing recommendations. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record RecommendationListCommandResult(List<WafRecommendation> Recommendations);
}
