// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.UnitTests.FileShare;

/// <summary>
/// Unit tests for FileShareListCommand.
/// Tests command initialization, option binding, execution, and error handling.
/// </summary>
public class FileShareListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileSharesService _fileSharesService;
    private readonly ILogger<FileShareListCommand> _logger;
    private readonly FileShareListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public FileShareListCommandTests()
    {
        _fileSharesService = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<FileShareListCommand>>();

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

        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("file share", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tests that the command has correct metadata properties.
    /// </summary>
    [Fact]
    public void ToolMetadata_IsConfiguredCorrectly()
    {
        var metadata = _command.Metadata;

        Assert.False(metadata.Destructive, "List command should not be destructive");
        Assert.True(metadata.ReadOnly, "List command should be read-only");
        Assert.True(metadata.Idempotent, "List command should be idempotent");
    }

    /// <summary>
    /// Tests listing file shares by subscription only.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithSubscriptionOnly_ReturnsAllFileShares()
    {
        // Arrange
        var subscription = "sub123";
        var expectedFileShares = new List<FileShareInfo>
        {
            new("fileshare1", "eastus", "Succeeded", DateTimeOffset.UtcNow, null, null, new() { { "env", "test" } }),
            new("fileshare2", "westus", "Succeeded", DateTimeOffset.UtcNow, null, null, new() { { "env", "prod" } })
        };

        _fileSharesService.ListFileSharesAsync(
            Arg.Is(subscription),
            Arg.Is<string?>(rg => rg == null),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedFileShares));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FileSharesJsonContext.Default.FileShareListCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.FileShares);
        Assert.Equal(expectedFileShares.Count, result.FileShares.Count);
    }

    /// <summary>
    /// Tests listing file shares filtered by resource group.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithResourceGroup_ReturnsFilteredFileShares()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "test-rg";
        var expectedFileShares = new List<FileShareInfo>
        {
            new("fileshare1", "eastus", "Succeeded", DateTimeOffset.UtcNow, null, null, new())
        };

        _fileSharesService.ListFileSharesAsync(
            Arg.Is(subscription),
            Arg.Is(resourceGroup),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedFileShares));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--resource-group", resourceGroup]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FileSharesJsonContext.Default.FileShareListCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.FileShares);
    }

    /// <summary>
    /// Tests that an empty list is returned when no file shares exist.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoFileShares()
    {
        // Arrange
        var subscription = "sub123";

        _fileSharesService.ListFileSharesAsync(
            Arg.Is(subscription),
            Arg.Is<string?>(rg => rg == null),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<FileShareInfo>()));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FileSharesJsonContext.Default.FileShareListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.FileShares);
    }

    /// <summary>
    /// Tests that service exceptions are properly handled.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var subscription = "sub123";
        var expectedError = "Service unavailable";

        _fileSharesService.ListFileSharesAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    /// <summary>
    /// Tests command option validation for required parameters.
    /// </summary>
    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --resource-group test-rg", true)]
    [InlineData("", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        var fileShares = new List<FileShareInfo>();

        _fileSharesService.ListFileSharesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(fileShares));

        // Act & Assert
        if (shouldSucceed)
        {
            var parsedArgs = _commandDefinition.Parse(args);
            var response = await _command.ExecuteAsync(_context, parsedArgs, TestContext.Current.CancellationToken);

            Assert.NotNull(response);
            Assert.True(response.Status == HttpStatusCode.OK || response.Status == HttpStatusCode.BadRequest);
        }
        else
        {
            // Parse should fail for invalid arguments
            var exception = Assert.Throws<Exception>(() => _commandDefinition.Parse(args));
            Assert.NotNull(exception);
        }
    }

    /// <summary>
    /// Tests command with tenant ID parameter.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithTenantId_SucceedsCorrectly()
    {
        // Arrange
        var subscription = "sub123";
        var tenantId = "tenant123";
        var fileShares = new List<FileShareInfo>();

        _fileSharesService.ListFileSharesAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Is(tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(fileShares));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--tenant", tenantId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}
