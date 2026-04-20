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

public class AzApiDocsGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AzApiDocsGetCommand> _logger;
    private readonly IAzApiDocsService _docsService;
    private readonly IAzApiExamplesService _examplesService;
    private readonly CommandContext _context;
    private readonly AzApiDocsGetCommand _command;
    private readonly Command _commandDefinition;

    public AzApiDocsGetCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();

        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<AzApiDocsGetCommand>>();
        _docsService = Substitute.For<IAzApiDocsService>();
        _examplesService = Substitute.For<IAzApiExamplesService>();
        _command = new(_logger, _docsService, _examplesService);
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
        Assert.False(_command.Metadata.LocalRequired);
        Assert.False(_command.Metadata.Secret);
    }

    [Fact]
    public async Task ExecuteAsync_ValidResourceType_ReturnsDocumentation()
    {
        var expectedResult = new AzApiDocsResult
        {
            ResourceType = "Microsoft.Compute/virtualMachines",
            ApiVersion = "2024-03-01",
            Schema = "resource \"azapi_resource\" \"virtualMachine\" { ... }",
            ParentResourceType = "Microsoft.Resources/resourceGroups",
            WritableScopes = "ResourceGroup",
            Summary = "AzAPI resource schema for Microsoft.Compute/virtualMachines@2024-03-01"
        };

        _docsService.GetDocumentation("Microsoft.Compute/virtualMachines", null)
            .Returns(expectedResult);

        _examplesService.GetExamplesAsync("Microsoft.Compute/virtualMachines", Arg.Any<CancellationToken>())
            .Returns(new List<AzApiExample>());

        var args = _commandDefinition.Parse(["--resource-type", "Microsoft.Compute/virtualMachines"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithApiVersion_PassesApiVersion()
    {
        var expectedResult = new AzApiDocsResult
        {
            ResourceType = "Microsoft.Storage/storageAccounts",
            ApiVersion = "2023-01-01",
            Schema = "...",
            Summary = "AzAPI resource schema"
        };

        _docsService.GetDocumentation("Microsoft.Storage/storageAccounts", "2023-01-01")
            .Returns(expectedResult);

        _examplesService.GetExamplesAsync("Microsoft.Storage/storageAccounts", Arg.Any<CancellationToken>())
            .Returns(new List<AzApiExample>());

        var args = _commandDefinition.Parse([
            "--resource-type", "Microsoft.Storage/storageAccounts",
            "--api-version", "2023-01-01"
        ]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        _docsService.Received(1).GetDocumentation("Microsoft.Storage/storageAccounts", "2023-01-01");
    }

    [Fact]
    public async Task ExecuteAsync_WithExamples_IncludesExamples()
    {
        var expectedResult = new AzApiDocsResult
        {
            ResourceType = "Microsoft.Compute/virtualMachines",
            ApiVersion = "2024-03-01",
            Schema = "...",
            Summary = "AzAPI resource schema"
        };

        var examples = new List<AzApiExample>
        {
            new()
            {
                Description = "Create a VM",
                Content = "resource \"azapi_resource\" \"vm\" { ... }",
                SourcePath = "settings/remarks/microsoft.compute/samples/vm.tf"
            }
        };

        _docsService.GetDocumentation("Microsoft.Compute/virtualMachines", null)
            .Returns(expectedResult);

        _examplesService.GetExamplesAsync("Microsoft.Compute/virtualMachines", Arg.Any<CancellationToken>())
            .Returns(examples);

        var args = _commandDefinition.Parse(["--resource-type", "Microsoft.Compute/virtualMachines"]);
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
        _docsService.GetDocumentation(Arg.Any<string>(), Arg.Any<string?>())
            .Throws(new InvalidDataException("Resource type not found."));

        var args = _commandDefinition.Parse(["--resource-type", "Microsoft.Fake/nonexistent"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}
