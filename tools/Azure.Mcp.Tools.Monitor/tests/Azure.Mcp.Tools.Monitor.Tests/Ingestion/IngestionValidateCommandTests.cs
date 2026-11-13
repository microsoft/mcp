// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Ingestion;
using Azure.Mcp.Tools.Monitor.Models.Ingestion;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Ingestion;

public sealed class IngestionValidateCommandTests
    : SubscriptionCommandUnitTestsBase<IngestionValidateCommand, IMonitorIngestionService>
{
    private const string Data = "[{\"Message\":\"hello\"}]";

    [Fact]
    public void Command_HasExpectedMetadataAndOptions()
    {
        Assert.Equal("validate", Command.Name);
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.Same(MonitorJsonContext.Default.IngestionValidationResult, Command.ResultTypeInfo);

        Assert.Equal(
            ["--data-collection-rule", "--log-data", "--resource-group", "--stream", "--subscription", "--tenant"],
            CommandDefinition.Options.Select(o => o.Name).Order(StringComparer.Ordinal).ToArray());
    }

    [Theory]
    [InlineData("--resource-group rg --data-collection-rule dcr --log-data [{}]")]
    [InlineData("--subscription sub --data-collection-rule dcr --log-data [{}]")]
    [InlineData("--subscription sub --resource-group rg --log-data [{}]")]
    [InlineData("--subscription sub --resource-group rg --data-collection-rule dcr")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_Success_ReturnsValidationResult()
    {
        Service.ValidateLogDataAsync("sub", "rg", "dcr", "Custom-T_CL", Data, "tenant", Arg.Any<CancellationToken>())
            .Returns(new IngestionValidationResult(
                false, "dcr", "Custom-T_CL", 1, true,
                [new(0, "Message", "bad type")], []));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--data-collection-rule", "dcr",
            "--stream", "Custom-T_CL",
            "--log-data", Data,
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.IngestionValidationResult);
        Assert.False(result.IsValid);
        Assert.Equal("Message", result.Errors.Single().Column);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceFailure_ReturnsError()
    {
        Service.ValidateLogDataAsync(default!, default!, default!, default, default!, default, TestContext.Current.CancellationToken)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException("boom"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--data-collection-rule", "dcr",
            "--log-data", Data);

        Assert.True((int)response.Status >= 400);
        Assert.Contains("boom", response.Message);
    }
}
