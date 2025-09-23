// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Commands;
using Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Models;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.AzureManagedLustre.UnitTests.FileSystem;

public class FileSystemListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureManagedLustreService _amlfsService;
    private readonly ILogger<FileSystemListCommand> _logger;
    private readonly FileSystemListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscriptionId = "sub123";
    private readonly string _knownResourceIdRg1 = "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Lustre/amlfs/fs1";
    private readonly string _knownResourceIdRg2 = "/subscriptions/sub123/resourceGroups/rg2/providers/Microsoft.Lustre/amlfs/fs2";

    public FileSystemListCommandTests()
    {
        _amlfsService = Substitute.For<IAzureManagedLustreService>();
        _logger = Substitute.For<ILogger<FileSystemListCommand>>();

        var services = new ServiceCollection().AddSingleton(_amlfsService);
        _serviceProvider = services.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFileSystems()
    {
        // Arrange
        var expected = new List<LustreFileSystem>
        {
            new(
                "fs1",
                _knownResourceIdRg1,
                "rg1",
                _knownSubscriptionId,
                "eastus",
                "Succeeded",
                "Available",
                "10.0.0.5",
                "AMLFS-Durable-Premium-40",
                48,
                null,
                "Monday",
                "01:00"
            ),
            new(
                "fs2",
                _knownResourceIdRg2,
                "rg2",
                _knownSubscriptionId,
                "eastus",
                "Succeeded",
                "Available",
                "10.0.0.20",
                "AMLFS-Durable-Premium-40",
                48,
                null,
                "Monday",
                "01:00"
            ),
        };

        _amlfsService.ListFileSystemsAsync(
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expected);

        var args = _commandDefinition.Parse([
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureManagedLustreJsonContext.Default.FileSystemListResult);

        Assert.NotNull(result);
        Assert.NotNull(result!.FileSystems);
        Assert.Equal(2, result.FileSystems.Count);
        Assert.Equal("fs1", result.FileSystems[0].Name);
    }

    [Theory]
    [InlineData("--resource-group testrg", false)] // Missing subscription
    [InlineData("--subscription sub123", true)] // Missing resource group
    [InlineData(" --resource-group testrg --subscription sub123", true)]
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expected = new List<LustreFileSystem>
            {
                new(
                    "fs1",
                    _knownResourceIdRg1,
                    "rg1",
                    _knownSubscriptionId,
                    "eastus",
                    "Succeeded",
                    "Available",
                    "10.0.0.5",
                    "AMLFS-Durable-Premium-40",
                    48,
                    null,
                    "Monday",
                    "01:00"
                ),
            };

            _amlfsService.ListFileSystemsAsync(
                Arg.Is(_knownSubscriptionId),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(expected);

        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);

            var json = JsonSerializer.Serialize(response.Results);
            var result = JsonSerializer.Deserialize(json, AzureManagedLustreJsonContext.Default.FileSystemListResult);
            Assert.NotNull(result!.FileSystems);
            Assert.NotNull(result.FileSystems[0].Name);
            Assert.Equal("fs1", result.FileSystems[0].Name);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }


    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoItems()
    {
        // Arrange
        _amlfsService.ListFileSystemsAsync(
            Arg.Is(_knownSubscriptionId),
            Arg.Is<string?>(x => x == null),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureManagedLustreJsonContext.Default.FileSystemListResult);

        Assert.NotNull(result);
        Assert.Empty(result.FileSystems);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_NotFound()
    {
        // Arrange - 404 Not Found
        _amlfsService.ListFileSystemsAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "not found"));

        var args = _commandDefinition.Parse(["--subscription", _knownSubscriptionId]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_Forbidden()
    {
        // Arrange - 403 Forbidden
        _amlfsService.ListFileSystemsAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "forbidden"));

        var args = _commandDefinition.Parse(["--subscription", _knownSubscriptionId]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("forbidden", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
