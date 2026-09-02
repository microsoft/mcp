// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills.Runs;

public sealed class DrillRunAddNotesCommandTests : CommandUnitTestsBase<DrillRunAddNotesCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";
    private const string DrillRun = "run1";
    private const string Notes = "Validation completed successfully.";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("add-notes", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData()]
    [InlineData("--service-group", ServiceGroup)]
    [InlineData("--service-group", ServiceGroup, "--drill", Drill)]
    [InlineData("--service-group", ServiceGroup, "--drill", Drill, "--drill-run", DrillRun)]
    public async Task ExecuteAsync_RejectsMissingRequiredOptions(params string[] args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData("../sg1", Drill, DrillRun)]
    [InlineData(ServiceGroup, "drill/one", DrillRun)]
    [InlineData(ServiceGroup, Drill, "run\\one")]
    public async Task ExecuteAsync_RejectsInvalidResourceNames(string serviceGroup, string drill, string drillRun)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", serviceGroup,
            "--drill", drill,
            "--drill-run", drillRun,
            "--notes", Notes);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().AddDrillRunNotesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsWhitespaceNotes()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--notes", "   ");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("non-whitespace", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AddsNotesAndReturnsAcceptedResult()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--notes", Notes);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillRunAddNotesCommandResult);
        Assert.True(result.Accepted);
        Assert.Equal(DrillRun, result.DrillRun);
        Assert.DoesNotContain(Notes, response.Results?.ToString(), StringComparison.Ordinal);
        await Service.Received(1).AddDrillRunNotesAsync(
            ServiceGroup,
            Drill,
            DrillRun,
            Notes,
            null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "current state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.AddDrillRunNotesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--notes", Notes);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }
}
