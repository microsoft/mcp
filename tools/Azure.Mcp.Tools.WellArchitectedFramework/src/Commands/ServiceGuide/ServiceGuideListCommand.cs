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

public sealed class ServiceGuideListCommand(ILogger<ServiceGuideListCommand> logger, IServiceGuideService serviceGuideService)
    : BaseCommand<ServiceGuideListOptions>
{
    private const string CommandTitle = "List Well-Architected Framework Service Guides";
    private readonly ILogger<ServiceGuideListCommand> _logger = logger;
    private readonly IServiceGuideService _serviceGuideService = serviceGuideService;

    public override string Id => "b8e5f0a3-9d4c-5b2f-0a6e-7c3d9f5b8e4a";

    public override string Name => "list";

    public override string Description =>
        "Get Azure Well-Architected Framework guidance for multiple Azure services, " +
        "including architectural best practices, design patterns, and recommendations based on the five pillars: " +
        "reliability, security, cost optimization, operational excellence, and performance efficiency. " +
        "Required option: --services: " + WellArchitectedFrameworkOptionDefinitions.ServicesNameDescription;

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
        command.Options.Add(WellArchitectedFrameworkOptionDefinitions.Services);
    }

    protected override ServiceGuideListOptions BindOptions(ParseResult parseResult)
    {
        return new ServiceGuideListOptions
        {
            Services = parseResult.GetValueOrDefault<string[]>(WellArchitectedFrameworkOptionDefinitions.Services.Name)
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
        context.Activity?.AddTag("WellArchitectedFramework_Services", string.Join(" ", options.Services ?? []));

        try
        {
            var servicesWithGuidance = new List<(string ServiceName, string ServiceGuideUrl)>();
            var servicesWithoutGuidance = new List<string>();

            if (options.Services != null)
            {
                foreach (var serviceName in options.Services)
                {
                    var serviceGuideUrl = _serviceGuideService.GetServiceGuideUrl(serviceName);

                    if (string.IsNullOrWhiteSpace(serviceGuideUrl))
                    {
                        servicesWithoutGuidance.Add(serviceName);
                    }
                    else
                    {
                        servicesWithGuidance.Add((serviceName, serviceGuideUrl));
                    }
                }
            }

            var results = new List<string>();
            if (servicesWithGuidance.Count > 0)
            {
                results.Add(GetGuidanceAvailable(servicesWithGuidance));
            }
            if (servicesWithoutGuidance.Count > 0)
            {
                results.Add(GetGuidanceNotAvailable(servicesWithoutGuidance));
            }

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(results, WellArchitectedFrameworkJsonContext.Default.ListString);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Well-Architected Framework guidance for services: {Services}", string.Join(" ", options.Services ?? []));
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    private string GetGuidanceAvailable(List<(string ServiceName, string ServiceGuideUrl)> servicesWithGuidance)
    {
        var serviceLines = servicesWithGuidance.Select(s => $"{s.ServiceName}: {s.ServiceGuideUrl}").ToArray();

        return $"""
            For detailed Azure Well-Architected Framework guidance, please refer to the markdown file at the URL specified for each service:

            {string.Join("\n", serviceLines)}
            """;
    }

    private string GetGuidanceNotAvailable(List<string> servicesWithoutGuidance)
    {
        var servicesList = string.Join(", ", servicesWithoutGuidance.Select(s => $"'{s}'"));

        return $"""
            Azure Well-Architected Framework guidance for the following service(s) is not available: {servicesList}

            Please try a different variation using the following format for the --services option:
            {WellArchitectedFrameworkOptionDefinitions.ServicesNameDescription}

            Supported services include: {_serviceGuideService.GetAllServiceNamesAsCommaSeparatedList()}

            Or visit the following URL for guidance on supported services: https://learn.microsoft.com/azure/well-architected/service-guides
            """;
    }
}
