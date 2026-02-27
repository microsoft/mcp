// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Models;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;

public interface IConsolidatedToolDefinitionProvider
{
    List<ConsolidatedToolDefinition> GetToolDefinitions();
}

public class NullConsolidatedToolDefinitionProvider : IConsolidatedToolDefinitionProvider
{
    public List<ConsolidatedToolDefinition> GetToolDefinitions() => [];
}

public class ResourceConsolidatedToolDefinitionProvider : IConsolidatedToolDefinitionProvider
{
    private ILogger<ResourceConsolidatedToolDefinitionProvider> _logger;
    private readonly Assembly _sourceAssembly;
    private readonly string _resourcePattern;

    public ResourceConsolidatedToolDefinitionProvider(ILogger<ResourceConsolidatedToolDefinitionProvider> logger, Assembly sourceAssembly, string resourcePattern)
    {
        _logger = logger;
        _sourceAssembly = sourceAssembly;
        _resourcePattern = resourcePattern;
    }

    public List<ConsolidatedToolDefinition> GetToolDefinitions()
    {
        try
        {
            var resourceName = EmbeddedResourceHelper.FindEmbeddedResource(_sourceAssembly, _resourcePattern);
            var json = EmbeddedResourceHelper.ReadEmbeddedResource(_sourceAssembly, resourceName);

            using var jsonDoc = JsonDocument.Parse(json);
            if (!jsonDoc.RootElement.TryGetProperty("consolidated_tools", out var toolsArray))
            {
                var errorMessage = "Property 'consolidated_tools' not found in consolidated-tools.json";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            return JsonSerializer.Deserialize(toolsArray.GetRawText(), ServerJsonContext.Default.ListConsolidatedToolDefinition) ?? new List<ConsolidatedToolDefinition>();
        }
        catch (Exception ex)
        {
            var errorMessage = "Failed to load consolidated tools from JSON file";
            _logger.LogError(ex, errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
    }
}
