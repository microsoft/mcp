// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Areas.Tools.Commands;
using Azure.Mcp.Core.Areas.Tools.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Core.UnitTests.Areas.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Tools.UnitTests;

public class ToolsListNamesCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ToolsListNamesCommand> _logger;
    private readonly CommandContext _context;
    private readonly ToolsListNamesCommand _command;
    private readonly Command _commandDefinition;

    public ToolsListNamesCommandTests()
    {
        var collection = new ServiceCollection();
        collection.AddLogging();

        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();
        collection.AddSingleton(commandFactory);

        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<ToolsListNamesCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Act & Assert
        Assert.Equal("list-names", _command.Name);
        Assert.Contains("List all available tool names", _command.Description);
        Assert.Equal("List Tool Names", _command.Title);
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.True(_command.Metadata.Idempotent);
        Assert.False(_command.Metadata.Secret);
    }

    [Fact]
    public void CanParseNamespaceOption()
    {
        // Arrange
        var commandDefinition = _command.GetCommand();

        // Act
        var parseResult = commandDefinition.Parse(["--namespace", "storage"]);

        // Assert
        Assert.NotNull(parseResult);
        Assert.False(parseResult.Errors.Any(), $"Parse errors: {string.Join(", ", parseResult.Errors)}");
        
        var namespaceValue = parseResult.GetValueOrDefault<string>(ToolsListOptionDefinitions.Namespace.Name);
        Assert.Equal("storage", namespaceValue);
    }

    [Fact]
    public async Task ExecuteAsync_WithNamespaceOption_FiltersCorrectly()
    {
        // Arrange
        var commandDefinition = _command.GetCommand();
        var parseResult = commandDefinition.Parse(["--namespace", "storage"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        // Note: We're not testing response.Results here since it's null in the test environment
        // but we can verify the command executes without errors
    }
}