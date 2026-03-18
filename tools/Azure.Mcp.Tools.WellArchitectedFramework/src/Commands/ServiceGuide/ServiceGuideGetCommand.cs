// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.WellArchitectedFramework.Commands;
using Azure.Mcp.Tools.WellArchitectedFramework.Options;
using Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide;
using Azure.Mcp.Tools.WellArchitectedFramework.Services.ServiceGuide;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
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
        "or list all supported services when no service is specified. " +
        "When a service is provided, returns architectural best practices, design patterns, and recommendations based on the five pillars: " +
        "reliability, security, cost optimization, operational excellence, and performance efficiency. " +
        "Optional: --service: " + WellArchitectedFrameworkOptionDefinitions.ServiceNameDescription + "; " +
        "Optional: --mode: " + WellArchitectedFrameworkOptionDefinitions.ModeDescription;

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
        command.Options.Add(WellArchitectedFrameworkOptionDefinitions.Mode);
    }

    protected override ServiceGuideGetOptions BindOptions(ParseResult parseResult)
    {
        var modeString = parseResult.GetValueOrDefault<string>(WellArchitectedFrameworkOptionDefinitions.Mode.Name);
        var mode = ServiceGuideOutputMode.Summary; // Default

        if (!string.IsNullOrWhiteSpace(modeString))
        {
            if (Enum.TryParse<ServiceGuideOutputMode>(modeString, ignoreCase: true, out var parsedMode))
            {
                mode = parsedMode;
            }
            else
            {
                throw new ArgumentException($"Invalid mode '{modeString}'. Valid values are: {string.Join(", ", Enum.GetNames<ServiceGuideOutputMode>())}");
            }
        }

        return new ServiceGuideGetOptions
        {
            Service = parseResult.GetValueOrDefault<string>(WellArchitectedFrameworkOptionDefinitions.Service.Name),
            Mode = mode
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
        context.Activity?.AddTag("WellArchitectedFramework_Mode", options.Mode);

        try
        {
            var supportedServicesBulletList = GetSupportedServicesBulletList();

            // If no service is specified, return list of all services
            if (string.IsNullOrWhiteSpace(options.Service))
            {
                var listResponse = GetServiceListResponse(supportedServicesBulletList);
                context.Response.Results = ResponseResult.Create([listResponse], WellArchitectedFrameworkJsonContext.Default.ListString);
            }
            else
            {
                // Service is specified
                var serviceName = options.Service;
                var serviceGuideUrl = _serviceGuideService.GetServiceGuideUrl(serviceName);

                // Check if guidance exists for the service
                if (string.IsNullOrWhiteSpace(serviceGuideUrl))
                {
                    var serviceNotAvailableGuidance = GetServiceNotAvailableGuidance(serviceName, supportedServicesBulletList);
                    context.Response.Results = ResponseResult.Create([serviceNotAvailableGuidance], WellArchitectedFrameworkJsonContext.Default.ListString);
                }
                else
                {
                    // Return guidance for service based on output mode
                    if (options.Mode == ServiceGuideOutputMode.Url)
                    {
                        var serviceGuideURLGuidance = GetServiceGuideURLGuidance(serviceName, serviceGuideUrl);
                        context.Response.Results = ResponseResult.Create([serviceGuideURLGuidance], WellArchitectedFrameworkJsonContext.Default.ListString);
                    }
                    else
                    {
                        var serviceGuideSummary = _serviceGuideService.GetServiceGuideSummary(serviceName);
                        context.Response.Results = ResponseResult.Create([serviceGuideSummary], WellArchitectedFrameworkJsonContext.Default.ListString);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(options.Service))
            {
                _logger.LogError(ex, "Error listing services with Well-Architected Framework guidance.");
            }
            else
            {
                _logger.LogError(ex, "Error getting Well-Architected Framework guidance for {Service}.", options.Service);
            }
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    private string GetServiceListResponse(string supportedServicesBulletList)
    {
        var serviceNames = _serviceGuideService.GetAllServiceNames();
        if (serviceNames.Count == 0)
        {
            return "No Azure Well-Architected Framework service guides are currently available.";
        }

        return $"""
            Azure Well-Architected Framework service guides are available for the following services:

            {supportedServicesBulletList}

            To get guidance for a specific service, use this command with the --service <service-name> option.
            """;
    }

    private string GetServiceGuideURLGuidance(string serviceName, string serviceGuideUrl)
    {
        return $"For detailed Azure Well-Architected Framework guidance on '{serviceName}' service, " +
            $"please refer to the markdown file at this URL: {serviceGuideUrl}";
    }

    private string GetServiceNotAvailableGuidance(string serviceName, string supportedServicesBulletList)
    {
        return $"""
            Azure Well-Architected Framework guidance for '{serviceName}' service is not available.

            Please try a different variation of the service name using the following format for the --service option:
            {WellArchitectedFrameworkOptionDefinitions.ServiceNameDescription}

            Supported services:
            {supportedServicesBulletList}

            For more information, visit: https://learn.microsoft.com/azure/well-architected/service-guides
            """;
    }

    private string GetSupportedServicesBulletList()
    {
        var serviceNames = _serviceGuideService.GetAllServiceNames();
        var supportedServicesBulletList = string.Join("\n", serviceNames.Select(name => $"- {name}"));

        return supportedServicesBulletList;
    }
}
