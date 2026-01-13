// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

    public ToolAnalyzerTests()
    {
        var utility = Substitute.For<Utility>();
        var logger = Substitute.For<ILogger<AzmcpProgram>>();
        var programOptions = Substitute.For<IOptions<AppConfiguration>>();
        var programConfig = new AppConfiguration
        {
            WorkDirectory = Path.GetTempPath(),
            AzmcpExe = "azmcp"
        };
        programOptions.Value.Returns(programConfig);

        _azmcpProgram = Substitute.ForPartsOf<AzmcpProgram>(utility, programOptions, logger);
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

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ToolAnalyzer(_azmcpProgram, _datastore, invalidOptions, _logger));
    }

    [Fact]
    public async Task RunAsync_ReturnsEarly_WhenLoadToolsDynamicallyReturnsNull()
    {
        // Arrange
        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(null));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

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
        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult { Tools = null }));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

        // Act
        await analyzer.RunAsync(DateTimeOffset.UtcNow, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.DidNotReceive().GetAvailableToolsAsync(Arg.Any<CancellationToken>());
        await _datastore.DidNotReceive().AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_ReturnsEarly_WhenToolsListIsEmpty()
    {
        // Arrange
        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult { Tools = [] }));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

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
        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools =
            [
                new Tool { Id = null, Name = "test-tool", Command = "area command" }
            ]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>([]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await analyzer.RunAsync(DateTimeOffset.UtcNow, TestContext.Current.CancellationToken));
        Assert.Contains("Tool without an id", exception.Message);
    }

    [Fact]
    public async Task RunAsync_ThrowsException_WhenToolHasNoCommand()
    {
        // Arrange
        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools =
            [
                new Tool { Id = "tool-1", Name = "test-tool", Command = null }
            ]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>([]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await analyzer.RunAsync(DateTimeOffset.UtcNow, TestContext.Current.CancellationToken));
        Assert.Contains("Tool without a tool area", exception.Message);
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

        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult(serverName));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult(serverVersion));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>([]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

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

        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult(serverName));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult(serverVersion));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool
        ]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

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

        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult(serverName));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult(serverVersion));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = []
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool
        ]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

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

        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool
        ]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

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

        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool1, tool2, tool4]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool1,
            existingTool2,
            existingTool3
        ]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

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
        var analysisTime = DateTimeOffset.UtcNow;
        var tool = new Tool { Id = "tool-1", Name = "New Tool", Command = "area command" };

        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>([]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

        // Act
        await analyzer.RunAsync(analysisTime, TestContext.Current.CancellationToken);

        // Assert
        var outputFile = Path.Combine(_tempWorkingDirectory, "tool_changes.json");
        Assert.True(File.Exists(outputFile));
        var fileContent = await File.ReadAllTextAsync(outputFile, TestContext.Current.CancellationToken);
        Assert.Contains(tool.Id, fileContent);
        Assert.Contains("Created", fileContent);
    }

    [Fact]
    public async Task RunAsync_SkipsDatastoreUpdate_WhenDryRunIsTrue()
    {
        // Arrange
        _appConfiguration.IsDryRun = true;
        var analysisTime = DateTimeOffset.UtcNow;
        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools =
            [
                new Tool { Id = "tool-1", Name = "New Tool", Command = "area command" }
            ]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>([]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

        // Act
        await analyzer.RunAsync(analysisTime, TestContext.Current.CancellationToken);

        // Assert
        await _datastore.DidNotReceive().AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>());

        // But file should still be written
        var outputFile = Path.Combine(_tempWorkingDirectory, "tool_changes.json");
        Assert.True(File.Exists(outputFile));
    }

    [Fact]
    public async Task RunAsync_HandlesCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools =
            [
                new Tool { Id = "tool-1", Name = "Test Tool", Command = "area command" }
            ]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>([]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

        // Act
        await analyzer.RunAsync(DateTimeOffset.UtcNow, cts.Token);

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

        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>([]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

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
        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools =
            [
                new Tool { Id = "tool-1", Name = "Test Tool", Command = "area command with spaces" }
            ]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>([]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

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

        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool
        ]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

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

        _azmcpProgram.GetServerNameAsync().Returns(Task.FromResult("test-server"));
        _azmcpProgram.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));
        _azmcpProgram.LoadToolsDynamicallyAsync().Returns(Task.FromResult<ListToolsResult?>(new ListToolsResult
        {
            Tools = [tool]
        }));
        _datastore.GetAvailableToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IList<AzureMcpTool>>(
        [
            existingTool
        ]));

        var analyzer = new ToolAnalyzer(_azmcpProgram, _datastore, _options, _logger);

        // Act
        await analyzer.RunAsync(DateTimeOffset.UtcNow, TestContext.Current.CancellationToken);

        // Assert - Should not detect any changes
        await _datastore.DidNotReceive().AddToolEventsAsync(Arg.Any<List<McpToolEvent>>(), Arg.Any<CancellationToken>());
    }
}
