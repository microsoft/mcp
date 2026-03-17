// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.WellArchitectedFramework.Commands.UsageGuide;
using Azure.Mcp.Tools.WellArchitectedFramework.Services.UsageGuide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.WellArchitectedFramework.UnitTests;

public class UsageGuideGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UsageGuideGetCommand> _logger;
    private readonly IUsageGuideService _usageGuideService;
    private readonly CommandContext _context;
    private readonly UsageGuideGetCommand _command;
    private readonly Command _commandDefinition;

    public UsageGuideGetCommandTests()
    {
        _logger = Substitute.For<ILogger<UsageGuideGetCommand>>();
        _usageGuideService = new UsageGuideService();

        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _command = new(_logger, _usageGuideService);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.NotNull(_command);
        Assert.Equal("get", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.Idempotent);
        Assert.False(_command.Metadata.OpenWorld);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.False(_command.Metadata.LocalRequired);
        Assert.False(_command.Metadata.Secret);
    }

    [Theory]
    [InlineData("", true)]  // Empty args is valid - command takes no parameters
    [InlineData("--unknown-option value", false)]  // Unknown options should fail
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Act & Assert
        if (shouldSucceed)
        {
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            // For failure cases, verify either parse errors or runtime validation failure
            if (parseResult.Errors.Count == 0)
            {
                var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

                // Runtime validation errors
                Assert.NotEqual(HttpStatusCode.OK, response.Status);
            }
            // Parse errors are also acceptable for failure cases
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsUsageGuide()
    {
        // Arrange
        var args = _commandDefinition.Parse("");

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<List<string>>(json);
        
        Assert.NotNull(result);
        Assert.Single(result);
        
        // Verify key content from the usage guide is present
        Assert.Contains("Azure Well-Architected Framework: AI Agent Usage Guide", result[0]);
        Assert.Contains("Building blocks of the framework", result[0]);
        Assert.Contains("Systematic Application Process", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var mockService = Substitute.For<IUsageGuideService>();
        mockService.GetUsageGuide().Returns(x => throw new InvalidOperationException("Test error"));
        
        var command = new UsageGuideGetCommand(_logger, mockService);
        var commandDefinition = command.GetCommand();
        var args = commandDefinition.Parse("");

        // Act
        var response = await command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }
}
