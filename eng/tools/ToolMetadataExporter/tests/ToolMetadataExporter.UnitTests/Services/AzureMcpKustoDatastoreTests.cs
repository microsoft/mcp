// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data;
using Kusto.Data.Common;
using Kusto.Ingest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ToolMetadataExporter.Models.Kusto;
using ToolMetadataExporter.Services;
using Xunit;

namespace ToolMetadataExporter.UnitTests.Services;

public class AzureMcpKustoDatastoreTests : IDisposable
{
    private readonly ICslQueryProvider _kustoClient;
    private readonly IKustoIngestClient _ingestClient;
    private readonly ILogger<AzureMcpKustoDatastore> _logger;
    private readonly IOptions<AppConfiguration> _options;
    private readonly AppConfiguration _appConfiguration;
    private readonly string _tempQueriesDirectory;

    public AzureMcpKustoDatastoreTests()
    {
        _kustoClient = Substitute.For<ICslQueryProvider>();
        _ingestClient = Substitute.For<IKustoIngestClient>();
        _logger = Substitute.For<ILogger<AzureMcpKustoDatastore>>();
        _options = Substitute.For<IOptions<AppConfiguration>>();

        // Create a temporary directory for queries
        _tempQueriesDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempQueriesDirectory);

        _appConfiguration = new AppConfiguration
        {
            DatabaseName = "TestDatabase",
            McpToolEventsTableName = "McpToolEvents",
            QueriesFolder = _tempQueriesDirectory
        };

