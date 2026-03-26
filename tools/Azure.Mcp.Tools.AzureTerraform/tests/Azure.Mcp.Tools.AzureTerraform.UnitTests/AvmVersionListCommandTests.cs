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

public class AvmVersionListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AvmVersionListCommand> _logger;
    private readonly IAvmDocsService _avmDocsService;
    private readonly CommandContext _context;
    private readonly AvmVersionListCommand _command;
    private readonly Command _commandDefinition;

    public AvmVersionListCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<AvmVersionListCommand>>();
        _avmDocsService = Substitute.For<IAvmDocsService>();
        _command = new(_logger, _avmDocsService);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("versions", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.NotEmpty(_command.Id);
        Assert.NotEmpty(_command.Title);
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_ValidModuleName_ReturnsVersions()
    {
        var expectedVersions = new List<AvmVersion>
        {
            new() { TagName = "0.4.0", CreatedAt = "2024-12-01T00:00:00Z", TarballUrl = "https://api.github.com/repos/Azure/terraform-azurerm-avm-res-storage-storageaccount/tarball/v0.4.0" },
            new() { TagName = "0.3.0", CreatedAt = "2024-10-01T00:00:00Z", TarballUrl = "https://api.github.com/repos/Azure/terraform-azurerm-avm-res-storage-storageaccount/tarball/v0.3.0" }
        };

        _avmDocsService.GetVersionsAsync("avm-res-storage-storageaccount", Arg.Any<CancellationToken>())
            .Returns(expectedVersions);

        var args = _commandDefinition.Parse(["--module-name", "avm-res-storage-storageaccount"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingModuleName_ReturnsValidationError()
    {
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        _avmDocsService.GetVersionsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Module not found"));

        var args = _commandDefinition.Parse(["--module-name", "nonexistent-module"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}
