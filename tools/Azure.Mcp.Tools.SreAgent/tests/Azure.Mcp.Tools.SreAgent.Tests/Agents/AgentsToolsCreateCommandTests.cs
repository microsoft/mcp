// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.SreAgent.Commands;
using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Agents;

public class AgentsToolsCreateCommandTests : SubscriptionCommandUnitTestsBase<AgentsToolsCreateCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var command = Command.GetCommand();
        Assert.NotNull(command.Options);
        Assert.True(command.Options.Any(o => o.Name == "--agent"), "Missing --agent option");
        Assert.True(command.Options.Any(o => o.Name == "--name"), "Missing --name option");
        Assert.True(command.Options.Any(o => o.Name == "--tool-type"), "Missing --tool-type option");
        Assert.True(command.Options.Any(o => o.Name == "--description"), "Missing --description option");
        Assert.True(command.Options.Any(o => o.Name == "--connector"), "Missing --connector option");
    }

    [Theory]
    [InlineData("--subscription sub --agent myagent --name mytool --tool-type KustoTool --connector test-connector --database testdb", true)]
    [InlineData("--subscription sub --agent myagent --name mytool --tool-type LinkTool --url-template https://example.com/{id}", true)]
    [InlineData("--subscription sub --agent myagent --name mytool", false)]
    [InlineData("--subscription sub --agent myagent --tool-type KustoTool", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetAgentAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://test.azuresre.ai" });

            Service.CreateAgentToolAsync(
                Arg.Any<string>(),
                Arg.Any<SreAgentToolCreateRequest>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new SreAgentTool { Name = "mytool" });
        }

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
    public async Task ExecuteAsync_DeserializationValidation()
    {
        Service.GetAgentAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://test.azuresre.ai" });

        var testTool = new SreAgentTool { Name = "testtool", Properties = new SreAgentToolProperties { Type = "KustoTool" } };
        Service.CreateAgentToolAsync(
            Arg.Any<string>(),
            Arg.Any<SreAgentToolCreateRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(testTool);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testtool", "--tool-type", "KustoTool", "--connector", "test-connector", "--database", "testdb");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, SreAgentJsonContext.Default.AgentsToolsCreateCommandResult);
        Assert.NotNull(result.Tool);
        Assert.Equal("testtool", result.Tool.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetAgentAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://test.azuresre.ai" });

        Service.CreateAgentToolAsync(Arg.Any<string>(), Arg.Any<SreAgentToolCreateRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testtool", "--tool-type", "KustoTool", "--connector", "test-connector", "--database", "testdb");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        Service.GetAgentAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://test.azuresre.ai" });

        Service.CreateAgentToolAsync(
            Arg.Any<string>(),
            Arg.Any<SreAgentToolCreateRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentTool { Name = "testtool" });

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testtool", "--tool-type", "KustoTool", "--connector", "test-connector", "--database", "testdb", "--description", "test description");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateAgentToolAsync(
            Arg.Any<string>(),
            Arg.Is<SreAgentToolCreateRequest>(r =>
                r.Name == "testtool" &&
                r.Properties.Type == "KustoTool" &&
                r.Properties.Connector == "test-connector" &&
                r.Properties.Database == "testdb" &&
                r.Properties.Description == "test description"),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
