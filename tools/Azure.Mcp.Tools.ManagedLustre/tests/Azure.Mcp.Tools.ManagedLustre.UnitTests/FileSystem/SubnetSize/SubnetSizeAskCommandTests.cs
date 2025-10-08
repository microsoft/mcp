// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedLustre.Commands;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem;

public class FileSystemSubnetSizeCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IManagedLustreService _amlfsService;
    private readonly ILogger<SubnetSizeAskCommand> _logger;
    private readonly SubnetSizeAskCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscriptionId = "sub123";

    public FileSystemSubnetSizeCommandTests()
    {
        _amlfsService = Substitute.For<IManagedLustreService>();
        _logger = Substitute.For<ILogger<SubnetSizeAskCommand>>();

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
        Assert.Equal("ask", command.Name);
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
        var result = JsonSerializer.Deserialize(json, ManagedLustreJsonContext.Default.FileSystemSubnetSizeResult);
        Assert.NotNull(result);
        Assert.Equal(21, result.NumberOfRequiredIPs);
    }

    [Theory]
    [InlineData("AMLFS-Durable-Premium-40")]
    [InlineData("AMLFS-Durable-Premium-125")]
    [InlineData("AMLFS-Durable-Premium-250")]
    [InlineData("AMLFS-Durable-Premium-500")]
    public async Task ExecuteAsync_ValidSkus_DoNotThrow(string sku)
    {
        // Arrange
        _amlfsService.GetRequiredAmlFSSubnetsSize(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>()).Returns(10);
        var args = _commandDefinition.Parse(["--sku", sku, "--size", "48", "--subscription", _knownSubscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Status == HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48 --subscription sub123", true)]
    [InlineData("--sku AMLFS-Durable-Premium-40 --subscription sub123", false)]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48", false)]
    [InlineData("--size 48 --subscription sub123", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        _amlfsService.GetRequiredAmlFSSubnetsSize(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>()).Returns(10);
        var parsedArgs = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parsedArgs);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
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
        Assert.True(response.Status >= HttpStatusCode.BadRequest);
        Assert.Contains("invalid sku", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_IsHandled()
    {
        // Arrange
        _amlfsService.GetRequiredAmlFSSubnetsSize(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception("error"));

        var args = _commandDefinition.Parse(["--sku", "AMLFS-Durable-Premium-40", "--size", "100", "--subscription", _knownSubscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.True(response.Status >= HttpStatusCode.InternalServerError);
        Assert.Contains("error", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
