// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.AzureManagedLustre.UnitTests.FileSystem;

public class FileSystemSubnetSizeCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureManagedLustreService _amlfsService;
    private readonly ILogger<FileSystemSubnetSizeCommand> _logger;
    private readonly FileSystemSubnetSizeCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscriptionId = "sub123";

    public FileSystemSubnetSizeCommandTests()
    {
        _amlfsService = Substitute.For<IAzureManagedLustreService>();
        _logger = Substitute.For<ILogger<FileSystemSubnetSizeCommand>>();

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
        Assert.Equal("required-subnet-size", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }


    [Fact]
    public async Task ExecuteAsync_ReturnsRequiredIPs()
    {
        // Arrange
        _amlfsService.GetRequiredAmlFSSubnetsSize(
            Arg.Is(_knownSubscriptionId),
            Arg.Is("AMLFS-Durable-Premium-40"),
            Arg.Is(480),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(21);

        var args = _commandDefinition.Parse([
            "--sku", "AMLFS-Durable-Premium-40",
            "--size", "480",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ResultJson>(json);
        Assert.NotNull(result);
        Assert.Equal(21, result!.NumberOfRequiredIPs);
    }

    [Theory]
    [InlineData("AMLFS-Durable-Premium-125")]
    [InlineData("AMLFS-Durable-Premium-250")]
    [InlineData("AMLFS-Durable-Premium-500")]
    public async Task ExecuteAsync_ValidSkus_DoNotThrow(string sku)
    {
        // Arrange
        _amlfsService.GetRequiredAmlFSSubnetsSize(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>()).Returns(10);
        var args = _commandDefinition.Parse(["--sku", sku, "--size", "32", "--subscription", _knownSubscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Status == 200 || response.Results != null);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSku_Returns400()
    {
        // Arrange: The command validates SKU in BindOptions and throws ArgumentException
        var args = _commandDefinition.Parse([
            "--sku", "INVALID-SKU",
            "--size", "100",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.True(response.Status >= 400);
        Assert.Contains("invalid sku", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_IsHandled()
    {
        // Arrange
        _amlfsService.GetRequiredAmlFSSubnetsSize(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception("boom"));

        var args = _commandDefinition.Parse(["--sku", "AMLFS-Durable-Premium-40", "--size", "100", "--subscription", _knownSubscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.True(response.Status >= 500);
        Assert.Contains("boom", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    private class ResultJson
    {
        [JsonPropertyName("numberOfRequiredIPs")]
        public int NumberOfRequiredIPs { get; set; }
    }
}
