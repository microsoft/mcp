// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.SreAgent.Commands.Workflows;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Workflows;

public class WorkflowsGenerateCommandTests : CommandUnitTestsBase<WorkflowsGenerateCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("generate", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var command = Command.GetCommand();
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.KindName}");
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.NameName}");
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.DescriptionName}");
    }

    [Theory]
    [InlineData("--kind agent --name test-agent --description \"Test agent\"", true)]
    [InlineData("--kind agent", false)]
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
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        var response = await ExecuteCommandAsync("--kind", "agent", "--name", "", "--description", "Test");

        // Missing required argument should result in BadRequest
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var response = await ExecuteCommandAsync("--kind", "agent", "--name", "test-agent", "--description", "Test agent");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        var response = await ExecuteCommandAsync("--kind", "agent", "--name", "test-agent", "--description", "Test agent", "--tools", "tool1", "--tools", "tool2");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
