// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.SreAgent.Commands.Architecture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using Xunit;
using System.Text.Json;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Commands;

namespace Azure.Mcp.Tools.SreAgent.UnitTests.Architecture;

public class PlanCommandTests
{
    private readonly ILogger<PlanCommand> _logger;
    private readonly PlanCommand _command;

    public PlanCommandTests()
    {
        _logger = Substitute.For<ILogger<PlanCommand>>();
        _command = new PlanCommand(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var def = _command.GetCommand();
        Assert.Equal("plan", def.Name);
        Assert.NotNull(def.Description);
        Assert.NotEmpty(def.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var def = _command.GetCommand();
        Assert.NotNull(def.Options);
        Assert.True(def.Options.Any(o => o.Name == "--requirements"), "Missing --requirements option");
        Assert.True(def.Options.Any(o => o.Name == "--trigger-type"), "Missing --trigger-type option");
        Assert.True(def.Options.Any(o => o.Name == "--kusto-connector"), "Missing --kusto-connector option");
    }

    [Theory]
    [InlineData("--requirements \"simple kusto query tool\"", true)]
    [InlineData("--requirements \"simple kusto query tool\" --trigger-type scheduled", true)]
    [InlineData("--requirements \"simple kusto query tool\" --trigger-type scheduled --kusto-connector my-connector", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        TestEnvironment.ClearAzureSubscriptionId();
        var context = new CommandContext(new ServiceCollection().AddSingleton(_logger).BuildServiceProvider());
        var commandDef = _command.GetCommand();
        var parseResult = commandDef.Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult, CancellationToken.None);

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
        TestEnvironment.ClearAzureSubscriptionId();
        var context = new CommandContext(new ServiceCollection().AddSingleton(_logger).BuildServiceProvider());
        var commandDef = _command.GetCommand();
        var args = "--requirements \"create kusto telemetry dashboard\"";
        var parseResult = commandDef.Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        var content = JsonSerializer.Deserialize(JsonSerializer.Serialize(response.Results), SreAgentJsonContext.Default.SreAgentTextResult)!.Message;
        Assert.Contains("Architecture Plan", content);
        Assert.Contains("Component Diagram", content);
        Assert.Contains("mermaid", content);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyRequirements()
    {
        TestEnvironment.ClearAzureSubscriptionId();
        var context = new CommandContext(new ServiceCollection().AddSingleton(_logger).BuildServiceProvider());
        var commandDef = _command.GetCommand();
        var args = "--requirements \"\"";
        var parseResult = commandDef.Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        TestEnvironment.ClearAzureSubscriptionId();
        var context = new CommandContext(new ServiceCollection().AddSingleton(_logger).BuildServiceProvider());
        var commandDef = _command.GetCommand();
        var args = "--requirements \"create kusto telemetry dashboard\" --trigger-type scheduled --kusto-connector my-connector";
        var parseResult = commandDef.Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        var content = JsonSerializer.Deserialize(JsonSerializer.Serialize(response.Results), SreAgentJsonContext.Default.SreAgentTextResult)!.Message;
        // Verify that scheduled trigger type is reflected in output
        Assert.Contains("Scheduled Task", content);
        // Verify that the kusto connector is referenced in output
        Assert.Contains("my-connector", content);
    }
}
