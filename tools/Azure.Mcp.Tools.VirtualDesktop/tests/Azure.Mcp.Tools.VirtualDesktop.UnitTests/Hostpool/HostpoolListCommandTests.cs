// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Models;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.VirtualDesktop.UnitTests.Hostpool;

[Trait("Area", "VirtualDesktop")]
public class HostpoolListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IVirtualDesktopService _virtualDesktopService;
    private readonly ILogger<HostpoolListCommand> _logger;
    private readonly HostpoolListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public HostpoolListCommandTests()
    {
        _virtualDesktopService = Substitute.For<IVirtualDesktopService>();
        _logger = Substitute.For<ILogger<HostpoolListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_virtualDesktopService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Act
        var command = _command.GetCommand();

        // Assert
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Equal("List hostpools", _command.Title);
    }

    [Theory]
    [InlineData("--subscription test-sub", true)]
    [InlineData("--subscription test-sub --tenant test-tenant", true)]
    [InlineData("--subscription test-sub --resource-group test-rg", true)]
    [InlineData("--subscription test-sub --resource-group test-rg --tenant test-tenant", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var hostpools = new List<HostPool>
            {
                new() { Name = "hostpool1" },
                new() { Name = "hostpool2" }
            }.AsReadOnly();
            _virtualDesktopService.ListHostpoolsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(hostpools);
            _virtualDesktopService.ListHostpoolsByResourceGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(hostpools);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.Contains("required", response.Message?.ToLower() ?? "");
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyResult_WhenNoHostpools()
    {
        // Arrange
        _virtualDesktopService.ListHostpoolsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);
        _virtualDesktopService.ListHostpoolsByResourceGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var parseResult = _commandDefinition.Parse("--subscription test-sub");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _virtualDesktopService.ListHostpoolsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<IReadOnlyList<HostPool>>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse("--subscription test-sub");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsHostpools_WhenSuccessful()
    {
        // Arrange
        var expectedHostpools = new List<HostPool>
        {
            new() { Name = "hostpool1" },
            new() { Name = "hostpool2" }
        }.AsReadOnly();
        _virtualDesktopService.ListHostpoolsAsync("test-sub", null, Arg.Any<RetryPolicyOptions>())
            .Returns(expectedHostpools);

        var parseResult = _commandDefinition.Parse("--subscription test-sub");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _virtualDesktopService.Received(1).ListHostpoolsAsync("test-sub", null, Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_CallsResourceGroupService_WhenResourceGroupProvided()
    {
        // Arrange
        var expectedHostpools = new List<HostPool>
        {
            new() { Name = "hostpool1" },
            new() { Name = "hostpool2" }
        }.AsReadOnly();
        _virtualDesktopService.ListHostpoolsByResourceGroupAsync("test-sub", "test-rg", null, Arg.Any<RetryPolicyOptions>())
            .Returns(expectedHostpools);

        var parseResult = _commandDefinition.Parse("--subscription test-sub --resource-group test-rg");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _virtualDesktopService.Received(1).ListHostpoolsByResourceGroupAsync("test-sub", "test-rg", null, Arg.Any<RetryPolicyOptions>());
        await _virtualDesktopService.DidNotReceive().ListHostpoolsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_CallsSubscriptionService_WhenNoResourceGroup()
    {
        // Arrange
        var expectedHostpools = new List<HostPool>
        {
            new() { Name = "hostpool1" },
            new() { Name = "hostpool2" }
        }.AsReadOnly();
        _virtualDesktopService.ListHostpoolsAsync("test-sub", null, Arg.Any<RetryPolicyOptions>())
            .Returns(expectedHostpools);

        var parseResult = _commandDefinition.Parse("--subscription test-sub");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _virtualDesktopService.Received(1).ListHostpoolsAsync("test-sub", null, Arg.Any<RetryPolicyOptions>());
        await _virtualDesktopService.DidNotReceive().ListHostpoolsByResourceGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyResult_WhenNoHostpoolsInResourceGroup()
    {
        // Arrange
        _virtualDesktopService.ListHostpoolsByResourceGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var parseResult = _commandDefinition.Parse("--subscription test-sub --resource-group test-rg");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
