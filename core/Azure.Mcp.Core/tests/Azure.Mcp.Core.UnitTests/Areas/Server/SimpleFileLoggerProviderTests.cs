// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server;

public class SimpleFileLoggerProviderTests : IDisposable
{
    private readonly string _testLogFilePath;
    private readonly List<string> _createdFiles = [];

    public SimpleFileLoggerProviderTests()
    {
        _testLogFilePath = Path.Combine(Path.GetTempPath(), $"test_log_{Guid.NewGuid()}.log");
    }

    public void Dispose()
    {
        // Clean up test files
        foreach (var file in _createdFiles)
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        if (File.Exists(_testLogFilePath))
        {
            try
            {
                File.Delete(_testLogFilePath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void CreateLogger_ReturnsNonNullLogger()
    {
        // Arrange
        using var provider = new SimpleFileLoggerProvider(_testLogFilePath);

        // Act
        var logger = provider.CreateLogger("TestCategory");

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void Logger_IsEnabled_ReturnsTrueForNonNoneLevel()
    {
        // Arrange
        using var provider = new SimpleFileLoggerProvider(_testLogFilePath);
        var logger = provider.CreateLogger("TestCategory");

        // Act & Assert
        Assert.True(logger.IsEnabled(LogLevel.Trace));
        Assert.True(logger.IsEnabled(LogLevel.Debug));
        Assert.True(logger.IsEnabled(LogLevel.Information));
        Assert.True(logger.IsEnabled(LogLevel.Warning));
        Assert.True(logger.IsEnabled(LogLevel.Error));
        Assert.True(logger.IsEnabled(LogLevel.Critical));
        Assert.False(logger.IsEnabled(LogLevel.None));
    }

    [Fact]
    public void Logger_Log_WritesToFile()
    {
        // Arrange
        _createdFiles.Add(_testLogFilePath);
        using var provider = new SimpleFileLoggerProvider(_testLogFilePath);
        var logger = provider.CreateLogger("TestCategory");
        var testMessage = "Test log message";

        // Act
        logger.LogInformation(testMessage);

        // Assert
        Assert.True(File.Exists(_testLogFilePath));
        var content = File.ReadAllText(_testLogFilePath);
        Assert.Contains(testMessage, content);
        Assert.Contains("[Information]", content);
        Assert.Contains("[TestCategory]", content);
    }

    [Fact]
    public void Logger_Log_WithException_IncludesExceptionInLog()
    {
        // Arrange
        _createdFiles.Add(_testLogFilePath);
        using var provider = new SimpleFileLoggerProvider(_testLogFilePath);
        var logger = provider.CreateLogger("TestCategory");
        var testMessage = "Error occurred";
        var exception = new InvalidOperationException("Test exception");

        // Act
        logger.LogError(exception, testMessage);

        // Assert
        var content = File.ReadAllText(_testLogFilePath);
        Assert.Contains(testMessage, content);
        Assert.Contains("Test exception", content);
        Assert.Contains("InvalidOperationException", content);
    }

    [Fact]
    public void Logger_Log_MultipleMessages_AppendsToFile()
    {
        // Arrange
        _createdFiles.Add(_testLogFilePath);
        using var provider = new SimpleFileLoggerProvider(_testLogFilePath);
        var logger = provider.CreateLogger("TestCategory");
        var message1 = "First message";
        var message2 = "Second message";

        // Act
        logger.LogInformation(message1);
        logger.LogWarning(message2);

        // Assert
        var content = File.ReadAllText(_testLogFilePath);
        Assert.Contains(message1, content);
        Assert.Contains(message2, content);
        Assert.Contains("[Information]", content);
        Assert.Contains("[Warning]", content);
    }

    [Fact]
    public void Logger_Log_IncludesTimestamp()
    {
        // Arrange
        _createdFiles.Add(_testLogFilePath);
        using var provider = new SimpleFileLoggerProvider(_testLogFilePath);
        var logger = provider.CreateLogger("TestCategory");
        var testMessage = "Timestamped message";

        // Act
        logger.LogInformation(testMessage);

        // Assert
        var content = File.ReadAllText(_testLogFilePath);
        // Check for timestamp pattern [yyyy-MM-dd HH:mm:ss.fff]
        Assert.Matches(@"\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\]", content);
    }

    [Fact]
    public void Logger_Log_DifferentCategories_WritesToSameFile()
    {
        // Arrange
        _createdFiles.Add(_testLogFilePath);
        using var provider = new SimpleFileLoggerProvider(_testLogFilePath);
        var logger1 = provider.CreateLogger("Category1");
        var logger2 = provider.CreateLogger("Category2");

        // Act
        logger1.LogInformation("Message from category 1");
        logger2.LogWarning("Message from category 2");

        // Assert
        var content = File.ReadAllText(_testLogFilePath);
        Assert.Contains("[Category1]", content);
        Assert.Contains("[Category2]", content);
        Assert.Contains("Message from category 1", content);
        Assert.Contains("Message from category 2", content);
    }

    [Fact]
    public void Logger_Log_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var directoryPath = Path.Combine(Path.GetTempPath(), $"test_dir_{Guid.NewGuid()}");
        var filePath = Path.Combine(directoryPath, "test.log");
        _createdFiles.Add(filePath);

        using var provider = new SimpleFileLoggerProvider(filePath);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Test message");

        // Assert
        Assert.True(Directory.Exists(directoryPath));
        Assert.True(File.Exists(filePath));

        // Cleanup
        try
        {
            Directory.Delete(directoryPath, true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SimpleFileLoggerProvider(null!));
    }
}
