// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBestPractices.Commands;
using Azure.Mcp.Tools.AzureBestPractices.Commands.CodingAgent;
using Azure.Mcp.Tools.AzureBestPractices.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBestPractices.Tests;

public sealed class CodingAgentBestPracticesGetCommandTests
    : CommandUnitTestsBase<CodingAgentBestPracticesGetCommand, IAzureBestPracticesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get_coding_agent", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsGuidance()
    {
        Service.GetCodingAgentBestPracticesAsync(Arg.Any<CancellationToken>())
            .Returns("Use managed identity and retry with backoff.");

        var response = await ExecuteCommandAsync("");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(
            response,
            AzureBestPracticesJsonContext.Default.CodingAgentBestPracticesGetCommandResult);
        Assert.Equal("Use managed identity and retry with backoff.", result.BestPractices);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetCodingAgentBestPracticesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("guidance unavailable"));

        var response = await ExecuteCommandAsync("");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("guidance unavailable", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCancellation()
    {
        Service.GetCodingAgentBestPracticesAsync(Arg.Any<CancellationToken>())
            .Returns("ok");

        var response = await ExecuteCommandAsync("");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetCodingAgentBestPracticesAsync(Arg.Any<CancellationToken>());
    }
}
