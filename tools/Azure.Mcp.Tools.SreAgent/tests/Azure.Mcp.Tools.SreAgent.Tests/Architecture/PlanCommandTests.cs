// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.SreAgent.Commands;
using Azure.Mcp.Tools.SreAgent.Commands.Architecture;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Architecture;

public class PlanCommandTests : CommandUnitTestsBase<PlanCommand, object>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var def = Command.GetCommand();
        Assert.Equal("plan", def.Name);
        Assert.NotNull(def.Description);
        Assert.NotEmpty(def.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var def = Command.GetCommand();
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
        var response = await ExecuteCommandAsync("--requirements \"create kusto telemetry dashboard\"");

        var content = ValidateAndDeserializeResponse(response, SreAgentJsonContext.Default.SreAgentTextResult).Message;
        Assert.Contains("Architecture Plan", content);
        Assert.Contains("Component Diagram", content);
        Assert.Contains("mermaid", content);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyRequirements()
    {
        var response = await ExecuteCommandAsync("--requirements", "");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        var response = await ExecuteCommandAsync("--requirements \"create kusto telemetry dashboard\" --trigger-type scheduled --kusto-connector my-connector");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        var content = JsonSerializer.Deserialize(JsonSerializer.Serialize(response.Results), SreAgentJsonContext.Default.SreAgentTextResult)!.Message;
        // Verify that scheduled trigger type is reflected in output
        Assert.Contains("Scheduled Task", content);
        // Verify that the kusto connector is referenced in output
        Assert.Contains("my-connector", content);
    }
}
