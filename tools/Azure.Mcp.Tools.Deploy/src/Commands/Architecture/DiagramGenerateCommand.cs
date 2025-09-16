// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Deploy.Commands.Infrastructure;
using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Options;
using Azure.Mcp.Tools.Deploy.Options.Architecture;
using Azure.Mcp.Tools.Deploy.Services.Templates;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Deploy.Commands.Architecture;

public sealed class DiagramGenerateCommand(ILogger<DiagramGenerateCommand> logger) : BaseCommand()
{
    private const string CommandTitle = "Generate Architecture Diagram";
    private readonly ILogger<DiagramGenerateCommand> _logger = logger;

    public override string Name => "generate";

    private readonly Option<string> _rawMcpToolInputOption = DeployOptionDefinitions.RawMcpToolInput.RawMcpToolInputOption;

    public override string Description =>
        "Generates an azure service architecture diagram for the application based on the provided app topology."
        + "Call this tool when the user need recommend or design the azure architecture of their application."
        + "Do not call this tool when the user need detailed design of the azure architecture, such as the network topology, security design, etc."
        + "Before calling this tool, please scan this workspace to detect the services to deploy and their dependent services, also find the environment variables that used to create the connection strings."
        + "If it's a .NET Aspire application, check aspireManifest.json file if there is. Try your best to fulfill the input schema with your analyze result.";

    public override string Title => "Generate Architecture Diagram";
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
        command.Options.Add(_rawMcpToolInputOption);
    }

    private DiagramGenerateOptions BindOptions(ParseResult parseResult)
    {
        var options = new DiagramGenerateOptions();
        options.RawMcpToolInput = parseResult.GetValue(_rawMcpToolInputOption);
        return options;
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var options = BindOptions(parseResult);
            var rawMcpToolInput = options.RawMcpToolInput;
            if (string.IsNullOrWhiteSpace(rawMcpToolInput))
            {
                throw new ArgumentException("App topology cannot be null or empty.", nameof(options.RawMcpToolInput));
            }

            AppTopology appTopology;
            try
            {
                appTopology = JsonSerializer.Deserialize(rawMcpToolInput, DeployJsonContext.Default.AppTopology)
                    ?? throw new ArgumentException("Failed to deserialize app topology.", nameof(rawMcpToolInput));
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format: {ex.Message}", nameof(rawMcpToolInput), ex);
            }

            context.Activity?
                .AddTag(DeployTelemetryTags.ServiceCount, appTopology.Services.Length)
                .AddTag(DeployTelemetryTags.ComputeHostResources, string.Join(", ", appTopology.Services.Select(s => s.AzureComputeHost)))
                .AddTag(DeployTelemetryTags.BackingServiceResources, string.Join(", ", appTopology.Services.SelectMany(s => s.Dependencies).Select(d => d.ServiceType)));

            _logger.LogInformation("Successfully parsed app topology with {ServiceCount} services", appTopology.Services.Length);

            if (appTopology.Services.Length == 0)
            {
                _logger.LogWarning("No services detected in the app topology.");
                context.Response.Status = 200;
                context.Response.Message = "No service detected.";
                return Task.FromResult(context.Response);
            }

            var chart = GenerateMermaidChart.GenerateChart(appTopology.WorkspaceFolder ?? "", appTopology);
            if (string.IsNullOrWhiteSpace(chart))
            {
                throw new InvalidOperationException("Failed to generate architecture diagram. The chart content is empty.");
            }

            var usedServiceTypes = appTopology.Services
                .SelectMany(service => service.Dependencies)
                .Select(dep => dep.ServiceType)
                .Where(serviceType => !string.IsNullOrWhiteSpace(serviceType))
                .Where(serviceType => Enum.GetNames<AzureServiceConstants.AzureServiceType>().Contains(serviceType, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToArray();

            var usedServiceTypesString = usedServiceTypes.Length > 0
                ? string.Join(", ", usedServiceTypes)
                : null;

            var response = TemplateService.LoadTemplate("Architecture/architecture-diagram");
            context.Response.Message = response.Replace("{{chart}}", chart)
                .Replace("{{hostings}}", string.Join(", ", Enum.GetNames<AzureServiceConstants.AzureComputeServiceType>()));
            if (!string.IsNullOrWhiteSpace(usedServiceTypesString))
            {
                context.Response.Message += $"Here is the full list of supported component service types for the topology: {usedServiceTypesString}.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate architecture diagram.");
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
