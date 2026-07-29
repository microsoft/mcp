// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.SreAgent.Commands;
using Azure.Mcp.Tools.SreAgent.Commands.Docs;
using Azure.Mcp.Tools.SreAgent.Services;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Docs;

public class DocsGetCommandTests : SubscriptionCommandUnitTestsBase<DocsGetCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var command = Command.GetCommand();
        Assert.NotNull(command.Options);
        var optionNames = command.Options.Select(o => o.Name).ToList();
        Assert.Contains("--topic", optionNames);
    }

    [Theory]
    [InlineData("--topic agents", true)]
    [InlineData("--topic tools", true)]
    [InlineData("--topic all", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        var response = await ExecuteCommandAsync(args);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandleServiceErrors()
    {
        var response = await ExecuteCommandAsync("--topic", "unknown-topic");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, SreAgentJsonContext.Default.SreAgentTextResult);
        Assert.Contains("Unknown topic", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDocumentationContent()
    {
        var response = await ExecuteCommandAsync("--topic", "agents");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        var result = ValidateAndDeserializeResponse(response, SreAgentJsonContext.Default.SreAgentTextResult);
        Assert.Contains("Agent", result.Message);
    }
}
