// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem.AutoimportJob;

public class AutoimportJobCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IManagedLustreService _managedLustreService;
    private readonly ILogger<AutoimportJobCreateCommand> _logger;
    private readonly AutoimportJobCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _subscription = "sub123";
    private readonly string _resourceGroup = "rg1";
    private readonly string _fileSystemName = "fs1";

    public AutoimportJobCreateCommandTests()
    {
        _managedLustreService = Substitute.For<IManagedLustreService>();
        _logger = Substitute.For<ILogger<AutoimportJobCreateCommand>>();

        var services = new ServiceCollection().AddSingleton(_managedLustreService);
        _serviceProvider = services.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var cmd = _command.GetCommand();
        Assert.Equal("create", cmd.Name);
        Assert.False(string.IsNullOrWhiteSpace(cmd.Description));
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_WithRequiredParameters()
    {
        // Arrange
        _managedLustreService.CreateAutoimportJobAsync(
            Arg.Is(_subscription),
            Arg.Is(_resourceGroup),
            Arg.Is(_fileSystemName),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("autoimport-20250107120000");

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _managedLustreService.Received(1).CreateAutoimportJobAsync(
            _subscription,
            _resourceGroup,
            _fileSystemName,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_WithAllOptionalParameters()
    {
        // Arrange
        var jobName = "my-custom-job";
        var conflictMode = "OverwriteIfDirty";
        var prefixes = new List<string> { "/data", "/logs" };
        var adminStatus = "Enable";
        var enableDeletions = true;
        var maxErrors = 100L;

        _managedLustreService.CreateAutoimportJobAsync(
            Arg.Is(_subscription),
            Arg.Is(_resourceGroup),
            Arg.Is(_fileSystemName),
            Arg.Is(jobName),
            Arg.Is(conflictMode),
            Arg.Is<List<string>>(x => x.Count == 2 && x[0] == "/data" && x[1] == "/logs"),
            Arg.Is(adminStatus),
            Arg.Is(enableDeletions),
            Arg.Is(maxErrors),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("my-custom-job");

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--job-name", jobName,
            "--conflict-resolution-mode", conflictMode,
            "--autoimport-prefixes", "/data", "/logs",
            "--admin-status", adminStatus,
            "--enable-deletions", "true",
            "--maximum-errors", "100"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _managedLustreService.Received(1).CreateAutoimportJobAsync(
            _subscription,
            _resourceGroup,
            _fileSystemName,
            jobName,
            conflictMode,
            Arg.Is<List<string>>(x => x.Count == 2 && x[0] == "/data" && x[1] == "/logs"),
            adminStatus,
            enableDeletions,
            maxErrors,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Fail")]
    [InlineData("Skip")]
    [InlineData("OverwriteIfDirty")]
    [InlineData("OverwriteAlways")]
    public async Task ExecuteAsync_Succeeds_WithDifferentConflictResolutionModes(string conflictMode)
    {
        // Arrange
        _managedLustreService.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Is(conflictMode),
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("autoimport-job");

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--conflict-resolution-mode", conflictMode
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _managedLustreService.Received(1).CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            conflictMode,
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Enable")]
    [InlineData("Disable")]
    public async Task ExecuteAsync_Succeeds_WithDifferentAdminStatus(string adminStatus)
    {
        // Arrange
        _managedLustreService.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Is(adminStatus),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("autoimport-job");

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--admin-status", adminStatus
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _managedLustreService.Received(1).CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            adminStatus,
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_WithMultipleAutoimportPrefixes()
    {
        // Arrange
        var prefixes = new List<string> { "/prefix1", "/prefix2", "/prefix3" };

        _managedLustreService.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is<List<string>>(x => x.Count == 3 && x[0] == "/prefix1" && x[1] == "/prefix2" && x[2] == "/prefix3"),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("autoimport-job");

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--autoimport-prefixes", "/prefix1", "/prefix2", "/prefix3"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _managedLustreService.Received(1).CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is<List<string>>(x => x.Count == 3),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_Succeeds_WithEnableDeletions(bool enableDeletions)
    {
        // Arrange
        _managedLustreService.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            Arg.Is(enableDeletions),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("autoimport-job");

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--enable-deletions", enableDeletions.ToString().ToLower()
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _managedLustreService.Received(1).CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            enableDeletions,
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(-1L)]  // infinite
    [InlineData(0L)]   // exit immediately
    [InlineData(100L)] // custom value
    public async Task ExecuteAsync_Succeeds_WithMaximumErrors(long maxErrors)
    {
        // Arrange
        _managedLustreService.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Is(maxErrors),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("autoimport-job");

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--maximum-errors", maxErrors.ToString()
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _managedLustreService.Received(1).CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            maxErrors,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--resource-group rg1 --filesystem-name fs1", false)] // missing subscription
    [InlineData("--subscription sub123 --filesystem-name fs1", false)] // missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1", false)] // missing filesystem-name
    public async Task ExecuteAsync_ValidationErrors_Return400(string argLine, bool shouldSucceed)
    {
        // Arrange
        var args = _commandDefinition.Parse(argLine.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        var expectedStatus = shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        Assert.Equal(expectedStatus, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_RequestFailed_UsesStatusCode()
    {
        // Arrange
        _managedLustreService.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Azure.RequestFailedException(404, "not found"));

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_GenericException_Returns500()
    {
        // Arrange
        _managedLustreService.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("boom"));

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.True((int)response.Status >= 500);
        Assert.Contains("boom", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        _managedLustreService.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("autoimport-20250107120000");

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert - verify command executed successfully with expected parameters
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _managedLustreService.Received(1).CreateAutoimportJobAsync(
            _subscription,
            _resourceGroup,
            _fileSystemName,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
