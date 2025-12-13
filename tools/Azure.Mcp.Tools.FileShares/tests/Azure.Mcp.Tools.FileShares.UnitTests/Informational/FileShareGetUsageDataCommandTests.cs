// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

/// <summary>
/// Unit tests for FileShareGetUsageDataCommand.
/// Tests command for retrieving file share usage statistics.
/// </summary>
public class FileShareGetUsageDataCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileSharesService _fileSharesService;
    private readonly ILogger<FileShareGetUsageDataCommand> _logger;
    private readonly FileShareGetUsageDataCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public FileShareGetUsageDataCommandTests()
    {
        _fileSharesService = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<FileShareGetUsageDataCommand>>();

        var collection = new ServiceCollection().AddSingleton(_fileSharesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    /// <summary>
    /// Tests that the command initializes correctly with expected properties.
    /// </summary>
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();

        Assert.Equal("getusagedata", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("usage", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tests that the command has correct metadata properties (read-only).
    /// </summary>
    [Fact]
    public void ToolMetadata_IsConfiguredAsReadOnly()
    {
        var metadata = _command.Metadata;

        Assert.True(metadata.ReadOnly, "GetUsageData should be read-only");
        Assert.False(metadata.Destructive, "GetUsageData should not be destructive");
        Assert.True(metadata.Idempotent, "GetUsageData should be idempotent");
    }

    /// <summary>
    /// Tests retrieving usage data for a location.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithValidLocation_ReturnsUsageData()
    {
        // Arrange
        var subscription = "sub123";
        var location = "eastus";
        var usageData = new { maxFileShares = 1000, currentFileShares = 150, quotaUtilization = 0.15 };

        _fileSharesService.GetFileShareUsageDataAsync(
            Arg.Is(subscription),
            Arg.Is(location),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(usageData));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--location", location]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    /// <summary>
    /// Tests usage data retrieval with tenant ID.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithTenantId_SucceedsCorrectly()
    {
        // Arrange
        var subscription = "sub123";
        var location = "westus";
        var tenantId = "tenant123";
        var usageData = new { maxFileShares = 1000, currentFileShares = 50, quotaUtilization = 0.05 };

        _fileSharesService.GetFileShareUsageDataAsync(
            Arg.Is(subscription),
            Arg.Is(location),
            Arg.Is(tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(usageData));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--location", location, "--tenant", tenantId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    /// <summary>
    /// Tests that service exceptions are properly handled.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var subscription = "sub123";
        var location = "eastus";
        var expectedError = "Location not supported";

        _fileSharesService.GetFileShareUsageDataAsync(
            Arg.Is(subscription),
            Arg.Is(location),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--location", location]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Status == HttpStatusCode.BadRequest || response.Status == HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// Tests command option validation for required parameters.
    /// </summary>
    [Theory]
    [InlineData("--subscription sub123 --location eastus", true)]
    [InlineData("--subscription sub123 --location eastus --tenant tenant123", true)]
    [InlineData("--subscription sub123", false)] // Missing location
    [InlineData("--location eastus", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        var usageData = new { maxFileShares = 1000, currentFileShares = 0 };

        _fileSharesService.GetFileShareUsageDataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(usageData));

        // Act & Assert
        if (shouldSucceed)
        {
            var parsedArgs = _commandDefinition.Parse(args);
            var response = await _command.ExecuteAsync(_context, parsedArgs, TestContext.Current.CancellationToken);
            Assert.NotNull(response);
        }
        else
        {
            var exception = Assert.Throws<Exception>(() => _commandDefinition.Parse(args));
            Assert.NotNull(exception);
        }
    }

    /// <summary>
    /// Tests multiple valid locations return usage data.
    /// </summary>
    [Theory]
    [InlineData("eastus")]
    [InlineData("westus")]
    [InlineData("westeurope")]
    [InlineData("southeastasia")]
    public async Task ExecuteAsync_WithVariousLocations_ReturnsUsageData(string location)
    {
        // Arrange
        var subscription = "sub123";
        var usageData = new { maxFileShares = 1000, currentFileShares = 100 };

        _fileSharesService.GetFileShareUsageDataAsync(
            Arg.Is(subscription),
            Arg.Is(location),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(usageData));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--location", location]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}
