// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
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

public class AztfexportResourceGroupCommandTests
{
    private readonly ILogger<AztfexportResourceGroupCommand> _logger;
    private readonly IAztfexportService _aztfexportService;
    private readonly CommandContext _context;
    private readonly AztfexportResourceGroupCommand _command;
    private readonly Command _commandDefinition;

    public AztfexportResourceGroupCommandTests()
    {
        var collection = new ServiceCollection();
        var serviceProvider = collection.BuildServiceProvider();
        _context = new(serviceProvider);
        _logger = Substitute.For<ILogger<AztfexportResourceGroupCommand>>();
        _aztfexportService = Substitute.For<IAztfexportService>();
        _command = new(_logger, _aztfexportService);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("resourcegroup", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.True(_command.Metadata.LocalRequired);
    }

    [Fact]
    public async Task ExecuteAsync_AztfexportAvailable_ReturnsCommand()
    {
        var expectedResult = new AztfexportCommandResult
        {
            AztfexportFound = true,
            Command = "aztfexport",
            Args = ["resource-group", "--non-interactive", "--plain-ui", "my-rg"],
            Description = "Export Azure resource group: my-rg"
        };

        _aztfexportService.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _aztfexportService.GenerateResourceGroupCommand(
            "my-rg", null, "azurerm", null, false, 10, true)
            .Returns(expectedResult);

        var args = _commandDefinition.Parse(["--resource-group-name", "my-rg"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_AztfexportNotAvailable_ReturnsInstallationHelp()
    {
        _aztfexportService.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);

        var args = _commandDefinition.Parse(["--resource-group-name", "my-rg"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceGroupName_ReturnsValidationError()
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

        var args = _commandDefinition.Parse(["--resource-group-name", "my-rg"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}
