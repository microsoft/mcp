// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:words slis

using System.ClientModel.Primitives;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills;

public sealed class DrillUpdateCommandTests : CommandUnitTestsBase<DrillUpdateCommand, IResilienceManagementService>
{
    private const string ValidArgs = "--service-group sg1 --drill drill1 --rbac-setup-mode Manual";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData(ValidArgs, true)]
    [InlineData("--service-group sg1 --drill drill1 --subscription sub --region westus2", true)]
    [InlineData("--drill drill1 --rbac-setup-mode Manual", false)]
    [InlineData("--service-group sg1 --rbac-setup-mode Manual", false)]
    [InlineData("--service-group sg1 --drill drill1", false)]
    [InlineData("--service-group sg1 --drill drill1 --subscription sub", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            ConfigureUpdatedDrill();
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsUpdatedDrill()
    {
        ConfigureUpdatedDrill();

        var response = await ExecuteCommandAsync(ValidArgs);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillUpdateCommandResult);
        Assert.Equal("drill1", result.Drill.Name);
        await Service.Received(1).UpdateDrillAsync(
            "sg1",
            "drill1",
            null,
            null,
            DrillRbacSetupMode.Manual,
            null,
            Arg.Any<DrillUpdateProperties>(),
            null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "conflicts with the current resource state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        Service.UpdateDrillAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<DrillRbacSetupMode?>(),
            Arg.Any<string?>(),
            Arg.Any<DrillUpdateProperties?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, "Sensitive provider details"));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Sensitive provider details", response.Message);
    }

    private void ConfigureUpdatedDrill()
    {
        Service.UpdateDrillAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<DrillRbacSetupMode?>(),
            Arg.Any<string?>(),
            Arg.Any<DrillUpdateProperties?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new DrillInfo("id1", "drill1"));
    }

    [Theory]
    [InlineData("--health-model-monitoring-properties", "healthModelMonitoringProperties")]
    [InlineData("--sli-monitoring-properties", "sliMonitoringProperties")]
    public async Task ExecuteAsync_ForwardsExplicitNullWithoutOtherUpdates(string option, string propertyName)
    {
        ConfigureUpdatedDrill();
        var response = await ExecuteCommandAsync("--service-group", "sg1", "--drill", "drill1", option, "null");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var properties = Assert.IsAssignableFrom<DrillUpdateProperties>(Assert.Single(Service.ReceivedCalls()).GetArguments()[6]);
        using JsonDocument document = JsonDocument.Parse(ModelReaderWriter.Write(properties, new ModelReaderWriterOptions("W")));
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty(propertyName).ValueKind);
        Assert.Single(document.RootElement.EnumerateObject());
    }

    [Theory]
    [InlineData("--goal-assignment-properties", "null")]
    [InlineData("--health-model-monitoring-properties", """{"healthModelId":"invalid"}""")]
    [InlineData("--sli-monitoring-properties", """{"slis":[]}""")]
    public async Task ExecuteAsync_RejectsInvalidGaConfiguration(string option, string payload)
    {
        var response = await ExecuteCommandAsync("--service-group", "sg1", "--drill", "drill1", option, payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }
}
