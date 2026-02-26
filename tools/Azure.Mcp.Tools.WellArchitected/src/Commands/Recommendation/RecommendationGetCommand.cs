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

public sealed class RecommendationGetCommand(ILogger<RecommendationGetCommand> logger)
    : GlobalCommand<RecommendationGetOptions>
{
    public override string Id => "3b9d2f71-8e4a-4c5b-b7f3-1d6a9e2c8f4b";
    public override string Name => "get";
    public override string Description => "Get a specific Well-Architected Framework recommendation by ID";
    public override string Title => "Get Recommendation";
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
        command.Options.Add(WellArchitectedOptionDefinitions.RecommendationId.AsRequired());
    }

    protected override RecommendationGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.RecommendationId = parseResult.GetValueOrDefault<string>(WellArchitectedOptionDefinitions.RecommendationId.Name);
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
            context.Activity?.AddTag(WellArchitectedTelemetryTags.RecommendationId, options.RecommendationId);

            if (string.IsNullOrEmpty(options.RecommendationId))
            {
                throw new ArgumentException("Recommendation ID is required", nameof(options.RecommendationId));
            }

            var service = context.GetService<IWellArchitectedService>();
            var recommendation = await service.GetRecommendationAsync(options.RecommendationId, cancellationToken);

            var commandResult = new RecommendationGetCommandResult(recommendation);
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(commandResult, WellArchitectedJsonContext.Default.RecommendationGetCommandResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting recommendation. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record RecommendationGetCommandResult(WafRecommendation? Recommendation);
}
