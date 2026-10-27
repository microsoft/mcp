// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Ingestion;
using Azure.Mcp.Tools.Monitor.Models.Ingestion;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Ingestion;

public sealed class IngestionUploadCommandTests
    : CommandUnitTestsBase<IngestionUploadCommand, IMonitorIngestionService>
{
    private const string Endpoint = "https://my-dce.eastus-1.ingest.monitor.azure.com";
    private const string Rule = "dcr-0123456789abcdef0123456789abcdef";
    private const string StreamName = "Custom-MyTable_CL";
    private const string Data = "[{\"TimeGenerated\":\"2024-01-01T00:00:00Z\",\"Message\":\"hello\"}]";

    [Fact]
    public void Command_HasExpectedMetadataAndOptions()
    {
        Assert.Equal("upload", Command.Name);
        Assert.Equal(ToolOperationPlane.Data, Command.Metadata.OperationPlane);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.Same(MonitorJsonContext.Default.IngestionUploadResult, Command.ResultTypeInfo);

        Assert.Equal(
            ["--data-collection-rule", "--ingestion-endpoint", "--log-data", "--stream", "--tenant"],
            CommandDefinition.Options.Select(o => o.Name).Order(StringComparer.Ordinal).ToArray());
    }

    [Theory]
    [InlineData("--data-collection-rule dcr-1 --stream s --log-data [{}]")]
    [InlineData("--ingestion-endpoint https://x --stream s --log-data [{}]")]
    [InlineData("--ingestion-endpoint https://x --data-collection-rule dcr-1 --log-data [{}]")]
    [InlineData("--ingestion-endpoint https://x --data-collection-rule dcr-1 --stream s")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().UploadLogsAsync(default!, default!, default!, default!, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{\"a\":1}")]
    [InlineData("[]")]
    public async Task ExecuteAsync_InvalidLogData_ReturnsBadRequestWithoutServiceCall(string data)
    {
        var response = await ExecuteCommandAsync(
            "--ingestion-endpoint", Endpoint,
            "--data-collection-rule", Rule,
            "--stream", StreamName,
            "--log-data", data);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().UploadLogsAsync(default!, default!, default!, default!, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_Success_ReturnsResult()
    {
        Service.UploadLogsAsync(Endpoint, Rule, StreamName, Data, "tenant", Arg.Any<CancellationToken>())
            .Returns(new IngestionUploadResult("Succeeded", Rule, StreamName, 1));

        var response = await ExecuteCommandAsync(
            "--ingestion-endpoint", Endpoint,
            "--data-collection-rule", Rule,
            "--stream", StreamName,
            "--log-data", Data,
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.IngestionUploadResult);
        Assert.Equal("Succeeded", result.Status);
        Assert.Equal(1, result.RecordCount);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceFailure_ReturnsError()
    {
        Service.UploadLogsAsync(default!, default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException("boom"));

        var response = await ExecuteCommandAsync(
            "--ingestion-endpoint", Endpoint,
            "--data-collection-rule", Rule,
            "--stream", StreamName,
            "--log-data", Data);

        Assert.True((int)response.Status >= 400);
        Assert.Contains("boom", response.Message);
    }
}
