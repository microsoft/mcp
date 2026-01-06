// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

/// <summary>
/// Unit tests for FileShareGetLimitsCommand.
/// Tests command for retrieving file share limits by location.
/// </summary>
public class FileShareGetLimitsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileSharesService _fileSharesService;
    private readonly ILogger<FileShareGetLimitsCommand> _logger;
    private readonly FileShareGetLimitsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public FileShareGetLimitsCommandTests()
    {
        _fileSharesService = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<FileShareGetLimitsCommand>>();

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

        Assert.Equal("getlimits", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("limit", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tests that the command has correct metadata properties (read-only).
    /// </summary>
    [Fact]
    public void ToolMetadata_IsConfiguredAsReadOnly()
    {
        var metadata = _command.Metadata;

        Assert.True(metadata.ReadOnly, "GetLimits should be read-only");
        Assert.False(metadata.Destructive, "GetLimits should not be destructive");
        Assert.True(metadata.Idempotent, "GetLimits should be idempotent");
    }

    /// <summary>
    /// Tests retrieving limits for a specific location.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithValidLocation_ReturnsLimits()
    {
        // Arrange
        var subscription = "sub123";
        var location = "eastus";
        var limits = new FileShareLimitsSchema { MaxFileShares = 10000, MaxFileShareSize = 1099511627776 };

        _fileSharesService.GetFileShareLimitsAsync(
            Arg.Is(subscription),
            Arg.Is(location),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(limits));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--location", location]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    /// <summary>
    /// Tests limits retrieval with tenant ID.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithTenantId_SucceedsCorrectly()
    {
        // Arrange
        var subscription = "sub123";
        var location = "westus";
        var tenantId = "tenant123";
        var limits = new FileShareLimitsSchema { MaxFileShares = 5000 };

        _fileSharesService.GetFileShareLimitsAsync(
            Arg.Is(subscription),
            Arg.Is(location),
            Arg.Is(tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(limits));

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
        var location = "invalidlocation";
        var expectedError = "Invalid location";

        _fileSharesService.GetFileShareLimitsAsync(
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
        var limits = new FileShareLimitsSchema { MaxFileShares = 10000 };

        _fileSharesService.GetFileShareLimitsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(limits));

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
    /// Tests multiple valid locations return limit information.
    /// </summary>
    [Theory]
    [InlineData("eastus")]
    [InlineData("westus")]
    [InlineData("westeurope")]
    [InlineData("southeastasia")]
    public async Task ExecuteAsync_WithVariousLocations_ReturnsLimits(string location)
    {
        // Arrange
        var subscription = "sub123";
        var limits = new FileShareLimitsSchema { MaxFileShares = 10000 };

        _fileSharesService.GetFileShareLimitsAsync(
            Arg.Is(subscription),
            Arg.Is(location),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(limits));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--location", location]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}
