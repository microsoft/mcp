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

public sealed class DrillRunFailoverCommandTests : CommandUnitTestsBase<DrillRunFailoverCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";
    private const string DrillRun = "run1";
    private const string SourceLocation = "eastus-az1";
    private const string SelectedResourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.AzureResilienceManagement/recoveryResources/resource1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("failover", command.Name);
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
            "--source-locations", SourceLocation);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().FailoverDrillRunAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("eastus")]
    [InlineData("eastus-az")]
    [InlineData("eastus-az0")]
    [InlineData("east_us-az1")]
    public async Task ExecuteAsync_RejectsInvalidSourceLocation(string sourceLocation)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--source-locations", sourceLocation);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("physical Azure zone format", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidSelectedResourceId()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--source-locations", SourceLocation,
            "--selected-resource-ids", "not-an-arm-id");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("absolute Azure resource ID", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_StartsFailoverAndReturnsAcceptedResult()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--source-locations", SourceLocation,
            "--source-locations", "westus2-az2",
            "--selected-resource-ids", SelectedResourceId,
            "--auto-failover", "true");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillRunFailoverCommandResult);
        Assert.True(result.Accepted);
        Assert.Equal(DrillRun, result.DrillRun);
        await Service.Received(1).FailoverDrillRunAsync(
            ServiceGroup,
            Drill,
            DrillRun,
            Arg.Is<IEnumerable<string>>(locations => locations.SequenceEqual(new[] { SourceLocation, "westus2-az2" })),
            Arg.Is<IEnumerable<string>>(resourceIds => resourceIds.SequenceEqual(new[] { SelectedResourceId })),
            true,
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
        Service.FailoverDrillRunAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--source-locations", SourceLocation);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }
}
