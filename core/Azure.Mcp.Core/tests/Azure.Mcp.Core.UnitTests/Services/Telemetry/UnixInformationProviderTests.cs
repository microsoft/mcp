// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Telemetry;

public class UnixInformationProviderTests
{
    private static readonly DirectoryInfo TestStorageDirectory = new DirectoryInfo(Path.DirectorySeparatorChar + Path.Join("test", "storage"));
    private static readonly string TestStoragePath = TestStorageDirectory.ToString();
    private static readonly DirectoryInfo ExpectedCacheDirectory = new DirectoryInfo(Path.Join(TestStorageDirectory.ToString(), "Microsoft", "DeveloperTools"));
    private static readonly string ExpectedCachePath = ExpectedCacheDirectory.ToString();

    private readonly ILogger<UnixMachineInformationProvider> _logger;
    private readonly TestUnixInformationProvider _provider;

    public UnixInformationProviderTests()
    {
        _logger = Substitute.For<ILogger<UnixMachineInformationProvider>>();
        _provider = new TestUnixInformationProvider(_logger, TestStorageDirectory.ToString());
    }

    [Fact]
    public async Task GetOrCreateDeviceId_WhenStoragePathThrows_ReturnsNull()
    {
        // Arrange
        var provider = new TestUnixInformationProvider(_logger, storagePath: TestStorageDirectory.ToString(), throwOnGetStoragePath: true);

        // Act
        var result = await provider.GetOrCreateDeviceId();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrCreateDeviceId_WhenExistingDeviceIdExists_ReturnsExistingValue()
    {
        // Arrange
        const string existingDeviceId = "existing-device-id";

        var provider = new NoOpUnixInformationProvider(existingDeviceId, true);

        // Act
        var result = await provider.GetOrCreateDeviceId();

        // Assert
        Assert.Equal(existingDeviceId, result);

        Assert.Equal(ExpectedCachePath, provider.ReadDirectoryPath);

        // Should not have written anything to disk if device id exists.
        Assert.Null(provider.WriteDirectoryPath);
        Assert.Null(provider.WriteFileName);
    }

    [Fact]
    public async Task GetOrCreateDeviceId_WhenNoExistingDeviceId_CreatesNewDeviceId()
    {
        // Arrange
        var provider = new NoOpUnixInformationProvider(null, true);

        // Act
        var result = await provider.GetOrCreateDeviceId();

        // Assert
        Assert.NotNull(result);

        string? read = provider.ReadDirectoryPath != null ? new DirectoryInfo(provider.ReadDirectoryPath).ToString() : null;
        string? write = provider.WriteDirectoryPath != null ? new DirectoryInfo(provider.WriteDirectoryPath).ToString() : null;
        Assert.Equal(ExpectedCacheDirectory.ToString(), read);
        Assert.Equal(ExpectedCacheDirectory.ToString(), write);

        Assert.NotNull(provider.WriteValue);
    }

    [Fact]
    public async Task GetOrCreateDeviceId_WhenWriteValueToDiskFails_ReturnsNull()
    {
        // Arrange
        var provider = new NoOpUnixInformationProvider(null, false);

        // Act
        var result = await provider.GetOrCreateDeviceId();

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task WriteValueToDisk_WhenValueIsNullOrWhitespace_ReturnsFalse(string? value)
    {
        // Act
        var result = await _provider.WriteValueToDisk("/test/path", "filename", value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ReadValueFromDisk_WhenFileDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _provider.ReadValueFromDisk("/nonexistent/path", "nonexistent.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Constructor_WithValidLogger_DoesNotThrow()
    {
        // Act & Assert
        var exception = Record.Exception(() => new TestUnixInformationProvider(_logger, TestStoragePath));
        Assert.Null(exception);
    }

    [Fact]
    public void GetStoragePath_WhenImplementationThrows_PropagatesException()
    {
        // Arrange
        var provider = new TestUnixInformationProvider(_logger, TestStoragePath, throwOnGetStoragePath: true);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => provider.GetTestStoragePath());
    }

    private class NoOpLogger : ILogger<NoOpUnixInformationProvider>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }
    }

    private class NoOpUnixInformationProvider(string? readStorageResult, bool writeValueResult)
        : UnixMachineInformationProvider(new NoOpLogger())
    {
        private readonly string? _readStorageResult = readStorageResult;
        private readonly bool _writeValueResult = writeValueResult;

        public override string GetStoragePath() => TestStoragePath;

        public string? ReadDirectoryPath { get; private set; }
        public string? ReadFileName { get; private set; }

        public string? WriteDirectoryPath { get; private set; }
        public string? WriteFileName { get; private set; }
        public string? WriteValue { get; private set; }

        public override Task<string?> ReadValueFromDisk(string directoryPath, string fileName)
        {
            ReadDirectoryPath = directoryPath;
            ReadFileName = fileName;
            return Task.FromResult(_readStorageResult);
        }

        public override Task<bool> WriteValueToDisk(string directoryPath, string fileName, string? value)
        {
            WriteDirectoryPath = directoryPath;
            WriteFileName = fileName;
            WriteValue = value;

            return Task.FromResult(_writeValueResult);
        }
    }

    private class TestUnixInformationProvider(ILogger<UnixMachineInformationProvider> logger,
        string? storagePath, bool throwOnGetStoragePath = false, string? deviceId = null)
        : UnixMachineInformationProvider(logger)
    {
        private readonly string? _storagePath = storagePath;
        private readonly string? _deviceId = deviceId;
        private readonly bool _throwOnGetStoragePath = throwOnGetStoragePath;

        public string GetTestStoragePath() => GetStoragePath();

        public override Task<string?> GetOrCreateDeviceId()
        {
            if (string.IsNullOrEmpty(_deviceId))
            {
                return base.GetOrCreateDeviceId();
            }
            else
            {
                return Task.FromResult<string?>(_deviceId);
            }
        }

        public override string GetStoragePath()
        {
            if (_throwOnGetStoragePath)
            {
                throw new InvalidOperationException("No storage path available");
            }
            return _storagePath ?? throw new InvalidOperationException("Storage path not set");
        }
    }
}
