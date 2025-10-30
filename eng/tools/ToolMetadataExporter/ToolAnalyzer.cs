// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToolMetadataExporter.Models;
using ToolMetadataExporter.Models.Kusto;
using ToolMetadataExporter.Services;
using ToolSelection.Models;

namespace ToolMetadataExporter;

public class ToolAnalyzer
{
    private const string Separator = "_";

    private readonly AzmcpProgram _azmcpExe;
    private readonly IAzureMcpDatastore _azureMcpDatastore;
    private readonly ILogger<ToolAnalyzer> _logger;
    private readonly string _workingDirectory;

    public ToolAnalyzer(AzmcpProgram program, IAzureMcpDatastore azureMcpDatastore,
        IOptions<AppConfiguration> configuration, ILogger<ToolAnalyzer> logger)
    {
        _azmcpExe = program;
        _azureMcpDatastore = azureMcpDatastore;
        _logger = logger;

        _workingDirectory = configuration.Value.WorkDirectory ?? throw new ArgumentNullException(nameof(AppConfiguration.WorkDirectory));
        ;
    }

    public async Task RunAsync(DateTimeOffset analysisTime, bool isDryRun, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting analysis. IsDryRun: {IsDryRun}", isDryRun);

        var serverVersion = await _azmcpExe.GetVersionAsync();
        var currentTools = await _azmcpExe.LoadToolsDynamicallyAsync();

        if (currentTools == null)
        {
            _logger.LogError("LoadToolsDynamicallyAsync did not return a result.");
            return;
        }
        else if (currentTools.Tools == null || currentTools.Tools.Count == 0)
        {
            _logger.LogWarning("azmcp program did not return any tools.");
            return;
        }

        var existingTools = (await _azureMcpDatastore.GetAvailableToolsAsync(cancellationToken)).ToDictionary(x => x.ToolId);

        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Analysis was cancelled.");
            return;
        }

        // Iterate through all the current tools and match them against the
        // state Kusto knows about.
        // For each tool, if there is no matching Tool, it is a new tool.
        // Else, check the ToolName and ToolArea. If either of those are different
        // then there is an update.
        var changes = new List<McpToolEvent>();

        foreach (var tool in currentTools.Tools)
        {
            if (string.IsNullOrEmpty(tool.Id))
            {
                throw new InvalidOperationException($"Tool without an id. Name: {tool.Name}. Command: {tool.Command}");
            }

            var toolArea = GetToolArea(tool)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(toolArea))
            {
                throw new InvalidOperationException($"Tool without a tool area. Name: {tool.Name}. Command: {tool.Command} Id: {tool.Id}");
            }

            var changeEvent = new McpToolEvent
            {
                EventTime = analysisTime,
                ToolId = tool.Id,
                ServerVersion = serverVersion,
            };

            var commandWithSeparator = tool.Command?.Replace(" ", Separator).ToLowerInvariant();

            var hasChange = false;
            if (existingTools.Remove(tool.Id, out var knownValue))
            {
                if (!string.Equals(commandWithSeparator, knownValue.ToolName, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(toolArea, knownValue.ToolArea, StringComparison.OrdinalIgnoreCase))
                {
                    hasChange = true;

                    changeEvent.EventType = McpToolEventType.Updated;
                    changeEvent.ToolName = knownValue.ToolName;
                    changeEvent.ToolArea = knownValue.ToolArea;
                    changeEvent.ReplacedByToolName = commandWithSeparator;
                    changeEvent.ReplacedByToolArea = toolArea;
                }
            }
            else
            {
                hasChange = true;
                changeEvent.EventType = McpToolEventType.Created;
                changeEvent.ToolName = commandWithSeparator;
                changeEvent.ToolArea = toolArea;
            }

            if (hasChange)
            {
                changes.Add(changeEvent);
            }
        }

        // We're done iterating through the newest available tools.
        // Any remaining entries in `existingTool` are ones that got deleted.
        var removals = existingTools.Select(x => new McpToolEvent
        {
            ServerVersion = serverVersion,
            EventTime = analysisTime,
            EventType = McpToolEventType.Deleted,
            ToolId = x.Key,
            ToolName = x.Value.ToolName,
            ToolArea = x.Value.ToolArea,
            ReplacedByToolName = null,
            ReplacedByToolArea = null,
        });

        changes.AddRange(removals);

        cancellationToken.ThrowIfCancellationRequested();

        if (!changes.Any())
        {
            _logger.LogInformation("No changes made.");
            return;
        }

        var outputFile = Path.Combine(_workingDirectory, "tool_changes.json");

        _logger.LogInformation("Tool updates. Writing output to: {FileName}", outputFile);

        var writerOptions = new JsonWriterOptions
        {
            Indented = true,
        };

        using (var ms = new MemoryStream())
        using (var jsonWriter = new Utf8JsonWriter(ms, writerOptions))
        {
            JsonSerializer.Serialize(jsonWriter, changes, ModelsSerializationContext.Default.ListMcpToolEvent);

            try
            {
                await File.WriteAllBytesAsync(outputFile, ms.ToArray(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to {FileName}", outputFile);
            }
        }

        if (!isDryRun)
        {
            _logger.LogInformation("Updating datastore.");
            await _azureMcpDatastore.AddToolEventsAsync(changes, cancellationToken);
        }
    }

    private static string? GetToolArea(Tool tool)
    {
        var split = tool.Command?.Split(" ", 2);
        return split == null ? null : split[0];
    }
}
