// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Models;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem;

public class FileSystemImportJobCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IManagedLustreService _amlfsService;
    private readonly ILogger<FileSystemImportJobCreateCommand> _logger;
    private readonly FileSystemImportJobCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _subscription = "sub123";
    private readonly string _resourceGroup = "rg1";
    private readonly string _fileSystem = "fs1";

    public FileSystemImportJobCreateCommandTests()
    {
        _amlfsService = Substitute.For<IManagedLustreService>();
        _logger = Substitute.For<ILogger<FileSystemImportJobCreateCommand>>();

        var services = new ServiceCollection().AddSingleton(_amlfsService);
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
        _amlfsService.CreateImportJobAsync(
            Arg.Is(_subscription),
            Arg.Is(_resourceGroup),
            Arg.Is(_fileSystem),
            Arg.Is<string?>(x => x == null),
            Arg.Any<IList<string>?>(),
            Arg.Is("Skip"),
            Arg.Is<int?>(-1), // defaulted by command now
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(new ImportJobInfo(
                "import-job-123",
                _fileSystem,
                _resourceGroup,
                _subscription,
                "Submitted (placeholder)",
                "Skip",
                -1,
                "Active",
                new List<string> { "/" }));

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--file-system", _fileSystem
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert basic response status and that results object exists (contents validated indirectly via service return setup)
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify required args & defaults passed to service
        await _amlfsService.Received(1).CreateImportJobAsync(
            _subscription,
            _resourceGroup,
            _fileSystem,
            null,
            Arg.Any<IList<string>?>(),
            "Skip",
            -1,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());

        // Inspect captured call to verify prefixes list content
        var call = _amlfsService.ReceivedCalls().First(c => c.GetMethodInfo().Name == nameof(IManagedLustreService.CreateImportJobAsync));
        var prefixesArg = (IList<string>?)call.GetArguments()[4];
        Assert.NotNull(prefixesArg);
        Assert.Single(prefixesArg!); // default now contains root path
        Assert.Equal("/", prefixesArg![0]);
        // Inspect captured call to verify conflict resolution mode
        var conflictModeArg = (string)call.GetArguments()[5]!;
        Assert.Equal("Skip", conflictModeArg);
    }

    [Fact]
    public async Task ExecuteAsync_PassesOptionalParameters()
    {
        // Arrange
        var prefixes = new[] { "/a", "/b" };
        var name = "custom-job";
        _amlfsService.CreateImportJobAsync(
            Arg.Is(_subscription),
            Arg.Is(_resourceGroup),
            Arg.Is(_fileSystem),
            Arg.Is(name),
            Arg.Is<IList<string>?>(p => p != null && p.Count == prefixes.Length && p[0] == prefixes[0] && p[1] == prefixes[1]),
            Arg.Is("Skip"),
            Arg.Is<int?>(5),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(new ImportJobInfo(
                name,
                _fileSystem,
                _resourceGroup,
                _subscription,
                "Submitted (placeholder)",
                "Skip",
                5,
                "Active",
                prefixes.ToList()));

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--file-system", _fileSystem,
            "--import-prefixes", prefixes[0], prefixes[1],
            "--conflict-resolution-mode", "Skip",
            "--maximum-errors", "5",
            "--name", name
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _amlfsService.Received(1).CreateImportJobAsync(
                _subscription,
                _resourceGroup,
                _fileSystem,
                name,
                Arg.Is<IList<string>?>(p => p != null && p.Count == prefixes.Length && p[0] == prefixes[0] && p[1] == prefixes[1]),
                "Skip",
                5,
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>());
    }

    [Theory]
    [InlineData("--resource-group rg1 --file-system fs1", false)] // missing subscription
    [InlineData("--subscription sub123 --file-system fs1", false)] // missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1", false)] // missing file-system
    public async Task ExecuteAsync_ValidationErrors_Return400(string argLine, bool shouldSucceed)
    {
        // Arrange
        var args = _commandDefinition.Parse(argLine.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, args);

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
        _amlfsService.CreateImportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<IList<string>?>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Azure.RequestFailedException(404, "not found"));

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--file-system", _fileSystem
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_GenericException_Returns500()
    {
        // Arrange
        _amlfsService.CreateImportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<IList<string>?>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception("boom"));

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--file-system", _fileSystem
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.True((int)response.Status >= 500);
        Assert.Contains("boom", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidConflictResolutionMode_Returns400()
    {
        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--file-system", _fileSystem,
            "--conflict-resolution-mode", "OverwriteAlways"
        ]);

        var response = await _command.ExecuteAsync(_context, args);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid conflict resolution mode", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
