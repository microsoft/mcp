// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Support.Commands;
using Azure.Mcp.Tools.Support.Commands.ProblemClassification;
using Azure.Mcp.Tools.Support.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Support.Tests.ProblemClassification;

public class ProblemClassificationListCommandTests : CommandUnitTestsBase<ProblemClassificationListCommand, ISupportService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.False(string.IsNullOrWhiteSpace(CommandDefinition.Description));
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsProblemClassifications()
    {
        Service.ListProblemClassificationsAsync("svc1", null, Arg.Any<CancellationToken>())
            .Returns([new("pc1", "/providers/Microsoft.Support/services/svc1/problemClassifications/pc1", "Cannot start or stop my VM")]);

        var response = await ExecuteCommandAsync("--service", "svc1");

        var result = ValidateAndDeserializeResponse(response, SupportJsonContext.Default.ProblemClassificationListCommandResult);
        var classification = Assert.Single(result.ProblemClassifications);
        Assert.Equal("/providers/Microsoft.Support/services/svc1/problemClassifications/pc1", classification.Id);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenServiceMissing()
    {
        var response = await ExecuteCommandAsync([]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListProblemClassificationsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--service", "svc1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
