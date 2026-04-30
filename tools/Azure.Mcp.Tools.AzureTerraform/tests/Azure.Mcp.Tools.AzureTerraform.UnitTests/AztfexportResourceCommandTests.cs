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

public class AztfexportResourceCommandTests : CommandUnitTestsBase<AztfexportResourceCommand, IAztfexportService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("resource", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.NotEmpty(Command.Id);
        Assert.True(Command.Metadata.LocalRequired);
        Assert.True(Command.Metadata.ReadOnly);
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

        Service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        Service.GenerateResourceCommand(resourceId, null, "azurerm", null, false, 10, true)
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync("--resource-id", resourceId);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_AztfexportNotAvailable_ReturnsInstallationHelp()
    {
        Service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);

        var response = await ExecuteCommandAsync("--resource-id", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceId_ReturnsValidationError()
    {
        var response = await ExecuteCommandAsync([]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        Service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        var response = await ExecuteCommandAsync("--resource-id", "/subscriptions/sub/rg/providers/Microsoft.Storage/storageAccounts/sa");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--resource-id /subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
            Service.GenerateResourceCommand(
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
        var resourceId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa";
        var expectedResult = new AztfexportCommandResult
        {
            AztfexportFound = true,
            Command = "aztfexport",
            Args = ["resource", "--non-interactive", "--plain-ui", resourceId],
            Description = $"Export Azure resource: {resourceId}"
        };

        Service.IsAztfexportAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        Service.GenerateResourceCommand(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync("--resource-id", resourceId);

        var result = ValidateAndDeserializeResponse(response, AzureTerraformJsonContext.Default.AztfexportCommandResult);

        Assert.True(result.AztfexportFound);
        Assert.Equal("aztfexport", result.Command);
        Assert.NotNull(result.Args);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = CommandDefinition.Parse([
            "--resource-id", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa",
            "--output-folder", "./output",
            "--provider", "azapi"
        ]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);

        var options = CommandDefinition.Options;

        Assert.Contains(options, o => o.Name == "--resource-id");
        Assert.Contains(options, o => o.Name == "--output-folder");
        Assert.Contains(options, o => o.Name == "--provider");
    }
}
