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

    private readonly AzmcpProgram _azmcpProgram;
    private readonly IAzureMcpDatastore _azureMcpDatastore;
    private readonly RunInformation _runInformation;
    private readonly ILogger<ToolAnalyzer> _logger;
    private readonly string _workingDirectory;
    private readonly bool _isDryRun;
    private readonly bool _useAnalysisTime;

    public ToolAnalyzer(AzmcpProgram program, IAzureMcpDatastore azureMcpDatastore, RunInformation runInformation,
        IOptions<AppConfiguration> configuration, ILogger<ToolAnalyzer> logger)
    {
        _azmcpProgram = program;
        _azureMcpDatastore = azureMcpDatastore;
        _runInformation = runInformation;
        _logger = logger;

        _workingDirectory = configuration.Value.WorkDirectory
            ?? throw new ArgumentException(
                $"Expected non-null value of {nameof(AppConfiguration.WorkDirectory)} in {nameof(configuration)}");

        _isDryRun = configuration.Value.IsDryRun;
        _useAnalysisTime = configuration.Value.UseAnalysisTime;
    }

    public async Task RunAsync(DateTimeOffset analysisTime, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Starting analysis. IsDryRun: {IsDryRun}", _isDryRun);

        var serverName = await _azmcpProgram.GetServerNameAsync();
        var serverVersion = await _azmcpProgram.GetServerVersionAsync();
        var currentTools = await _azmcpProgram.LoadToolsDynamicallyAsync();

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

        var toolsBaseFileName = await GetOutputFileNameAsync("tool", analysisTime, cancellationToken);
        var toolsFileFullPath = Path.Combine(_workingDirectory, $"{toolsBaseFileName}.json");

        await Utility.SaveToolsToJsonAsync(currentTools, toolsFileFullPath);
        _logger.LogInformation("💾 Saved {Count} tools to {ToolsFilePath}.", currentTools.Tools?.Count, toolsFileFullPath);

        cancellationToken.ThrowIfCancellationRequested();

        var existingTools = (await _azureMcpDatastore.GetAvailableToolsAsync(cancellationToken)).ToDictionary(x => x.ToolId);

        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Analysis was cancelled.");
            return;
        }

        var eventTime = _useAnalysisTime ? analysisTime : _azmcpProgram.AzMcpBuildDateTime;

        // Iterate through all the current tools and match them against the
        // state Kusto knows about.
        // For each tool, if there is no matching Tool, it is a new tool.
        // Else, check the ToolName and ToolArea. If either of those are different
        // then there is an update.
        var changes = new List<McpToolEvent>();

        // Suppress Null deference warning: currentTools.Tools is checked for null above.
        foreach (var tool in currentTools.Tools!)
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
                EventTime = eventTime,
                ToolId = tool.Id,
                ServerName = serverName,
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
            ServerName = serverName,
            ServerVersion = serverVersion,
            EventTime = eventTime,
            EventType = McpToolEventType.Deleted,
            ToolId = x.Key,
            ToolName = x.Value.ToolName,
            ToolArea = x.Value.ToolArea,
            ReplacedByToolName = null,
            ReplacedByToolArea = null,
        });

        changes.AddRange(removals);

        cancellationToken.ThrowIfCancellationRequested();

        if (changes.Count == 0)
        {
            _logger.LogInformation("No changes made.");
            return;
        }

        var filename = await GetOutputFileNameAsync("tool_changes", eventTime, cancellationToken);
        var outputFile = Path.Combine(_workingDirectory, $"{filename}.json");

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

        cancellationToken.ThrowIfCancellationRequested();

        if (!_isDryRun)
        {
            _logger.LogInformation("Updating datastore.");
            await _azureMcpDatastore.AddToolEventsAsync(changes, cancellationToken);
        }
    }

    internal async Task<string> GetOutputFileNameAsync(string baseName, DateTimeOffset analysisTime, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var serverVersion = await _azmcpProgram.GetServerVersionAsync();
        var fileName = await _runInformation.GetRunInfoFileNameAsync(baseName);
        var fileDate = _useAnalysisTime ? analysisTime : _azmcpProgram.AzMcpBuildDateTime;

        return $"{fileName}_{fileDate:yyyyMMddHHmmss}";
    }

    private static string? GetToolArea(Tool tool)
    {
        var split = tool.Command?.Split(" ", 2);
        return split?[0];
    }
}
