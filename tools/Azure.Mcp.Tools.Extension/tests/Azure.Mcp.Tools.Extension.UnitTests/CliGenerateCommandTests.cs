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

public sealed class CliGenerateCommandTests : CommandUnitTestsBase<CliGenerateCommand, ICliGenerateService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("generate", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--intent mock_intent", false)]
    [InlineData("--cli-type az", false)]
    [InlineData("--cli-type wrong_cli_type", false)]
    [InlineData("--intent mock_intent --cli-type az", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.GenerateAzureCLICommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Command")
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
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        Service.GenerateAzureCLICommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Command")
            });

        // Act
        var response = await ExecuteCommandAsync("--intent", "mock_intent", "--cli-type", "az");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ExtensionJsonContext.Default.CliGenerateResult);

        Assert.Equal("az", result.CliType);
        Assert.Equal("Command", result.Command);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GenerateAzureCLICommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--intent", "mock_intent", "--cli-type", "az");

        // Assert
        Assert.Equal([500], [(int)response.Status]);
        Assert.Contains("Test error", response.Message);
    }
}
