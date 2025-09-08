// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Models;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.AzureManagedLustre.UnitTests.FileSystem;

public class FileSystemImportJobCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureManagedLustreService _amlfsService;
    private readonly ILogger<FileSystemImportJobCreateCommand> _logger;
    private readonly FileSystemImportJobCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;
    private readonly string _subscription = "sub123";
    private readonly string _resourceGroup = "rg1";
    private readonly string _fileSystem = "fs1";

    public FileSystemImportJobCreateCommandTests()
    {
        _amlfsService = Substitute.For<IAzureManagedLustreService>();
        _logger = Substitute.For<ILogger<FileSystemImportJobCreateCommand>>();

        var services = new ServiceCollection().AddSingleton(_amlfsService);
        _serviceProvider = services.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
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
            Arg.Is<IList<string>?>(x => x != null && x.Count == 1 && x[0] == "/"), // defaulted by command
            Arg.Is("OverwriteAlways"),
            Arg.Is<int?>(-1), // defaulted by command now
            Arg.Is<string?>("Active"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(new ImportJobInfo(
                "import-job-123",
                _fileSystem,
                _resourceGroup,
                _subscription,
                "Submitted (placeholder)",
                "OverwriteAlways",
                -1,
                "Active",
                new List<string> { "/" }));

        var args = _parser.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--file-system", _fileSystem
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ImportJobCreateResultJson>(json);
        Assert.NotNull(result);
        Assert.NotNull(result!.ImportJob);
        Assert.Equal("import-job-123", result.ImportJob.Name);
        Assert.Equal(_fileSystem, result.ImportJob.FileSystemName);
        Assert.Equal("/", result.ImportJob.ImportPrefixes![0]);
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
            Arg.Is<string?>("Active"),
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

        var args = _parser.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--file-system", _fileSystem,
            "--import-prefixes", prefixes[0], prefixes[1],
            "--conflict-resolution-mode", "Skip",
            "--maximum-errors", "5",
            "--admin-status", "Active",
            "--name", name
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        await _amlfsService.Received(1).CreateImportJobAsync(
            _subscription,
            _resourceGroup,
            _fileSystem,
            name,
            Arg.Is<IList<string>?>(p => p != null && p.Count == prefixes.Length && p[0] == prefixes[0] && p[1] == prefixes[1]),
            "Skip",
            5,
            "Active",
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
        var args = _parser.Parse(argLine.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
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
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Azure.RequestFailedException(404, "not found"));

        var args = _parser.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--file-system", _fileSystem
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(404, response.Status);
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
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception("boom"));

        var args = _parser.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--file-system", _fileSystem
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.True(response.Status >= 500);
        Assert.Contains("boom", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    private class ImportJobCreateResultJson
    {
        [JsonPropertyName("importJob")] public ImportJobJson? ImportJob { get; set; }
    }

    private class ImportJobJson
    {
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("fileSystemName")] public string FileSystemName { get; set; } = string.Empty;
        [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
        [JsonPropertyName("resourceGroupName")] public string? ResourceGroupName { get; set; }
        [JsonPropertyName("subscriptionId")] public string? SubscriptionId { get; set; }
        [JsonPropertyName("conflictResolutionMode")] public string? ConflictResolutionMode { get; set; }
        [JsonPropertyName("maximumErrors")] public int? MaximumErrors { get; set; }
        [JsonPropertyName("adminStatus")] public string? AdminStatus { get; set; }
        [JsonPropertyName("importPrefixes")] public IList<string>? ImportPrefixes { get; set; }
    }
}
