// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureTerraform.Commands;
using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests;

public class AztfexportQueryCommandTests
{
    private readonly ILogger<AztfexportQueryCommand> _logger;
    private readonly IAztfexportService _aztfexportService;
    private readonly CommandContext _context;
    private readonly AztfexportQueryCommand _command;
    private readonly Command _commandDefinition;

    public AztfexportQueryCommandTests()
    {
        var collection = new ServiceCollection();
        var serviceProvider = collection.BuildServiceProvider();
        _context = new(serviceProvider);
        _logger = Substitute.For<ILogger<AztfexportQueryCommand>>();
        _aztfexportService = Substitute.For<IAztfexportService>();
        _command = new(_logger, _aztfexportService);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("query", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.True(_command.Metadata.LocalRequired);
    }

    [Fact]
    public async Task ExecuteAsync_AztfexportAvailable_ReturnsCommand()
    {
        var query = "type =~ 'Microsoft.Storage/storageAccounts'";
        var expectedResult = new AztfexportCommandResult
        {
            AztfexportFound = true,
            Command = "aztfexport",
            Args = ["query", "--non-interactive", "--plain-ui", query],
            Description = $"Export Azure resources by query: {query}"
        };

        _aztfexportService.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _aztfexportService.GenerateQueryCommand(
            query, null, "azurerm", null, false, 10, true)
            .Returns(expectedResult);

        var args = _commandDefinition.Parse(["--query", query]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_AztfexportNotAvailable_ReturnsInstallationHelp()
    {
        _aztfexportService.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);

        var args = _commandDefinition.Parse(["--query", "type =~ 'Microsoft.Storage/storageAccounts'"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingQuery_ReturnsValidationError()
    {
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        _aztfexportService.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        var args = _commandDefinition.Parse(["--query", "type =~ 'Microsoft.Storage/storageAccounts'"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var query = "type =~ 'Microsoft.Storage/storageAccounts'";
        var expectedResult = new AztfexportCommandResult
        {
            AztfexportFound = true,
            Command = "aztfexport",
            Args = ["query", "--non-interactive", "--plain-ui", query],
            Description = $"Export Azure resources by query: {query}"
        };

        _aztfexportService.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _aztfexportService.GenerateQueryCommand(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse(["--query", query]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureTerraformJsonContext.Default.AztfexportCommandResult);

        Assert.NotNull(result);
        Assert.True(result.AztfexportFound);
        Assert.Equal("aztfexport", result.Command);
        Assert.NotNull(result.Args);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = _commandDefinition.Parse([
            "--query", "type =~ 'Microsoft.Storage/storageAccounts'",
            "--output-folder", "./output",
            "--provider", "azapi",
            "--name-pattern", "res-"
        ]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);

        var command = _command.GetCommand();
        var options = command.Options;

        Assert.Contains(options, o => o.Name == "--query");
        Assert.Contains(options, o => o.Name == "--output-folder");
        Assert.Contains(options, o => o.Name == "--provider");
    }
}
