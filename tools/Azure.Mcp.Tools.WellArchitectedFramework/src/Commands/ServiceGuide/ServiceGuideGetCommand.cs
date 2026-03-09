// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Tools.WellArchitectedFramework.Commands;
using Azure.Mcp.Tools.WellArchitectedFramework.Options;
using Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using ServiceGuideModel = Azure.Mcp.Tools.WellArchitectedFramework.Models.ServiceGuide;

namespace Azure.Mcp.Tools.WellArchitectedFramework.Commands.ServiceGuide;

public sealed class ServiceGuideGetCommand(ILogger<ServiceGuideGetCommand> logger)
    : BaseCommand<ServiceGuideGetOptions>
{
    private const string CommandTitle = "Get Well-Architected Framework Service Guide";
    private readonly ILogger<ServiceGuideGetCommand> _logger = logger;

    private static Dictionary<string, ServiceGuideModel>? s_serviceGuidesCache;

    public override string Id => "a7d4e9f2-8c3b-4a1e-9f5d-6b2c8e4a7d3f";

    public override string Name => "get";

    public override string Description =>
        "Get Azure Well-Architected Framework guidance for a specific Azure service, " +
        "including architectural best practices, design patterns, and recommendations based on the five pillars: " +
        "reliability, security, cost optimization, operational excellence, and performance efficiency. " +
        "Required option: --service: The Azure service name (case-insensitive; spaces and hyphens are normalized) " +
        "e.g., 'App Service', 'app-service', 'SQL Database', 'sql-database', 'Cosmos DB', 'cosmos-db'";

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

        try
        {
            // Lazy load service guides on first use
            LoadServiceGuides();

            var serviceName = options.Service!;
            var serviceGuideUrl = GetServiceGuideUrl(serviceName);
            var guidance = GetGuidance(serviceName, serviceGuideUrl);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create([guidance], WellArchitectedFrameworkJsonContext.Default.ListString);
            context.Response.Message = string.Empty;

            context.Activity?.AddTag("WellArchitectedFramework_Service", options.Service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Well-Architected Framework guidance for service: {Service}", options.Service);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    private static void LoadServiceGuides()
    {
        if (s_serviceGuidesCache != null)
        {
            return;
        }

        Assembly assembly = typeof(ServiceGuideGetCommand).Assembly;

        try
        {
            string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "service-guides.json");
            string jsonContent = EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
            var serviceGuides = JsonSerializer.Deserialize(
                jsonContent,
                WellArchitectedFrameworkJsonContext.Default.DictionaryStringServiceGuide);
            s_serviceGuidesCache = serviceGuides ?? new Dictionary<string, ServiceGuideModel>();

            return;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            // If loading fails, set to empty dictionary to prevent repeated attempts
            s_serviceGuidesCache = new Dictionary<string, ServiceGuideModel>();

            throw new InvalidOperationException("Missing 'service-guides.json' file", ex);
        }
        catch (JsonException ex)
        {
            // If loading fails, set to empty dictionary to prevent repeated attempts
            s_serviceGuidesCache = new Dictionary<string, ServiceGuideModel>();

            throw new InvalidOperationException("Failed to parse 'service-guides.json' file", ex);
        }
    }

    private static string? GetServiceGuideUrl(string serviceName)
    {
        if (s_serviceGuidesCache == null)
        {
            return null;
        }

        var serviceNameNormalized = NormalizeServiceName(serviceName);
        
        foreach (var kvp in s_serviceGuidesCache)
        {
            if (kvp.Value.ServiceNameVariationsNormalized.Contains(serviceNameNormalized))
            {
                return kvp.Value.ServiceGuideUrl;
            }
        }

        return null;
    }

    private static string NormalizeServiceName(string serviceName)
    {
        return serviceName
            .ToLowerInvariant()
            .Trim()
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty);
    }

    private static string GetGuidance(string serviceName, string? serviceGuideUrl)
    {
        if (string.IsNullOrWhiteSpace(serviceGuideUrl))
        {
            return $"Azure Well-Architected Framework guidance for '{serviceName}' service is not available. " +
                "Please use lowercase with hyphens for the service name or try a different variation of the service name. " +
                $"Supported services include: {GetAllServiceNamesAsCommaSeparatedList()}. " +
                "Or visit the following URL for guidance on supported services: https://learn.microsoft.com/azure/well-architected/service-guides";
        }

        return $"For detailed Azure Well-Architected Framework guidance on {serviceName} service, " +
            $"please refer to the markdown file at this url: {serviceGuideUrl}";
    }

    private static string GetAllServiceNamesAsCommaSeparatedList()
    {
        if (s_serviceGuidesCache == null || s_serviceGuidesCache.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(", ", s_serviceGuidesCache.Keys);
    }
}
