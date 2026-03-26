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

public class AztfexportResourceCommandTests
{
    private readonly ILogger<AztfexportResourceCommand> _logger;
    private readonly IAztfexportService _aztfexportService;
    private readonly CommandContext _context;
    private readonly AztfexportResourceCommand _command;
    private readonly Command _commandDefinition;

    public AztfexportResourceCommandTests()
    {
        var collection = new ServiceCollection();
        var serviceProvider = collection.BuildServiceProvider();
        _context = new(serviceProvider);
        _logger = Substitute.For<ILogger<AztfexportResourceCommand>>();
        _aztfexportService = Substitute.For<IAztfexportService>();
        _command = new(_logger, _aztfexportService);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("resource", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.NotEmpty(_command.Id);
        Assert.True(_command.Metadata.LocalRequired);
        Assert.True(_command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_AztfexportAvailable_ReturnsCommand()
    {
        var resourceId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa";
        var expectedResult = new AztfexportCommandResult
        {
            AztfexportFound = true,
            Command = "aztfexport",
            Args = ["resource", "--non-interactive", "--plain-ui", resourceId],
            Description = $"Export Azure resource: {resourceId}"
        };

        _aztfexportService.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _aztfexportService.GenerateResourceCommand(
            resourceId, null, "azurerm", null, false, 10, true)
            .Returns(expectedResult);

        var args = _commandDefinition.Parse(["--resource-id", resourceId]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_AztfexportNotAvailable_ReturnsInstallationHelp()
    {
        _aztfexportService.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);

        var args = _commandDefinition.Parse(["--resource-id", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceId_ReturnsValidationError()
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

        var args = _commandDefinition.Parse(["--resource-id", "/subscriptions/sub/rg/providers/Microsoft.Storage/storageAccounts/sa"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}
