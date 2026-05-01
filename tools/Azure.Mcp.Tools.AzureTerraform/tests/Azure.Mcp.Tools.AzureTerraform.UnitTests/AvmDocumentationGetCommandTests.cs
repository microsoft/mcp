// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Commands;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests;

public class AvmDocumentationGetCommandTests : CommandUnitTestsBase<AvmDocumentationGetCommand, IAvmDocsService>
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
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ReturnsDocumentation()
    {
        Service.GetDocumentationAsync("avm-res-storage-storageaccount", "0.4.0", Arg.Any<CancellationToken>())
            .Returns("# Azure Storage Account Module\n\nThis module creates a storage account.");

        var response = await ExecuteCommandAsync(
            "--module-name", "avm-res-storage-storageaccount",
            "--module-version", "0.4.0");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingModuleName_ReturnsValidationError()
    {
        var response = await ExecuteCommandAsync("--module-version", "0.4.0");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_MissingModuleVersion_ReturnsValidationError()
    {
        var response = await ExecuteCommandAsync("--module-name", "avm-res-storage-storageaccount");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        Service.GetDocumentationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Module not found"));

        var response = await ExecuteCommandAsync("--module-name", "nonexistent", "--module-version", "1.0.0");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_VerifiesServiceCalled()
    {
        Service.GetDocumentationAsync("test-module", "1.0.0", Arg.Any<CancellationToken>())
            .Returns("# Test Module");

        await ExecuteCommandAsync("--module-name", "test-module", "--module-version", "1.0.0");

        await Service.Received(1).GetDocumentationAsync("test-module", "1.0.0", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--module-name avm-res-storage-storageaccount --module-version 0.4.0", true)]
    [InlineData("--module-name avm-res-storage-storageaccount", false)]
    [InlineData("--module-version 0.4.0", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetDocumentationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns("# Module docs");
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
        Service.GetDocumentationAsync("avm-res-storage-storageaccount", "0.4.0", Arg.Any<CancellationToken>())
            .Returns("# Azure Storage Account Module\n\nThis module creates a storage account.");

        var response = await ExecuteCommandAsync(
            "--module-name", "avm-res-storage-storageaccount",
            "--module-version", "0.4.0");

        var result = ValidateAndDeserializeResponse(response, AzureTerraformJsonContext.Default.AvmDocumentationResult);

        Assert.Equal("avm-res-storage-storageaccount", result.ModuleName);
        Assert.Equal("0.4.0", result.ModuleVersion);
        Assert.Contains("Azure Storage Account Module", result.Documentation);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = CommandDefinition.Parse(["--module-name", "avm-res-storage-storageaccount", "--module-version", "0.4.0"]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);

        var command = Command.GetCommand();
        var options = command.Options;

        Assert.Contains(options, o => o.Name == "--module-name");
        Assert.Contains(options, o => o.Name == "--module-version");
    }
}