        _options.Value.Returns(_appConfiguration);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempQueriesDirectory))
            {
                Directory.Delete(_tempQueriesDirectory, recursive: true);
            }
        }
        catch (Exception)
        {
            // Suppress cleanup exceptions to avoid failing tests
            // The OS will eventually clean up temp directories
        }
    }

    [Fact]
    public void Constructor_ThrowsWhenDatabaseNameIsNull()
    {
        // Arrange
        var invalidConfig = new AppConfiguration
        {
            DatabaseName = null,
            McpToolEventsTableName = "McpToolEvents",
            QueriesFolder = _tempQueriesDirectory
        };
        var invalidOptions = Substitute.For<IOptions<AppConfiguration>>();
        invalidOptions.Value.Returns(invalidConfig);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AzureMcpKustoDatastore(_kustoClient, _ingestClient, invalidOptions, _logger));
    }

    [Fact]
    public void Constructor_ThrowsWhenTableNameIsNull()
    {
        // Arrange
        var invalidConfig = new AppConfiguration
        {
            DatabaseName = "TestDatabase",
            McpToolEventsTableName = null,
            QueriesFolder = _tempQueriesDirectory
        };
        var invalidOptions = Substitute.For<IOptions<AppConfiguration>>();
        invalidOptions.Value.Returns(invalidConfig);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AzureMcpKustoDatastore(_kustoClient, _ingestClient, invalidOptions, _logger));
    }

    [Fact]
    public void Constructor_ThrowsWhenQueriesFolderIsNull()
    {
        // Arrange
        var invalidConfig = new AppConfiguration
        {
            DatabaseName = "TestDatabase",
            McpToolEventsTableName = "McpToolEvents",
            QueriesFolder = null
        };
        var invalidOptions = Substitute.For<IOptions<AppConfiguration>>();
        invalidOptions.Value.Returns(invalidConfig);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AzureMcpKustoDatastore(_kustoClient, _ingestClient, invalidOptions, _logger));
    }

    [Fact]
    public void Constructor_ThrowsWhenQueriesFolderDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var invalidConfig = new AppConfiguration
        {
            DatabaseName = "TestDatabase",
            McpToolEventsTableName = "McpToolEvents",
            QueriesFolder = nonExistentPath
        };
        var invalidOptions = Substitute.For<IOptions<AppConfiguration>>();
        invalidOptions.Value.Returns(invalidConfig);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new AzureMcpKustoDatastore(_kustoClient, _ingestClient, invalidOptions, _logger));
        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public async Task GetAvailableToolsAsync_ThrowsWhenQueryFileNotFound()
    {
        // Arrange
        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await datastore.GetAvailableToolsAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Could not find GetAvailableTools.kql", exception.Message);
    }

    [Fact]
    public async Task GetAvailableToolsAsync_ReturnsCreatedTools()
    {
        // Arrange
        var queryFile = Path.Combine(_tempQueriesDirectory, "GetAvailableTools.kql");
        await File.WriteAllTextAsync(queryFile, "test query", TestContext.Current.CancellationToken);

        var mockReader = CreateMockDataReader(new[]
        {
            new McpToolEvent
            {
                EventTime = DateTime.UtcNow,
                EventType = McpToolEventType.Created,
                ServerVersion = "1.0.0",
                ToolId = "tool-1",
                ToolName = "TestTool",
                ToolArea = "TestArea",
                ReplacedByToolName = null,
                ReplacedByToolArea = null
            }
        });

        _kustoClient.ExecuteQueryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ClientRequestProperties>(),
            Arg.Any<CancellationToken>())
            .Returns(mockReader);

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act
        var result = await datastore.GetAvailableToolsAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("tool-1", result[0].ToolId);
        Assert.Equal("TestTool", result[0].ToolName);
        Assert.Equal("TestArea", result[0].ToolArea);
    }

    [Fact]
    public async Task GetAvailableToolsAsync_ReturnsUpdatedTools()
    {
        // Arrange
        var queryFile = Path.Combine(_tempQueriesDirectory, "GetAvailableTools.kql");
        await File.WriteAllTextAsync(queryFile, "test query", TestContext.Current.CancellationToken);

        var mockReader = CreateMockDataReader(new[]
        {
            new McpToolEvent
            {
                EventTime = DateTime.UtcNow,
                EventType = McpToolEventType.Updated,
                ServerVersion = "1.0.0",
                ToolId = "tool-1",
                ToolName = "OldTool",
                ToolArea = "OldArea",
                ReplacedByToolName = "NewTool",
                ReplacedByToolArea = "NewArea"
            }
        });

        _kustoClient.ExecuteQueryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ClientRequestProperties>(),
            Arg.Any<CancellationToken>())
            .Returns(mockReader);

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act
        var result = await datastore.GetAvailableToolsAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("tool-1", result[0].ToolId);
        Assert.Equal("NewTool", result[0].ToolName);
        Assert.Equal("NewArea", result[0].ToolArea);
    }

    [Fact]
    public async Task GetAvailableToolsAsync_ThrowsForUnsupportedEventType()
    {
        // Arrange
        var queryFile = Path.Combine(_tempQueriesDirectory, "GetAvailableTools.kql");
        await File.WriteAllTextAsync(queryFile, "test query", TestContext.Current.CancellationToken);

        var mockReader = CreateMockDataReader(new[]
        {
            new McpToolEvent
            {
                EventTime = DateTime.UtcNow,
                EventType = McpToolEventType.Deleted,
                ServerVersion = "1.0.0",
                ToolId = "tool-1",
                ToolName = "TestTool",
                ToolArea = "TestArea",
                ReplacedByToolName = null,
                ReplacedByToolArea = null
            }
        });

        _kustoClient.ExecuteQueryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ClientRequestProperties>(),
            Arg.Any<CancellationToken>())
            .Returns(mockReader);

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await datastore.GetAvailableToolsAsync(TestContext.Current.CancellationToken));
        Assert.Contains("unsupported event type", exception.Message);
    }

    [Fact]
    public async Task GetAvailableToolsAsync_ThrowsWhenToolIdIsEmpty()
    {
        // Arrange
        var queryFile = Path.Combine(_tempQueriesDirectory, "GetAvailableTools.kql");
        await File.WriteAllTextAsync(queryFile, "test query", TestContext.Current.CancellationToken);

        var mockReader = CreateMockDataReader(new[]
        {
            new McpToolEvent
            {
                EventTime = DateTime.UtcNow,
                EventType = McpToolEventType.Created,
                ServerVersion = "1.0.0",
                ToolId = "",
                ToolName = "TestTool",
                ToolArea = "TestArea",
                ReplacedByToolName = null,
                ReplacedByToolArea = null
            }
        });

        _kustoClient.ExecuteQueryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ClientRequestProperties>(),
            Arg.Any<CancellationToken>())
            .Returns(mockReader);

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await datastore.GetAvailableToolsAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Cannot have an event with no id", exception.Message);
    }

    [Fact]
    public async Task GetAvailableToolsAsync_ThrowsWhenToolNameIsEmpty()
    {
        // Arrange
        var queryFile = Path.Combine(_tempQueriesDirectory, "GetAvailableTools.kql");
        await File.WriteAllTextAsync(queryFile, "test query", TestContext.Current.CancellationToken);

        var mockReader = CreateMockDataReader(new[]
        {
            new McpToolEvent
            {
                EventTime = DateTime.UtcNow,
                EventType = McpToolEventType.Created,
                ServerVersion = "1.0.0",
                ToolId = "tool-1",
                ToolName = "",
                ToolArea = "TestArea",
                ReplacedByToolName = null,
                ReplacedByToolArea = null
            }
        });

        _kustoClient.ExecuteQueryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ClientRequestProperties>(),
            Arg.Any<CancellationToken>())
            .Returns(mockReader);

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await datastore.GetAvailableToolsAsync(TestContext.Current.CancellationToken));
        Assert.Contains("without tool name and/or a tool area", exception.Message);
    }

    [Fact]
    public async Task GetAvailableToolsAsync_ThrowsWhenToolAreaIsEmpty()
    {
        // Arrange
        var queryFile = Path.Combine(_tempQueriesDirectory, "GetAvailableTools.kql");
        await File.WriteAllTextAsync(queryFile, "test query", TestContext.Current.CancellationToken);

        var mockReader = CreateMockDataReader(new[]
        {
            new McpToolEvent
            {
                EventTime = DateTime.UtcNow,
                EventType = McpToolEventType.Created,
                ServerVersion = "1.0.0",
                ToolId = "tool-1",
                ToolName = "TestTool",
                ToolArea = "",
                ReplacedByToolName = null,
                ReplacedByToolArea = null
            }
        });

        _kustoClient.ExecuteQueryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ClientRequestProperties>(),
            Arg.Any<CancellationToken>())
            .Returns(mockReader);

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await datastore.GetAvailableToolsAsync(TestContext.Current.CancellationToken));
        Assert.Contains("without tool name and/or a tool area", exception.Message);
    }

    [Fact]
    public async Task GetAvailableToolsAsync_ProcessesMultipleTools()
    {
        // Arrange
        var queryFile = Path.Combine(_tempQueriesDirectory, "GetAvailableTools.kql");
        await File.WriteAllTextAsync(queryFile, "test query", TestContext.Current.CancellationToken);

        var mockReader = CreateMockDataReader(new[]
        {
            new McpToolEvent
            {
                EventTime = DateTime.UtcNow,
                EventType = McpToolEventType.Created,
                ServerVersion = "1.0.0",
                ToolId = "tool-1",
                ToolName = "Tool1",
                ToolArea = "Area1",
                ReplacedByToolName = null,
                ReplacedByToolArea = null
            },
            new McpToolEvent
            {
                EventTime = DateTime.UtcNow,
                EventType = McpToolEventType.Updated,
                ServerVersion = "1.0.0",
                ToolId = "tool-2",
                ToolName = "OldTool2",
                ToolArea = "OldArea2",
                ReplacedByToolName = "NewTool2",
                ReplacedByToolArea = "NewArea2"
            }
        });

        _kustoClient.ExecuteQueryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ClientRequestProperties>(),
            Arg.Any<CancellationToken>())
            .Returns(mockReader);

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act
        var result = await datastore.GetAvailableToolsAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("tool-1", result[0].ToolId);
        Assert.Equal("Tool1", result[0].ToolName);
        Assert.Equal("tool-2", result[1].ToolId);
        Assert.Equal("NewTool2", result[1].ToolName);
    }

    [Fact]
    public async Task AddToolEventsAsync_IngestsDataSuccessfully()
    {
        // Arrange
        var toolEvents = new List<McpToolEvent>
        {
            new()
            {
                EventTime = DateTime.UtcNow,
                EventType = McpToolEventType.Created,
                ServerVersion = "1.0.0",
                ToolId = "tool-1",
                ToolName = "TestTool",
                ToolArea = "TestArea",
                ReplacedByToolName = null,
                ReplacedByToolArea = null
            }
        };

        var mockIngestionStatus = Substitute.For<IKustoIngestionResult>();
        var statusCollection = new List<IngestionStatus>
        {
            new()
            {
                IngestionSourceId = Guid.NewGuid(),
                Table = "McpToolEvents",
                Status = Status.Succeeded,
                Details = "Success"
            }
        };
        mockIngestionStatus.GetIngestionStatusCollection().Returns(statusCollection);

        _ingestClient.IngestFromStreamAsync(
            Arg.Any<Stream>(),
            Arg.Any<KustoIngestionProperties>())
            .Returns(mockIngestionStatus);

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act
        await datastore.AddToolEventsAsync(toolEvents, TestContext.Current.CancellationToken);

        // Assert
        await _ingestClient.Received(1).IngestFromStreamAsync(
            Arg.Any<Stream>(),
            Arg.Is<KustoIngestionProperties>(p =>
                p.DatabaseName == "TestDatabase" &&
                p.TableName == "McpToolEvents"));
    }

    [Fact]
    public async Task AddToolEventsAsync_LogsWarningWhenResultIsNull()
    {
        // Arrange
        var toolEvents = new List<McpToolEvent>
        {
            new()
            {
                EventTime = DateTime.UtcNow,
                EventType = McpToolEventType.Created,
                ServerVersion = "1.0.0",
                ToolId = "tool-1",
                ToolName = "TestTool",
                ToolArea = "TestArea",
                ReplacedByToolName = null,
                ReplacedByToolArea = null
            }
        };

        _ingestClient.IngestFromStreamAsync(
            Arg.Any<Stream>(),
            Arg.Any<KustoIngestionProperties>())
            .Returns((IKustoIngestionResult?)null);

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act
        await datastore.AddToolEventsAsync(toolEvents, TestContext.Current.CancellationToken);

        // Assert
        await _ingestClient.Received(1).IngestFromStreamAsync(
            Arg.Any<Stream>(),
            Arg.Any<KustoIngestionProperties>());
    }

    [Fact]
    public async Task AddToolEventsAsync_HandlesEmptyList()
    {
        // Arrange
        var toolEvents = new List<McpToolEvent>();

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act
        await datastore.AddToolEventsAsync(toolEvents, TestContext.Current.CancellationToken);

        // Assert
        await _ingestClient.Received(1).IngestFromStreamAsync(
            Arg.Any<Stream>(),
            Arg.Any<KustoIngestionProperties>());
    }

    [Fact]
    public async Task GetLatestToolEventsAsync_ThrowsWhenKqlFileNotFound()
    {
        // Arrange
        var nonExistentFile = Path.Combine(_tempQueriesDirectory, "NonExistent.kql");
        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await foreach (var _ in datastore.GetLatestToolEventsAsync(nonExistentFile, TestContext.Current.CancellationToken))
            {
                // Should not reach here
            }
        });
    }

    [Fact]
    public async Task GetLatestToolEventsAsync_ReadsEventsFromKusto()
    {
        // Arrange
        var queryFile = Path.Combine(_tempQueriesDirectory, "TestQuery.kql");
        await File.WriteAllTextAsync(queryFile, "test query", TestContext.Current.CancellationToken);

        var mockReader = CreateMockDataReader(new[]
        {
            new McpToolEvent
            {
                EventTime = DateTime.UtcNow,
                EventType = McpToolEventType.Created,
                ServerVersion = "1.0.0",
                ToolId = "tool-1",
                ToolName = "TestTool",
                ToolArea = "TestArea",
                ReplacedByToolName = null,
                ReplacedByToolArea = null
            }
        });

        _kustoClient.ExecuteQueryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ClientRequestProperties>(),
            Arg.Any<CancellationToken>())
            .Returns(mockReader);

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act
        var results = new List<McpToolEvent>();
        await foreach (var toolEvent in datastore.GetLatestToolEventsAsync(queryFile, TestContext.Current.CancellationToken))
        {
            results.Add(toolEvent);
        }

        // Assert
        Assert.Single(results);
        Assert.Equal("tool-1", results[0].ToolId);
        Assert.Equal("TestTool", results[0].ToolName);
    }

    [Fact]
    public async Task GetLatestToolEventsAsync_ThrowsOnInvalidEventType()
    {
        // Arrange
        var queryFile = Path.Combine(_tempQueriesDirectory, "TestQuery.kql");
        await File.WriteAllTextAsync(queryFile, "test query", TestContext.Current.CancellationToken);

        var mockReader = Substitute.For<IDataReader>();
        mockReader.Read().Returns(true, false);
        mockReader.GetOrdinal("EventTime").Returns(0);
        mockReader.GetOrdinal("EventType").Returns(1);
        mockReader.GetOrdinal("ServerVersion").Returns(2);
        mockReader.GetOrdinal("ToolId").Returns(3);
        mockReader.GetOrdinal("ToolName").Returns(4);
        mockReader.GetOrdinal("ToolArea").Returns(5);
        mockReader.GetOrdinal("ReplacedByToolName").Returns(6);
        mockReader.GetOrdinal("ReplacedByToolArea").Returns(7);

        mockReader.GetDateTime(0).Returns(DateTime.UtcNow);
        mockReader.GetString(1).Returns("InvalidEventType");
        mockReader.GetString(2).Returns("1.0.0");
        mockReader.GetString(3).Returns("tool-1");
        mockReader.GetString(4).Returns("TestTool");
        mockReader.GetString(5).Returns("TestArea");
        mockReader.IsDBNull(6).Returns(true);
        mockReader.IsDBNull(7).Returns(true);

        _kustoClient.ExecuteQueryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ClientRequestProperties>(),
            Arg.Any<CancellationToken>())
            .Returns(mockReader);

        var datastore = new AzureMcpKustoDatastore(_kustoClient, _ingestClient, _options, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in datastore.GetLatestToolEventsAsync(queryFile, TestContext.Current.CancellationToken))
            {
                // Should not reach here
            }
        });
    }

    private static IDataReader CreateMockDataReader(McpToolEvent[] events)
    {
        var reader = Substitute.For<IDataReader>();
        var currentIndex = -1;

        reader.Read().Returns(callInfo =>
        {
            currentIndex++;
            return currentIndex < events.Length;
        });

        reader.GetOrdinal("EventTime").Returns(0);
        reader.GetOrdinal("EventType").Returns(1);
        reader.GetOrdinal("ServerVersion").Returns(2);
        reader.GetOrdinal("ToolId").Returns(3);
        reader.GetOrdinal("ToolName").Returns(4);
        reader.GetOrdinal("ToolArea").Returns(5);
        reader.GetOrdinal("ReplacedByToolName").Returns(6);
        reader.GetOrdinal("ReplacedByToolArea").Returns(7);

        reader.GetDateTime(Arg.Is(0)).Returns(x => GetCurrentEventTime());
        reader.GetString(Arg.Is(1)).Returns(x => GetCurrentEventType());
        reader.GetString(Arg.Is(2)).Returns(x => GetCurrentServerVersion());
        reader.GetString(Arg.Is(3)).Returns(x => GetCurrentToolId());
        reader.GetString(Arg.Is(4)).Returns(x => GetCurrentToolName());
        reader.GetString(Arg.Is(5)).Returns(x => GetCurrentToolArea());
        
        reader.IsDBNull(Arg.Is(6)).Returns(x => IsReplacedByToolNameNull());
        reader.GetString(Arg.Is(6)).Returns(x => GetCurrentReplacedByToolName());
        
        reader.IsDBNull(Arg.Is(7)).Returns(x => IsReplacedByToolAreaNull());
        reader.GetString(Arg.Is(7)).Returns(x => GetCurrentReplacedByToolArea());

        return reader;

        DateTime GetCurrentEventTime() => events[Math.Max(0, currentIndex)].EventTime?.DateTime ?? DateTime.UtcNow;
        string GetCurrentEventType() => events[Math.Max(0, currentIndex)].EventType?.ToString() ?? string.Empty;
        string GetCurrentServerVersion() => events[Math.Max(0, currentIndex)].ServerVersion ?? string.Empty;
        string GetCurrentToolId() => events[Math.Max(0, currentIndex)].ToolId ?? string.Empty;
        string GetCurrentToolName() => events[Math.Max(0, currentIndex)].ToolName ?? string.Empty;
        string GetCurrentToolArea() => events[Math.Max(0, currentIndex)].ToolArea ?? string.Empty;
        bool IsReplacedByToolNameNull() => string.IsNullOrEmpty(events[Math.Max(0, currentIndex)].ReplacedByToolName);
        string GetCurrentReplacedByToolName() => events[Math.Max(0, currentIndex)].ReplacedByToolName ?? string.Empty;
        bool IsReplacedByToolAreaNull() => string.IsNullOrEmpty(events[Math.Max(0, currentIndex)].ReplacedByToolArea);
        string GetCurrentReplacedByToolArea() => events[Math.Max(0, currentIndex)].ReplacedByToolArea ?? string.Empty;
    }
}
