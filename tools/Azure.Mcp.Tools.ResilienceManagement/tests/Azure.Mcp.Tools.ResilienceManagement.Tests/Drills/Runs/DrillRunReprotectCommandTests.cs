// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills.Runs;

public sealed class DrillRunReprotectCommandTests : CommandUnitTestsBase<DrillRunReprotectCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";
    private const string DrillRun = "run1";
    private const string OperationId = "11111111-1111-1111-1111-111111111111";
    private const string SelectedResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/drills/drill1/drillResources/22222222-2222-2222-2222-222222222222";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("reprotect", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData()]
    [InlineData("--service-group", ServiceGroup)]
    [InlineData("--service-group", ServiceGroup, "--drill", Drill)]
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
            "--drill-run", drillRun);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().ReprotectDrillRunAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsSelectedResourceOutsideRequestedDrill()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--selected-resource-ids",
            "/providers/Microsoft.Management/serviceGroups/other/providers/Microsoft.AzureResilienceManagement/drills/other/drillResources/22222222-2222-2222-2222-222222222222");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().ReprotectDrillRunAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReprotectsDrillRunAndReturnsAcceptedResult()
    {
        Service.ReprotectDrillRunAsync(
            ServiceGroup,
            Drill,
            DrillRun,
            Arg.Any<IEnumerable<string>?>(),
            null,
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new DrillRunReprotectResult(
                OperationId,
                "Accepted",
                false,
                202,
                "https://management.azure.com/providers/Microsoft.AzureResilienceManagement/locations/westus2/operationStatuses/provider-operation",
                "https://management.azure.com/providers/Microsoft.AzureResilienceManagement/locations/westus2/operations/provider-operation"));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--selected-resource-ids", SelectedResourceId);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillRunReprotectCommandResult);
        Assert.True(result.Accepted);
        Assert.Equal(DrillRun, result.DrillRun);
        Assert.Equal(OperationId, result.OperationId);
        Assert.Equal("Accepted", result.Status);
        Assert.False(result.HasCompleted);
        Assert.Equal(202, result.HttpStatus);
        Assert.Contains("operationStatuses/provider-operation", result.LocationUri);
        Assert.Contains("operations/provider-operation", result.AzureAsyncOperationUri);
        await Service.Received(1).ReprotectDrillRunAsync(
            ServiceGroup,
            Drill,
            DrillRun,
            Arg.Is<IEnumerable<string>?>(ids => ids != null && ids.SequenceEqual(new[] { SelectedResourceId })),
            null,
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "current state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.ReprotectDrillRunAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }
}
