// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
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

public class AssemblyResourceConsolidatedToolDefinitionProvider : IConsolidatedToolDefinitionProvider
{
    private ILogger<AssemblyResourceConsolidatedToolDefinitionProvider> _logger;
    private readonly Assembly _sourceAssembly;
    private readonly string _resourceName;

    public AssemblyResourceConsolidatedToolDefinitionProvider(ILogger<AssemblyResourceConsolidatedToolDefinitionProvider> logger, Assembly sourceAssembly, string resourceName)
    {
        _logger = logger;
        _sourceAssembly = sourceAssembly;
        _resourceName = resourceName;
    }

    public List<ConsolidatedToolDefinition> GetToolDefinitions()
    {
        try
        {
            using Stream? stream = _sourceAssembly.GetManifestResourceStream(_resourceName);
            if (stream == null)
            {
                var errorMessage = $"Failed to load embedded resource '{_resourceName}'";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

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
