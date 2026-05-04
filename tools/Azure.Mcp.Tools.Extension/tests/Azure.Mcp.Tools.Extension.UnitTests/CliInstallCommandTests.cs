// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.Extension.Commands;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Extension.UnitTests;

public sealed class CliInstallCommandTests : CommandUnitTestsBase<CliInstallCommand, ICliInstallService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("install", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--cli-type azd", true)]
    [InlineData("--cli-type az", true)]
    [InlineData("--cli-type func", true)]
    [InlineData("--cli-type wrong_cli_type", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.GetCliInstallInstructions(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Instructions")
                });
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal([shouldSucceed ? 200 : 400], [(int)response.Status]);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        Service.GetCliInstallInstructions(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Instructions")
            });

        // Act
        var response = await ExecuteCommandAsync("--cli-type", "az");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ExtensionJsonContext.Default.CliInstallResult);

        Assert.Equal("az", result.CliType);
        Assert.Equal("Instructions", result.InstallationInstructions);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GetCliInstallInstructions(Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--cli-type", "az");

        // Assert
        Assert.Equal([500], [(int)response.Status]);
        Assert.Contains("Test error", response.Message);
    }
}
