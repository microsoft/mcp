// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Telemetry;
using OpenTelemetry.Logs;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Telemetry;

public class TelemetryLogRecordEraserTests
{
    private readonly TelemetryLogRecordEraser _processor;

    public TelemetryLogRecordEraserTests()
    {
        _processor = new TelemetryLogRecordEraser();
    }

    [Fact]
    public void OnEnd_ClearsAttributes_WhenLogRecordHasAttributes()
    {
        // Arrange
        var logRecord = CreateLogRecord();
        var originalAttributes = new List<KeyValuePair<string, object?>>
        {
            new("key1", "value1"),
            new("key2", 123),
            new("sensitive", "secret-data")
        };
        logRecord.Attributes = originalAttributes;

        // Act
        _processor.OnEnd(logRecord);

        // Assert
        Assert.NotNull(logRecord.Attributes);
        Assert.Empty(logRecord.Attributes);
    }

    [Fact]
    public void OnEnd_ClearsBody_WhenLogRecordHasBody()
    {
        // Arrange
        var logRecord = CreateLogRecord();
        logRecord.Body = "This is sensitive log body content";

        // Act
        _processor.OnEnd(logRecord);

        // Assert
        Assert.Equal(string.Empty, logRecord.Body);
    }

    [Fact]
    public void OnEnd_ClearsFormattedMessage_WhenLogRecordHasFormattedMessage()
    {
        // Arrange
        var logRecord = CreateLogRecord();
        logRecord.FormattedMessage = "User {userId} performed action {action}";

        // Act
        _processor.OnEnd(logRecord);

        // Assert
        Assert.Equal(string.Empty, logRecord.FormattedMessage);
    }

    [Fact]
    public void OnEnd_ClearsAllFields_WhenLogRecordHasAllData()
    {
        // Arrange
        var logRecord = CreateLogRecord();
        logRecord.Attributes = new List<KeyValuePair<string, object?>>
        {
            new("userId", "12345"),
            new("action", "login")
        };
        logRecord.Body = "User login attempt";
        logRecord.FormattedMessage = "User 12345 attempted login";

        // Act
        _processor.OnEnd(logRecord);

        // Assert
        Assert.NotNull(logRecord.Attributes);
        Assert.Empty(logRecord.Attributes);
        Assert.Equal(string.Empty, logRecord.Body);
        Assert.Equal(string.Empty, logRecord.FormattedMessage);
    }

    [Fact]
    public async Task OnEnd_HandlesNullAttributes_Gracefully()
    {
        // Arrange
        var logRecord = CreateLogRecord();
        logRecord.Attributes = null;

        // Act
        var exception = await Record.ExceptionAsync(() => Task.Run(
            () => _processor.OnEnd(logRecord), TestContext.Current.CancellationToken));

        // Assert
        Assert.Null(exception);
        Assert.NotNull(logRecord.Attributes);
        Assert.Empty(logRecord.Attributes);
    }

    [Fact]
    public void OnEnd_HandlesEmptyAttributes_Correctly()
    {
        // Arrange
        var logRecord = CreateLogRecord();
        logRecord.Attributes = new List<KeyValuePair<string, object?>>();

        // Act
        _processor.OnEnd(logRecord);

        // Assert
        Assert.NotNull(logRecord.Attributes);
        Assert.Empty(logRecord.Attributes);
    }

    [Fact]
    public async Task OnEnd_HandlesNullBody_Gracefully()
    {
        // Arrange
        var logRecord = CreateLogRecord();
        logRecord.Body = null;

        // Act
        var exception = await Record.ExceptionAsync(() => Task.Run(
            () => _processor.OnEnd(logRecord), TestContext.Current.CancellationToken));

        // Assert
        Assert.Null(exception);
        Assert.Equal(string.Empty, logRecord.Body);
    }

    [Fact]
    public async Task OnEnd_HandlesNullFormattedMessage_Gracefully()
    {
        // Arrange
        var logRecord = CreateLogRecord();
        logRecord.FormattedMessage = null;

        // Act
        var exception = await Record.ExceptionAsync(() => Task.Run(
            () => _processor.OnEnd(logRecord), TestContext.Current.CancellationToken));

        // Assert
        Assert.Null(exception);
        Assert.Equal(string.Empty, logRecord.FormattedMessage);
    }

    [Fact]
    public void OnEnd_PreservesOtherLogRecordProperties()
    {
        // Arrange
        var logRecord = CreateLogRecord();
        var originalTimestamp = logRecord.Timestamp;
        var originalLogLevel = logRecord.LogLevel;
        var originalCategoryName = logRecord.CategoryName;

        // Set data that should be cleared
        logRecord.Attributes = new List<KeyValuePair<string, object?>> { new("test", "value") };
        logRecord.Body = "test body";
        logRecord.FormattedMessage = "test message";

        // Act
        _processor.OnEnd(logRecord);

        // Assert - Other properties should remain unchanged
        Assert.Equal(originalTimestamp, logRecord.Timestamp);
        Assert.Equal(originalLogLevel, logRecord.LogLevel);
        Assert.Equal(originalCategoryName, logRecord.CategoryName);
    }

    [Fact]
    public void OnEnd_MultipleCallsOnSameRecord_DoesNotThrow()
    {
        // Arrange
        var logRecord = CreateLogRecord();
        logRecord.Attributes = new List<KeyValuePair<string, object?>> { new("test", "value") };
        logRecord.Body = "test body";
        logRecord.FormattedMessage = "test message";

        // Act
        var exception = Record.Exception(() =>
        {
            _processor.OnEnd(logRecord);
            _processor.OnEnd(logRecord);
            _processor.OnEnd(logRecord);
        });

        // Assert
        Assert.Null(exception);
        Assert.Empty(logRecord.Attributes);
        Assert.Equal(string.Empty, logRecord.Body);
        Assert.Equal(string.Empty, logRecord.FormattedMessage);
    }

    [Theory]
    [InlineData("sensitive-data")]
    [InlineData("")]
    [InlineData(null)]
    public void OnEnd_ClearsBody_ForVariousBodyValues(string? bodyValue)
    {
        // Arrange
        var logRecord = CreateLogRecord();
        logRecord.Body = bodyValue;

        // Act
        _processor.OnEnd(logRecord);

        // Assert
        Assert.Equal(string.Empty, logRecord.Body);
    }

    [Theory]
    [InlineData("User {id} logged in")]
    [InlineData("")]
    [InlineData(null)]
    public void OnEnd_ClearsFormattedMessage_ForVariousMessageValues(string? messageValue)
    {
        // Arrange
        var logRecord = CreateLogRecord();
        logRecord.FormattedMessage = messageValue;

        // Act
        _processor.OnEnd(logRecord);

        // Assert
        Assert.Equal(string.Empty, logRecord.FormattedMessage);
    }

    /// <summary>
    /// Helper method to create a LogRecord.
    /// </summary>
    private static LogRecord CreateLogRecord()
    {
        var record = Activator.CreateInstance(typeof(LogRecord), true);

        Assert.NotNull(record);

        return (LogRecord)record;
    }
}
