// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.WellArchitectedFramework.Commands;
using Azure.Mcp.Tools.WellArchitectedFramework.Options;
using Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide;
using Azure.Mcp.Tools.WellArchitectedFramework.Services.ServiceGuide;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.WellArchitectedFramework.Commands.ServiceGuide;

public sealed class ServiceGuideGetCommand(ILogger<ServiceGuideGetCommand> logger, IServiceGuideService serviceGuideService)
    : BaseCommand<ServiceGuideGetOptions>
{
    private const string CommandTitle = "Get Well-Architected Framework Service Guide";
    private readonly ILogger<ServiceGuideGetCommand> _logger = logger;
    private readonly IServiceGuideService _serviceGuideService = serviceGuideService;

    public override string Id => "a7d4e9f2-8c3b-4a1e-9f5d-6b2c8e4a7d3f";

    public override string Name => "get";

    public override string Description =>
        "Get Azure Well-Architected Framework guidance for a specific Azure service, " +
        "including architectural best practices, design patterns, and recommendations based on the five pillars: " +
        "reliability, security, cost optimization, operational excellence, and performance efficiency. " +
        "Required option: --service: " + WellArchitectedFrameworkOptionDefinitions.ServiceNameDescription;

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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(WellArchitectedFrameworkOptionDefinitions.Service);
    }

    protected override ServiceGuideGetOptions BindOptions(ParseResult parseResult)
    {
        return new ServiceGuideGetOptions
        {
            Service = parseResult.GetValueOrDefault<string>(WellArchitectedFrameworkOptionDefinitions.Service.Name)
        };
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

        var options = BindOptions(parseResult);
        context.Activity?.AddTag("WellArchitectedFramework_Service", options.Service);

        try
        {
            var serviceName = options.Service!;
            var serviceGuideUrl = _serviceGuideService.GetServiceGuideUrl(serviceName);

            var guidance = string.IsNullOrWhiteSpace(serviceGuideUrl)
                ? GetGuidanceNotAvailable(serviceName)
                : GetGuidanceAvailable(serviceName, serviceGuideUrl);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create([guidance], WellArchitectedFrameworkJsonContext.Default.ListString);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Well-Architected Framework guidance for service: {Service}", options.Service);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    private string GetGuidanceAvailable(string serviceName, string serviceGuideUrl)
    {
        return $"""
            For detailed Azure Well-Architected Framework guidance on '{serviceName}' service,
            please refer to the markdown file at this URL: {serviceGuideUrl}
            """;
    }

    private string GetGuidanceNotAvailable(string serviceName)
    {
        return $"""
            Azure Well-Architected Framework guidance for '{serviceName}' service is not available.

            Please try a different variation of the service name using the following format for the --service option:
            {WellArchitectedFrameworkOptionDefinitions.ServiceNameDescription}

            Supported services include: {_serviceGuideService.GetAllServiceNamesAsCommaSeparatedList()}

            Or visit the following URL for guidance on supported services: https://learn.microsoft.com/azure/well-architected/service-guides
            """;
    }
}
