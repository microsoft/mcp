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
    internal const string DateTimeFormat = "yyyyMMddHHmmss";

    private const string Separator = "_";

    private readonly AzmcpProgram _azmcpProgram;
    private readonly IAzureMcpDatastore _azureMcpDatastore;
    private readonly RunInformation _runInformation;
    private readonly ILogger<ToolAnalyzer> _logger;
    private readonly AppConfiguration _appConfiguration;
    private readonly string _workingDirectory;

    public ToolAnalyzer(AzmcpProgram program, IAzureMcpDatastore azureMcpDatastore, RunInformation runInformation,
        IOptions<AppConfiguration> configuration, ILogger<ToolAnalyzer> logger)
    {
        _azmcpProgram = program;
        _azureMcpDatastore = azureMcpDatastore;
        _runInformation = runInformation;
        _logger = logger;

        _appConfiguration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
        _workingDirectory = _appConfiguration.WorkDirectory
            ?? throw new ArgumentException(
                $"Expected non-null value of {nameof(AppConfiguration.WorkDirectory)} in {nameof(configuration)}");
    }

    public async Task RunAsync(DateTimeOffset analysisTime, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Starting analysis.");

        LogConfiguration();

        var serverName = await _azmcpProgram.GetServerNameAsync();
        var serverVersion = await _azmcpProgram.GetServerVersionAsync();
        var currentTools = await _azmcpProgram.LoadToolsDynamicallyAsync();

        if (currentTools == null)
        {
            _logger.LogError("LoadToolsDynamicallyAsync did not return a result.");
            return;
        }
        else if (currentTools.Tools == null)
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

        var eventTime = _appConfiguration.UseAnalysisTime ? analysisTime : _azmcpProgram.AzMcpBuildDateTime;

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

        if (!_appConfiguration.IsDryRun)
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
        var fileDate = _appConfiguration.UseAnalysisTime ? analysisTime : _azmcpProgram.AzMcpBuildDateTime;

        return $"{fileName}_{fileDate.ToString(DateTimeFormat)}";
    }

    private static string? GetToolArea(Tool tool)
    {
        var split = tool.Command?.Split(" ", 2);
        return split?[0];
    }

    private void LogConfiguration()
    {
        _logger.LogInformation(
            """
            Configuration:
              Working Directory: {WorkingDirectory}
              Is Dry Run: {IsDryRun}
              Use Analysis Time: {UseAnalysisTime}
              Azmcp Exe: {AzmcpExe}
              MCP Tool Events Table Name: {McpToolEventsTableName}
              Database Name: {DatabaseName}
              Query Endpoint: {QueryEndpoint}
              Ingestion Endpoint: {IngestionEndpoint}
            """,
            _workingDirectory,
            _appConfiguration.IsDryRun,
            _appConfiguration.UseAnalysisTime,
            _appConfiguration.AzmcpExe,
            _appConfiguration.McpToolEventsTableName,
            _appConfiguration.DatabaseName,
            _appConfiguration.QueryEndpoint,
            _appConfiguration.IngestionEndpoint);
    }
}
