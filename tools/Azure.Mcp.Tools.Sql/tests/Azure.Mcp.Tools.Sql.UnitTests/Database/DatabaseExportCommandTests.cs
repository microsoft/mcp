// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Database;

public class DatabaseExportCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _sqlService;
    private readonly ILogger<DatabaseExportCommand> _logger;
    private readonly DatabaseExportCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DatabaseExportCommandTests()
    {
        _sqlService = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<DatabaseExportCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_sqlService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("export", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Export an Azure SQL Database to a BACPAC file", command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsSuccessResult()
    {
        // Arrange
        var mockResult = new SqlDatabaseExportResult(
            OperationId: "operation-123",
            RequestId: "request-456",
            Status: "InProgress",
            QueuedTime: DateTimeOffset.UtcNow,
            LastModifiedTime: DateTimeOffset.UtcNow,
            ServerName: "test-server",
            DatabaseName: "test-db",
            StorageUri: "https://storage.blob.core.windows.net/container/export.bacpac",
            Message: null
        );

        _sqlService.ExportDatabaseAsync(
            Arg.Is("test-server"),
            Arg.Is("test-db"),
            Arg.Is("test-rg"),
            Arg.Is("test-subscription"),
            Arg.Is("https://storage.blob.core.windows.net/container/export.bacpac"),
            Arg.Is("storagekey123"),
            Arg.Is("StorageAccessKey"),
            Arg.Is("admin"),
            Arg.Is("password123"),
            Arg.Is("SQL"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResult);

        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--server", "test-server",
            "--database", "test-db",
            "--storage-uri", "https://storage.blob.core.windows.net/container/export.bacpac",
            "--storage-key", "storagekey123",
            "--storage-key-type", "StorageAccessKey",
            "--admin-user", "admin",
            "--admin-password", "password123",
            "--auth-type", "SQL"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        // Debug: Add detailed error info
        if (response.Status != 200)
        {
            throw new Exception($"Expected 200 but got {response.Status}. Message: {response.Message}");
        }
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithServiceException_ReturnsErrorResult()
    {
        // Arrange
        _sqlService.ExportDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Database not found"));

        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--server", "test-server",
            "--database", "test-db",
            "--storage-uri", "https://storage.blob.core.windows.net/container/export.bacpac",
            "--storage-key", "storagekey123",
            "--storage-key-type", "StorageAccessKey",
            "--admin-user", "admin",
            "--admin-password", "password123",
            "--auth-type", "SQL"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Database not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingSubscription_ReturnsValidationError()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--resource-group", "test-rg",
            "--server", "test-server",
            "--database", "test-db",
            "--storage-uri", "https://storage.blob.core.windows.net/container/export.bacpac",
            "--storage-key", "storagekey123",
            "--storage-key-type", "StorageAccessKey",
            "--admin-user", "admin",
            "--admin-password", "password123",
            "--auth-type", "SQL"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("subscription", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingStorageUri_ReturnsValidationError()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--server", "test-server",
            "--database", "test-db",
            "--storage-key", "storagekey123",
            "--storage-key-type", "StorageAccessKey",
            "--admin-user", "admin",
            "--admin-password", "password123",
            "--auth-type", "SQL"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Missing Required options: --storage-uri", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidStorageUri_ReturnsValidationError()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg", 
            "--server", "test-server",
            "--database", "test-db",
            "--storage-uri", "invalid-uri",
            "--storage-key", "storagekey123",
            "--storage-key-type", "StorageAccessKey",
            "--admin-user", "admin",
            "--admin-password", "password123",
            "--auth-type", "SQL"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Storage URI must be a valid absolute URI", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidStorageKeyType_ReturnsValidationError()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--server", "test-server",
            "--database", "test-db",
            "--storage-uri", "https://storage.blob.core.windows.net/container/export.bacpac",
            "--storage-key", "storagekey123",
            "--storage-key-type", "InvalidType",
            "--admin-user", "admin",
            "--admin-password", "password123",
            "--auth-type", "SQL"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Invalid storage key type", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidAuthenticationType_ReturnsValidationError()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--server", "test-server",
            "--database", "test-db",
            "--storage-uri", "https://storage.blob.core.windows.net/container/export.bacpac",
            "--storage-key", "storagekey123",
            "--storage-key-type", "StorageAccessKey",
            "--admin-user", "admin",
            "--admin-password", "password123",
            "--auth-type", "InvalidAuth"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Invalid authentication type", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var mockResult = new SqlDatabaseExportResult(
            OperationId: "operation-123",
            RequestId: "request-456",
            Status: "InProgress",
            QueuedTime: DateTimeOffset.UtcNow,
            LastModifiedTime: DateTimeOffset.UtcNow,
            ServerName: "test-server",
            DatabaseName: "test-db",
            StorageUri: "https://storage.blob.core.windows.net/container/export.bacpac",
            Message: null
        );

        _sqlService.ExportDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResult);

        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--server", "test-server",
            "--database", "test-db",
            "--storage-uri", "https://storage.blob.core.windows.net/container/export.bacpac",
            "--storage-key", "storagekey123",
            "--storage-key-type", "StorageAccessKey",
            "--admin-user", "admin",
            "--admin-password", "password123",
            "--auth-type", "SQL"
        ]);

        // Act
        await _command.ExecuteAsync(_context, args);

        // Assert
        await _sqlService.Received(1).ExportDatabaseAsync(
            "test-server",
            "test-db",
            "test-rg",
            "test-subscription",
            "https://storage.blob.core.windows.net/container/export.bacpac",
            "storagekey123",
            "StorageAccessKey",
            "admin",
            "password123",
            "SQL",
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("StorageAccessKey")]
    [InlineData("SharedAccessKey")]
    [InlineData("ManagedIdentity")]
    public async Task ExecuteAsync_WithValidStorageKeyTypes_ExecutesSuccessfully(string storageKeyType)
    {
        // Arrange
        var mockResult = new SqlDatabaseExportResult(
            OperationId: "operation-123",
            RequestId: "request-456",
            Status: "InProgress",
            QueuedTime: DateTimeOffset.UtcNow,
            LastModifiedTime: DateTimeOffset.UtcNow,
            ServerName: "test-server",
            DatabaseName: "test-db",
            StorageUri: "https://storage.blob.core.windows.net/container/export.bacpac",
            Message: null
        );

        _sqlService.ExportDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResult);

        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--server", "test-server",
            "--database", "test-db",
            "--storage-uri", "https://storage.blob.core.windows.net/container/export.bacpac",
            "--storage-key", "storagekey123",
            "--storage-key-type", storageKeyType,
            "--admin-user", "admin",
            "--admin-password", "password123",
            "--auth-type", "SQL"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
    }

    [Theory]
    [InlineData("SQL")]
    [InlineData("ADPassword")]
    [InlineData("ManagedIdentity")]
    public async Task ExecuteAsync_WithValidAuthTypes_ExecutesSuccessfully(string authType)
    {
        // Arrange
        var mockResult = new SqlDatabaseExportResult(
            OperationId: "operation-123",
            RequestId: "request-456",
            Status: "InProgress",
            QueuedTime: DateTimeOffset.UtcNow,
            LastModifiedTime: DateTimeOffset.UtcNow,
            ServerName: "test-server",
            DatabaseName: "test-db",
            StorageUri: "https://storage.blob.core.windows.net/container/export.bacpac",
            Message: null
        );

        _sqlService.ExportDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResult);

        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--server", "test-server",
            "--database", "test-db",
            "--storage-uri", "https://storage.blob.core.windows.net/container/export.bacpac",
            "--storage-key", "storagekey123",
            "--storage-key-type", "StorageAccessKey",
            "--admin-user", "admin",
            "--admin-password", "password123",
            "--auth-type", authType
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingAdminUser_ReturnsValidationError()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--server", "test-server",
            "--database", "test-db",
            "--storage-uri", "https://storage.blob.core.windows.net/container/export.bacpac",
            "--storage-key", "storagekey123",
            "--storage-key-type", "StorageAccessKey",
            "--admin-password", "password123",
            "--auth-type", "SQL"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Missing Required options: --admin-user", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingAdminPassword_ReturnsValidationError()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--server", "test-server",
            "--database", "test-db",
            "--storage-uri", "https://storage.blob.core.windows.net/container/export.bacpac",
            "--storage-key", "storagekey123",
            "--storage-key-type", "StorageAccessKey",
            "--admin-user", "admin",
            "--auth-type", "SQL"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Missing Required options: --admin-password", response.Message);
    }
}