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

public class AztfexportResourceGroupCommandTests : CommandUnitTestsBase<AztfexportResourceGroupCommand, IAztfexportService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("resourcegroup", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.True(Command.Metadata.LocalRequired);
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

        Service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        Service.GenerateResourceGroupCommand(
            "my-rg", null, "azurerm", null, false, 10, true)
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync("--resource-group", "my-rg");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_AztfexportNotAvailable_ReturnsInstallationHelp()
    {
        Service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);

        var response = await ExecuteCommandAsync("--resource-group", "my-rg");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceGroup_ReturnsValidationError()
    {
        var response = await ExecuteCommandAsync([]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        Service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        var response = await ExecuteCommandAsync("--resource-group", "my-rg");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--resource-group my-rg", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
            Service.GenerateResourceGroupCommand(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<bool>())
                .Returns(new AztfexportCommandResult { AztfexportFound = true, Command = "aztfexport", Args = [], Description = "test" });
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
        var expectedResult = new AztfexportCommandResult
        {
            AztfexportFound = true,
            Command = "aztfexport",
            Args = ["resource-group", "--non-interactive", "--plain-ui", "my-rg"],
            Description = "Export Azure resource group: my-rg"
        };

        Service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        Service.GenerateResourceGroupCommand(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync("--resource-group", "my-rg");

        var result = ValidateAndDeserializeResponse(response, AzureTerraformJsonContext.Default.AztfexportCommandResult);

        Assert.True(result.AztfexportFound);
        Assert.Equal("aztfexport", result.Command);
        Assert.NotNull(result.Args);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = CommandDefinition.Parse([
            "--resource-group", "my-rg",
            "--output-folder", "./output",
            "--provider", "azapi"
        ]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);

        var options = CommandDefinition.Options;

        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--output-folder");
        Assert.Contains(options, o => o.Name == "--provider");
    }
}
