// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Commands;
using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests;

public class AzureRMDocsGetCommandTests : CommandUnitTestsBase<AzureRMDocsGetCommand, IAzureRMDocsService>
{
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

        Service.GetDocumentationAsync(
            "azurerm_resource_group",
            "resource",
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync("--resource-type", "azurerm_resource_group");

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
        Service.GetDocumentationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var response = await ExecuteCommandAsync("--resource-type", "azurerm_resource_group");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithDocType_PassesDocType()
    {
        Service.GetDocumentationAsync(
            "azurerm_resource_group",
            "data-source",
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(new AzureRMDocsResult { ResourceType = "azurerm_resource_group" });

        var response = await ExecuteCommandAsync("--resource-type", "azurerm_resource_group", "--doc-type", "data-source");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetDocumentationAsync(
            "azurerm_resource_group",
            "data-source",
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithArgumentFilter_PassesArgumentName()
    {
        Service.GetDocumentationAsync(
            "azurerm_resource_group",
            "resource",
            "name",
            null,
            Arg.Any<CancellationToken>())
            .Returns(new AzureRMDocsResult { ResourceType = "azurerm_resource_group" });

        var response = await ExecuteCommandAsync("--resource-type", "azurerm_resource_group", "--argument", "name");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetDocumentationAsync(
            "azurerm_resource_group",
            "resource",
            "name",
            null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--resource-type azurerm_resource_group", true)]
    [InlineData("--resource-type azurerm_resource_group --doc-type data-source", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetDocumentationAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(new AzureRMDocsResult { ResourceType = "azurerm_resource_group" });
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

        Service.GetDocumentationAsync(
            "azurerm_resource_group",
            "resource",
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync("--resource-type", "azurerm_resource_group");

        var result = ValidateAndDeserializeResponse(response, AzureTerraformJsonContext.Default.AzureRMDocsResult);

        Assert.Equal("azurerm_resource_group", result.ResourceType);
        Assert.Equal("Manages a Resource Group.", result.Summary);
        Assert.NotNull(result.Arguments);
        Assert.Single(result.Arguments);
        Assert.Equal("name", result.Arguments[0].Name);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = CommandDefinition.Parse([
            "--resource-type", "azurerm_resource_group",
            "--doc-type", "data-source",
            "--argument", "name",
            "--attribute", "id"
        ]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);

        var options = CommandDefinition.Options;

        Assert.Contains(options, o => o.Name == "--resource-type");
        Assert.Contains(options, o => o.Name == "--doc-type");
        Assert.Contains(options, o => o.Name == "--argument");
        Assert.Contains(options, o => o.Name == "--attribute");
    }
}
