// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Commands;
using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests;

public class AzApiDocsGetCommandTests : CommandUnitTestsBase<AzApiDocsGetCommand, IAzApiDocsService>
{
    private readonly IAzApiExamplesService _examplesService;

    public AzApiDocsGetCommandTests()
    {
        _examplesService = Substitute.For<IAzApiExamplesService>();
        Services.AddSingleton(_examplesService);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.NotEmpty(Command.Id);
        Assert.NotEmpty(Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.Secret);
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

        Service.GetDocumentation("Microsoft.Compute/virtualMachines", null)
            .Returns(expectedResult);

        _examplesService.GetExamplesAsync("Microsoft.Compute/virtualMachines", Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--resource-type", "Microsoft.Compute/virtualMachines");

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

        Service.GetDocumentation("Microsoft.Storage/storageAccounts", "2023-01-01")
            .Returns(expectedResult);

        _examplesService.GetExamplesAsync("Microsoft.Storage/storageAccounts", Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(
            "--resource-type", "Microsoft.Storage/storageAccounts",
            "--api-version", "2023-01-01");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Service.Received(1).GetDocumentation("Microsoft.Storage/storageAccounts", "2023-01-01");
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

        Service.GetDocumentation("Microsoft.Compute/virtualMachines", null)
            .Returns(expectedResult);

        _examplesService.GetExamplesAsync("Microsoft.Compute/virtualMachines", Arg.Any<CancellationToken>())
            .Returns(examples);

        var response = await ExecuteCommandAsync("--resource-type", "Microsoft.Compute/virtualMachines");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceType_ReturnsValidationError()
    {
        var response = await ExecuteCommandAsync([]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        Service.GetDocumentation(Arg.Any<string>(), Arg.Any<string?>())
            .Throws(new InvalidDataException("Resource type not found."));

        var response = await ExecuteCommandAsync("--resource-type", "Microsoft.Fake/nonexistent");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--resource-type Microsoft.Compute/virtualMachines", true)]
    [InlineData("--resource-type Microsoft.Compute/virtualMachines --api-version 2024-03-01", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetDocumentation(Arg.Any<string>(), Arg.Any<string?>())
                .Returns(new AzApiDocsResult { ResourceType = "Microsoft.Compute/virtualMachines", ApiVersion = "2024-03-01", Schema = "...", Summary = "..." });
            _examplesService.GetExamplesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var response = await ExecuteCommandAsync(args);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
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

        Service.GetDocumentation("Microsoft.Compute/virtualMachines", null)
            .Returns(expectedResult);

        _examplesService.GetExamplesAsync("Microsoft.Compute/virtualMachines", Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--resource-type", "Microsoft.Compute/virtualMachines");

        var result = ValidateAndDeserializeResponse(response, AzureTerraformJsonContext.Default.AzApiDocsResult);

        Assert.NotNull(result);
        Assert.Equal("Microsoft.Compute/virtualMachines", result.ResourceType);
        Assert.Equal("2024-03-01", result.ApiVersion);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = CommandDefinition.Parse(["--resource-type", "Microsoft.Compute/virtualMachines", "--api-version", "2024-03-01"]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);

        var options = CommandDefinition.Options;

        Assert.Contains(options, o => o.Name == "--resource-type");
        Assert.Contains(options, o => o.Name == "--api-version");
    }
}
