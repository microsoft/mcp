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

public class AvmDocumentationGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AvmDocumentationGetCommand> _logger;
    private readonly IAvmDocsService _avmDocsService;
    private readonly CommandContext _context;
    private readonly AvmDocumentationGetCommand _command;
    private readonly Command _commandDefinition;

    public AvmDocumentationGetCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<AvmDocumentationGetCommand>>();
        _avmDocsService = Substitute.For<IAvmDocsService>();
        _command = new(_logger, _avmDocsService);
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
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ReturnsDocumentation()
    {
        _avmDocsService.GetDocumentationAsync("avm-res-storage-storageaccount", "0.4.0", Arg.Any<CancellationToken>())
            .Returns("# Azure Storage Account Module\n\nThis module creates a storage account.");

        var args = _commandDefinition.Parse(["--module-name", "avm-res-storage-storageaccount", "--module-version", "0.4.0"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingModuleName_ReturnsValidationError()
    {
        var args = _commandDefinition.Parse(["--module-version", "0.4.0"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_MissingModuleVersion_ReturnsValidationError()
    {
        var args = _commandDefinition.Parse(["--module-name", "avm-res-storage-storageaccount"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        _avmDocsService.GetDocumentationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Module not found"));

        var args = _commandDefinition.Parse(["--module-name", "nonexistent", "--module-version", "1.0.0"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_VerifiesServiceCalled()
    {
        _avmDocsService.GetDocumentationAsync("test-module", "1.0.0", Arg.Any<CancellationToken>())
            .Returns("# Test Module");

        var args = _commandDefinition.Parse(["--module-name", "test-module", "--module-version", "1.0.0"]);
        await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        await _avmDocsService.Received(1).GetDocumentationAsync("test-module", "1.0.0", Arg.Any<CancellationToken>());
    }
}
