// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.WellArchitectedFramework.Commands;
using Azure.Mcp.Tools.WellArchitectedFramework.Services.UsageGuide;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.WellArchitectedFramework.Commands.UsageGuide;

public sealed class UsageGuideGetCommand(ILogger<UsageGuideGetCommand> logger, IUsageGuideService usageGuideService)
    : BaseCommand<GlobalOptions>
{
    private const string CommandTitle = "Get Well-Architected Framework Usage Guide";
    private readonly ILogger<UsageGuideGetCommand> _logger = logger;
    private readonly IUsageGuideService _usageGuideService = usageGuideService;

    public override string Id => "b8e5f0a3-9d4c-5b2f-0a6e-7c3d9f5b8e4a";

    public override string Name => "get";

    public override string Description =>
        "Get the Azure Well-Architected Framework usage guide for AI agents. " +
        "This guide provides systematic instructions on how to apply the framework when architecting new Azure workloads, " +
        "covering the five pillars (reliability, security, cost optimization, operational excellence, and performance efficiency) " +
        "and their application through design principles, design strategies, and configuration recommendations.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override GlobalOptions BindOptions(ParseResult parseResult)
    {
        return new GlobalOptions();
    }

    public override Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        BindOptions(parseResult);

        try
        {
            var usageGuide = _usageGuideService.GetUsageGuide();

            if (string.IsNullOrWhiteSpace(usageGuide))
            {
                context.Response.Results = ResponseResult.Create(
                    ["Azure Well-Architected Framework usage guide is not available."],
                    WellArchitectedFrameworkJsonContext.Default.ListString);
            }
            else
            {
                context.Response.Results = ResponseResult.Create(
                    [usageGuide],
                    WellArchitectedFrameworkJsonContext.Default.ListString);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Well-Architected Framework usage guide.");
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
