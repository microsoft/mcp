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

public class AzureRMDocsGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AzureRMDocsGetCommand> _logger;
    private readonly IAzureRMDocsService _docsService;
    private readonly CommandContext _context;
    private readonly AzureRMDocsGetCommand _command;
    private readonly Command _commandDefinition;

    public AzureRMDocsGetCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();

        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<AzureRMDocsGetCommand>>();
        _docsService = Substitute.For<IAzureRMDocsService>();
        _command = new(_logger, _docsService);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.NotEmpty(_command.Id);
        Assert.NotEmpty(_command.Title);
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.True(_command.Metadata.Idempotent);
    }

    [Fact]
    public async Task ExecuteAsync_ValidResourceType_ReturnsDocumentation()
    {
        var expectedResult = new AzureRMDocsResult
        {
            ResourceType = "azurerm_resource_group",
            DocumentationUrl = "https://example.com/docs",
            Summary = "Manages a Resource Group.",
            Arguments =
            [
                new() { Name = "name", Description = "The name.", Required = true, Type = "Single" }
            ],
            Attributes =
            [
                new() { Name = "id", Description = "The ID." }
            ],
            Examples = ["resource \"azurerm_resource_group\" \"example\" {}"],
            Notes = ["Some note"]
        };

        _docsService.GetDocumentationAsync(
            "azurerm_resource_group",
            "resource",
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse(["--resource-type", "azurerm_resource_group"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceType_ReturnsValidationError()
    {
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        _docsService.GetDocumentationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var args = _commandDefinition.Parse(["--resource-type", "azurerm_resource_group"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithDocType_PassesDocType()
    {
        _docsService.GetDocumentationAsync(
            "azurerm_resource_group",
            "data-source",
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(new AzureRMDocsResult { ResourceType = "azurerm_resource_group" });

        var args = _commandDefinition.Parse(["--resource-type", "azurerm_resource_group", "--doc-type", "data-source"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _docsService.Received(1).GetDocumentationAsync(
            "azurerm_resource_group",
            "data-source",
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithArgumentFilter_PassesArgumentName()
    {
        _docsService.GetDocumentationAsync(
            "azurerm_resource_group",
            "resource",
            "name",
            null,
            Arg.Any<CancellationToken>())
            .Returns(new AzureRMDocsResult { ResourceType = "azurerm_resource_group" });

        var args = _commandDefinition.Parse(["--resource-type", "azurerm_resource_group", "--argument", "name"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _docsService.Received(1).GetDocumentationAsync(
            "azurerm_resource_group",
            "resource",
            "name",
            null,
            Arg.Any<CancellationToken>());
    }
}
