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

namespace Azure.Mcp.Tools.WellArchitectedFramework.Commands.ServiceGuide;

public sealed class ServiceGuideGetCommand(ILogger<ServiceGuideGetCommand> logger)
    : BaseCommand<ServiceGuideGetOptions>
{
    private const string CommandTitle = "Get Well-Architected Framework Service Guide";
    private readonly ILogger<ServiceGuideGetCommand> _logger = logger;

    private static Dictionary<string, ServiceGuide>? s_serviceGuidesCache;

    public override string Id => "a7d4e9f2-8c3b-4a1e-9f5d-6b2c8e4a7d3f";

    public override string Name => "get";

    public override string Description =>
        "Get Azure Well-Architected Framework guidance for a specific Azure service, " +
        "including architectural best practices, design patterns, and recommendations based on the five pillars: " +
        "reliability, security, cost optimization, operational excellence, and performance efficiency. " +
        "Required options: - service: The Azure service name in lowercase with hyphens " +
        "(e.g., 'app-service', 'sql-database', 'cosmos-db', 'virtual-machines')";

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

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Lazy load service guides on first use
            LoadServiceGuides();

            var serviceNameExact = options.Service!;
            var serviceNameNormalized = NormalizeServiceName(serviceNameExact);
            var serviceGuideUrl = GetServiceGuideUrl(serviceNameExact, serviceNameNormalized);
            var guidance = GetGuidance(serviceNameExact, serviceGuideUrl);

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

        return context.Response;
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
                ServiceGuideJsonContext.Default.DictionaryStringServiceGuide);
            s_serviceGuidesCache = serviceGuides ?? new Dictionary<string, ServiceGuide>();

            return;
        }
        catch (FileNotFoundException ex)
        {
            // If loading fails, set to empty dictionary to prevent repeated attempts
            s_serviceGuidesCache = new Dictionary<string, ServiceGuide>();

            throw new InvalidOperationException("Missing 'service-guides.json' file", ex);
        }
        catch (JsonException ex)
        {
            // If loading fails, set to empty dictionary to prevent repeated attempts
            s_serviceGuidesCache = new Dictionary<string, ServiceGuide>();

            throw new InvalidOperationException("Failed to parse 'service-guides.json' file", ex);
        }
    }

    private static string NormalizeServiceName(string serviceNameExact)
    {
        return serviceNameExact.ToLowerInvariant().Trim().Replace("-", string.Empty).Replace(" ", string.Empty);
    }

    private static string? GetServiceGuideUrl(string serviceNameExact, string serviceNameNormalized)
    {
        if (s_serviceGuidesCache == null)
        {
            return null;
        }

        // Try exact match on dictionary key
        if (s_serviceGuidesCache.TryGetValue(serviceNameExact, out var entry))
        {
            return entry.ServiceGuideUrl;
        }

        // Try matching by normalized key
        foreach (var kvp in s_serviceGuidesCache)
        {
            var normalizedKey = NormalizeServiceName(kvp.Key);
            if (normalizedKey == serviceNameNormalized)
            {
                return kvp.Value.ServiceGuideUrl;
            }

            // Check serviceNameExact in the entry
            if (NormalizeServiceName(kvp.Value.ServiceNameExact) == serviceNameNormalized)
            {
                return kvp.Value.ServiceGuideUrl;
            }

            // Check serviceNameVariations
            foreach (var variation in kvp.Value.ServiceNameVariations)
            {
                if (NormalizeServiceName(variation) == serviceNameNormalized)
                {
                    return kvp.Value.ServiceGuideUrl;
                }
            }
        }

        return null;
    }

    private static string GetGuidance(string serviceName, string? serviceGuideUrl)
    {
        if (string.IsNullOrWhiteSpace(serviceGuideUrl))
        {
            return $"Azure Well-Architected Framework guidance for '{serviceName}' service is not available. " +
                "Use lowercase with hyphens for the service name. " +
                "Please try again with a different variation of the service name or visit the following URL for guidance on supported services: " +
                "https://learn.microsoft.com/azure/well-architected/service-guides";
        }

        return $"For detailed Azure Well-Architected Framework guidance on {serviceName} service, " +
            $"please refer to the markdown file at this url: {serviceGuideUrl}";
    }
}

internal sealed class ServiceGuide
{
    public required string ServiceNameExact { get; set; }
    public required string[] ServiceNameVariations { get; set; }
    public required string ServiceGuideUrl { get; set; }
}

[System.Text.Json.Serialization.JsonSerializable(typeof(Dictionary<string, ServiceGuide>))]
[System.Text.Json.Serialization.JsonSerializable(typeof(ServiceGuide))]
[System.Text.Json.Serialization.JsonSourceGenerationOptions(PropertyNamingPolicy = System.Text.Json.Serialization.JsonKnownNamingPolicy.CamelCase)]
internal partial class ServiceGuideJsonContext : System.Text.Json.Serialization.JsonSerializerContext
{
}
