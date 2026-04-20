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

public class AvmModuleListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AvmModuleListCommand> _logger;
    private readonly IAvmDocsService _avmDocsService;
    private readonly CommandContext _context;
    private readonly AvmModuleListCommand _command;
    private readonly Command _commandDefinition;

    public AvmModuleListCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<AvmModuleListCommand>>();
        _avmDocsService = Substitute.For<IAvmDocsService>();
        _command = new(_logger, _avmDocsService);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.NotEmpty(_command.Id);
        Assert.NotEmpty(_command.Title);
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.True(_command.Metadata.Idempotent);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsModuleList()
    {
        var expectedModules = new List<AvmModule>
        {
            new()
            {
                ModuleName = "avm-res-storage-storageaccount",
                Description = "Azure Storage Account module",
                Source = "Azure/avm-res-storage-storageaccount/azurerm",
                RepoUrl = "https://github.com/Azure/terraform-azurerm-avm-res-storage-storageaccount"
            },
            new()
            {
                ModuleName = "avm-res-compute-virtualmachine",
                Description = "Azure Virtual Machine module",
                Source = "Azure/avm-res-compute-virtualmachine/azurerm",
                RepoUrl = "https://github.com/Azure/terraform-azurerm-avm-res-compute-virtualmachine"
            }
        };

        _avmDocsService.ListModulesAsync(Arg.Any<CancellationToken>())
            .Returns(expectedModules);

        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        _avmDocsService.ListModulesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var expectedModules = new List<AvmModule>
        {
            new()
            {
                ModuleName = "avm-res-storage-storageaccount",
                Description = "Azure Storage Account module",
                Source = "Azure/avm-res-storage-storageaccount/azurerm",
                RepoUrl = "https://github.com/Azure/terraform-azurerm-avm-res-storage-storageaccount"
            }
        };

        _avmDocsService.ListModulesAsync(Arg.Any<CancellationToken>())
            .Returns(expectedModules);

        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureTerraformJsonContext.Default.AvmModuleListResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Modules);
        Assert.Single(result.Modules);
        Assert.Equal("avm-res-storage-storageaccount", result.Modules[0].ModuleName);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = _commandDefinition.Parse([]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);
    }
}
