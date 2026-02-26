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

namespace Azure.Mcp.Tools.WellArchitected.Commands.ServiceGuide;

public sealed class ServiceGuideGetCommand(ILogger<ServiceGuideGetCommand> logger)
    : GlobalCommand<ServiceGuideGetOptions>
{
    public override string Id => "2d8c5f3a-7e1b-49a6-8d2f-9c4e6b3a1f7d";
    public override string Name => "get";
    public override string Description => "Get a Well-Architected Framework service guide for a specific Azure service";
    public override string Title => "Get Service Guide";
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
        command.Options.Add(WellArchitectedOptionDefinitions.Service.AsRequired());
    }

    protected override ServiceGuideGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
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
            context.Activity?.AddTag(WellArchitectedTelemetryTags.Service, options.Service);

            if (string.IsNullOrEmpty(options.Service))
            {
                throw new ArgumentException("Service is required", nameof(options.Service));
            }

            var service = context.GetService<IWellArchitectedService>();
            var serviceGuide = await service.GetServiceGuideAsync(options.Service, cancellationToken);

            var commandResult = new ServiceGuideGetCommandResult(serviceGuide);
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(commandResult, WellArchitectedJsonContext.Default.ServiceGuideGetCommandResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting service guide. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ServiceGuideGetCommandResult(WafServiceGuide? ServiceGuide);
}
