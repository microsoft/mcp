// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ToolMetadataExporter.Models;
using ToolMetadataExporter.Models.Kusto;
using ToolMetadataExporter.Services;
using ToolSelection.Models;
using Xunit;

namespace ToolMetadataExporter.UnitTests;

public class ToolAnalyzerTests : IDisposable
{
    private readonly AzmcpProgram _azmcpProgram;
    private readonly IAzureMcpDatastore _datastore;
    private readonly ILogger<ToolAnalyzer> _logger;
    private readonly IOptions<AppConfiguration> _options;
    private readonly AppConfiguration _appConfiguration;
    private readonly string _tempWorkingDirectory;
    private readonly RunInformation _runInformation;

    public ToolAnalyzerTests()
    {
        var utilityLogger = Substitute.For<ILogger<Utility>>();
        var utility = Substitute.ForPartsOf<Utility>(utilityLogger);
        var logger = Substitute.For<ILogger<AzmcpProgram>>();
        var programOptions = Substitute.For<IOptions<AppConfiguration>>();
        var programConfig = new AppConfiguration
        {
            WorkDirectory = Path.GetTempPath(),
            AzmcpExe = "azmcp"
        };
        programOptions.Value.Returns(programConfig);

        _azmcpProgram = Substitute.For<AzmcpProgram>();
        _datastore = Substitute.For<IAzureMcpDatastore>();
        _logger = Substitute.For<ILogger<ToolAnalyzer>>();
        _options = Substitute.For<IOptions<AppConfiguration>>();

        // Create a temporary directory for working files
        _tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempWorkingDirectory);

        _appConfiguration = new AppConfiguration
        {
            WorkDirectory = _tempWorkingDirectory,
            IsDryRun = false
        };

        _options.Value.Returns(_appConfiguration);

        _runInformation = new RunInformation(_azmcpProgram);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempWorkingDirectory))
            {
                Directory.Delete(_tempWorkingDirectory, recursive: true);
            }
        }
        catch (Exception)
        {
            // Suppress cleanup exceptions to avoid failing tests
        }
    }

    [Fact]
    public void Constructor_ThrowsWhenWorkDirectoryIsNull()
    {
        // Arrange
        var invalidConfig = new AppConfiguration
        {
            WorkDirectory = null,
            IsDryRun = false
        };
        var invalidOptions = Substitute.For<IOptions<AppConfiguration>>();
        invalidOptions.Value.Returns(invalidConfig);
        var runInfo = new RunInformation(_azmcpProgram);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new ToolAnalyzer(_azmcpProgram, _datastore, runInfo, invalidOptions, _logger));
    }

    [Fact]
    public async Task RunAsync_ReturnsEarly_WhenLoadToolsDynamicallyReturnsNull()
    {
        // Arrange
        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");
        var toolsResult = Task.FromResult<ListToolsResult?>(null);

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(DateTimeOffset.UtcNow, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.DidNotReceive().GetAvailableToolsAsync(Arg.Any<CancellationToken>());
        await _datastore.DidNotReceive().AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_ReturnsEarly_WhenToolsListIsNull()
    {
        // Arrange
        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");
        var toolsResult = Task.FromResult<ListToolsResult?>(new ListToolsResult { Tools = null });

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(DateTimeOffset.UtcNow, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.DidNotReceive().GetAvailableToolsAsync(Arg.Any<CancellationToken>());
        await _datastore.DidNotReceive().AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_ThrowsException_WhenToolHasNoId()
    {
        // Arrange
        var availableTools = Task.FromResult<IList<AzureMcpTool>>([]);

        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");
        var toolsResult = Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools =
            [
                new Tool { Id = null, Name = "test-tool", Command = "area command" }
            ]
        });

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await analyzer.RunAsync(DateTimeOffset.UtcNow, TestContext.Current.CancellationToken));
        Assert.Contains("Tool without an id", exception.Message);
    }

    [Fact]
    public async Task RunAsync_ThrowsException_WhenToolHasNoCommand()
    {
        // Arrange
        var availableTools = Task.FromResult<IList<AzureMcpTool>>([]);

        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");
        var toolsResult = Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools =
            [
                new Tool { Id = "tool-1", Name = "test-tool", Command = null }
            ]
        });

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await analyzer.RunAsync(DateTimeOffset.UtcNow, TestContext.Current.CancellationToken));
        Assert.Contains("Tool without a tool area", exception.Message);
    }

    public static TheoryData<IList<AzureMcpTool>, int> RunAsync_Updates_WhenToolsListIsEmptyArgs =>
        new()
        {
            { new List<AzureMcpTool> { new("tool-1", "area command", "area") }, 1 },
            { new List<AzureMcpTool>(), 0 },
        };

    /// <summary>
    /// Verify the case that if the current tools list is empty, we check against the datastore.
    /// Happens if previous version there were tools, but in the current version there are none.
    /// </summary>
    [Theory]
    [MemberData(nameof(RunAsync_Updates_WhenToolsListIsEmptyArgs))]
    public async Task RunAsync_Updates_WhenToolsListIsEmpty(IList<AzureMcpTool> existingTools, int numberOfExpectedDatabaseUpdateCalls)
    {
        // Arrange
        var toolsListResult = new ListToolsResult() { Tools = [] };
        var result = Task.FromResult<ListToolsResult?>(toolsListResult);

        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(result);

        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(existingTools));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(DateTimeOffset.UtcNow, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.Received(1).GetAvailableToolsAsync(Arg.Any<CancellationToken>());
        await _datastore.Received(numberOfExpectedDatabaseUpdateCalls)
            .AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_DetectsNewTool()
    {
        // Arrange
        var analysisTime = DateTimeOffset.UtcNow;
        var serverName = "test-server";
        var serverVersion = "1.0.0";
        var tool = new Tool { Id = "tool-1", Name = "New Tool", Command = "area command" };
        var expectedToolName = "area_command";
        var expectedToolArea = "area";

        var availableTools = Task.FromResult<IList<AzureMcpTool>>([]);

        var serverNameResult = Task.FromResult(serverName);
        var serverVersionResult = Task.FromResult(serverVersion);
        var toolsResult = Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool]
        });

        _azmcpProgram.GetServerNameAsync().Returns(serverNameResult);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersionResult);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(analysisTime, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.Received(1).AddToolEventsAsync(
            Arg.Is<List<McpToolEvent>>(events =>
                events.Count == 1 &&
                events[0].EventType == McpToolEventType.Created &&
                events[0].ToolId == tool.Id &&
                events[0].ToolName == expectedToolName &&
                events[0].ToolArea == expectedToolArea &&
                events[0].ServerName == serverName &&
                events[0].ServerVersion == serverVersion),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_DetectsUpdatedTool()
    {
        // Arrange
        var analysisTime = DateTimeOffset.UtcNow;
        var serverName = "test-server";
        var serverVersion = "1.0.0";
        var tool = new Tool { Id = "tool-1", Name = "Updated Tool", Command = "newarea newcommand" };
        var existingTool = new AzureMcpTool("tool-1", "oldarea_oldcommand", "oldarea");
        var expectedNewToolName = "newarea_newcommand";
        var expectedNewToolArea = "newarea";

        var toolsListResult = new ListToolsResult() { Tools = [tool] };
        var result = Task.FromResult<ListToolsResult?>(toolsListResult);

        var availableTools = Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool
        ]);

        var serverNameResult = Task.FromResult(serverName);
        var serverVersionResult = Task.FromResult(serverVersion);

        _azmcpProgram.GetServerNameAsync().Returns(serverNameResult);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersionResult);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(result);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(analysisTime, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.Received(1).AddToolEventsAsync(
            Arg.Is<List<McpToolEvent>>(events =>
                events.Count == 1 &&
                events[0].EventType == McpToolEventType.Updated &&
                events[0].ToolId == tool.Id &&
                events[0].ToolName == existingTool.ToolName &&
                events[0].ToolArea == existingTool.ToolArea &&
                events[0].ReplacedByToolName == expectedNewToolName &&
                events[0].ReplacedByToolArea == expectedNewToolArea),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_DetectsDeletedTool()
    {
        // Arrange
        var analysisTime = DateTimeOffset.UtcNow;
        var serverName = "test-server";
        var serverVersion = "1.0.0";
        var existingTool = new AzureMcpTool("tool-1", "area_command", "area");

        var toolsListResult = new ListToolsResult() { Tools = [] };
        var result = Task.FromResult<ListToolsResult?>(toolsListResult);

        var availableTools = Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool
        ]);

        var serverNameResult = Task.FromResult(serverName);
        var serverVersionResult = Task.FromResult(serverVersion);

        _azmcpProgram.GetServerNameAsync().Returns(serverNameResult);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersionResult);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(result);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);
        _datastore.AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(analysisTime, TestContext.Current.CancellationToken);

         
        // Assert
        await _datastore.Received(1).AddToolEventsAsync(
            Arg.Is<List<McpToolEvent>>(events =>
                events.Count == 1 &&
                events[0].EventType == McpToolEventType.Deleted &&
                events[0].ToolId == existingTool.ToolId &&
                events[0].ToolName == existingTool.ToolName &&
                events[0].ToolArea == existingTool.ToolArea &&
                events[0].ReplacedByToolName == null &&
                events[0].ReplacedByToolArea == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_DoesNotDetectChange_WhenToolUnchanged()
    {
        // Arrange
        var tool = new Tool { Id = "tool-1", Name = "Test Tool", Command = "area command" };
        var existingTool = new AzureMcpTool("tool-1", "area_command", "area");

        var toolsListResult = new ListToolsResult() { Tools = [tool] };
        var result = Task.FromResult<ListToolsResult?>(toolsListResult);

        var availableTools = Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool
        ]);

        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(result);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(DateTimeOffset.UtcNow, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.DidNotReceive().AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_HandlesMultipleChanges()
    {
        // Arrange
        var analysisTime = DateTimeOffset.UtcNow;
        var tool1 = new Tool { Id = "tool-1", Name = "Tool 1", Command = "area1 command1" }; // Unchanged
        var tool2 = new Tool { Id = "tool-2", Name = "Tool 2", Command = "area2 newcommand" }; // Updated
        var tool4 = new Tool { Id = "tool-4", Name = "Tool 4", Command = "area4 command4" }; // New
        var existingTool1 = new AzureMcpTool("tool-1", "area1_command1", "area1");
        var existingTool2 = new AzureMcpTool("tool-2", "area2_oldcommand", "area2");
        var existingTool3 = new AzureMcpTool("tool-3", "area3_command3", "area3"); // Deleted

        var availableTools = Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool1,
            existingTool2,
            existingTool3
        ]);

        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");
        var toolsResult = Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool1, tool2, tool4]
        });

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(analysisTime, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.Received(1).AddToolEventsAsync(
            Arg.Is<List<McpToolEvent>>(events =>
                events.Count == 3 &&
                events.Any(e => e.EventType == McpToolEventType.Updated && e.ToolId == tool2.Id) &&
                events.Any(e => e.EventType == McpToolEventType.Created && e.ToolId == tool4.Id) &&
                events.Any(e => e.EventType == McpToolEventType.Deleted && e.ToolId == existingTool3.ToolId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WritesChangesToFile()
    {
        // Arrange
        _appConfiguration.UseAnalysisTime = false;
        var analysisTime = DateTimeOffset.UtcNow;
        var tool = new Tool { Id = "tool-1", Name = "New Tool", Command = "area command" };

        var availableTools = Task.FromResult<IList<AzureMcpTool>>([]);

        var version = "1.0.0";
        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult(version);
        var toolsResult = Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool]
        });

        var buildDate = DateTimeOffset.FromUnixTimeMilliseconds(1769181072910);

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.AzMcpBuildDateTime.Returns(buildDate);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        var expectedFileName = $"{version}_tool_changes_{buildDate.ToString(ToolAnalyzer.DateTimeFormat)}.json";

        // Act
        await analyzer.RunAsync(analysisTime, TestContext.Current.CancellationToken);

        // Assert
        var outputFile = Path.Combine(_tempWorkingDirectory, expectedFileName);
        Assert.True(File.Exists(outputFile), $"Expected {outputFile} to exist.");
        var fileContent = await File.ReadAllTextAsync(outputFile, TestContext.Current.CancellationToken);
        Assert.Contains(tool.Id, fileContent);
        Assert.Contains("Created", fileContent);
    }

    [Fact]
    public async Task RunAsync_SkipsDatastoreUpdate_WhenDryRunIsTrue()
    {
        // Arrange
        _appConfiguration.IsDryRun = true;
        _appConfiguration.UseAnalysisTime = true;

        var version = "1.0.0";
        var analysisTime = DateTimeOffset.UtcNow;
        var analysisTimeAsString = analysisTime.ToString(ToolAnalyzer.DateTimeFormat);

        var availableTools = Task.FromResult<IList<AzureMcpTool>>([]);

        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult(version);
        var toolsResult = Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools =
            [
                new Tool { Id = "tool-1", Name = "New Tool", Command = "area command" }
            ]
        });

        var expectedFileName = $"{version}_tool_changes_{analysisTimeAsString}.json";

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);
        _datastore.AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(analysisTime, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.DidNotReceive().AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>());

        // But file should still be written
        var outputFile = Path.Combine(_tempWorkingDirectory, expectedFileName);
        Assert.True(File.Exists(outputFile), $"Expected {outputFile} to exist.");
    }

    [Fact]
    public async Task RunAsync_HandlesCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var availableTools = Task.FromResult<IList<AzureMcpTool>>([]);

        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");
        var toolsResult = Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools =
            [
                new Tool { Id = "tool-1", Name = "Test Tool", Command = "area command" }
            ]
        });

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(() => analyzer.RunAsync(DateTimeOffset.UtcNow, cts.Token));

        // Assert - Should return early without throwing
        await _datastore.DidNotReceive().AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_NormalizesCommandToLowercase()
    {
        // Arrange
        var analysisTime = DateTimeOffset.UtcNow;
        var tool = new Tool { Id = "tool-1", Name = "Test Tool", Command = "Area Command" };
        var expectedToolName = "area_command";
        var expectedToolArea = "area";

        var availableTools = Task.FromResult<IList<AzureMcpTool>>([]);

        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");
        var toolsResult = Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool]
        });

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(analysisTime, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.Received(1).AddToolEventsAsync(
            Arg.Is<List<McpToolEvent>>(events =>
                events.Count == 1 &&
                events[0].ToolName == expectedToolName &&
                events[0].ToolArea == expectedToolArea),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_ReplacesSpacesWithUnderscores()
    {
        // Arrange
        var analysisTime = DateTimeOffset.UtcNow;

        var availableTools = Task.FromResult<IList<AzureMcpTool>>([]);

        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");
        var toolsResult = Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools =
            [
                new Tool { Id = "tool-1", Name = "Test Tool", Command = "area command with spaces" }
            ]
        });

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(analysisTime, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.Received(1).AddToolEventsAsync(
            Arg.Is<List<McpToolEvent>>(events =>
                events.Count == 1 &&
                events[0].ToolName == "area_command_with_spaces"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_DetectsToolAreaChange()
    {
        // Arrange
        var analysisTime = DateTimeOffset.UtcNow;
        var tool = new Tool { Id = "tool-1", Name = "Test Tool", Command = "newarea command" };
        var existingTool = new AzureMcpTool("tool-1", "oldarea_command", "oldarea");
        var expectedNewToolArea = "newarea";

        var availableTools = Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool
        ]);

        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");
        var toolsResult = Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool]
        });

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(toolsResult);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(analysisTime, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.Received(1).AddToolEventsAsync(
            Arg.Is<List<McpToolEvent>>(events =>
                events.Count == 1 &&
                events[0].EventType == McpToolEventType.Updated &&
                events[0].ToolArea == existingTool.ToolArea &&
                events[0].ReplacedByToolArea == expectedNewToolArea),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_IsCaseInsensitive_ForComparison()
    {
        // Arrange
        var tool = new Tool { Id = "tool-1", Name = "Test Tool", Command = "AREA COMMAND" };
        var existingTool = new AzureMcpTool("tool-1", "area_command", "area");

        var toolsListResult = new ListToolsResult() { Tools = [tool] };
        var result = Task.FromResult<ListToolsResult?>(toolsListResult);

        var availableTools = Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool
        ]);

        var serverName = Task.FromResult("test-server");
        var serverVersion = Task.FromResult("1.0.0");

        _azmcpProgram.GetServerNameAsync().Returns(serverName);
        _azmcpProgram.GetServerVersionAsync().Returns(serverVersion);
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(result);
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(availableTools);

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _runInformation, _options, _logger);

        // Act
        await analyzer.RunAsync(DateTimeOffset.UtcNow, TestContext.Current.CancellationToken);

        // Assert - Should not detect any changes
        await _datastore.DidNotReceive().AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>());
    }
}
