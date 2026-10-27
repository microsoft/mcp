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
    private const string SelectedResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/drills/drill1/drillResources/22222222-2222-2222-2222-222222222222";
    private const string OperationId = "11111111-1111-1111-1111-111111111111";

    [Theory]
    [InlineData()]
    [InlineData("--auto-failover", "true")]
    [InlineData("--auto-failover", "false")]
    [InlineData("--selected-resource-ids", SelectedResourceId)]
    public async Task ExecuteAsync_RejectsMissingSourceLocations(params string[] failoverInputs)
    {
        var response = await ExecuteCommandAsync(["--service-group", ServiceGroup, "--drill", Drill,
            "--drill-run", DrillRun, .. failoverInputs]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsAutomaticFailoverToFalse()
    {
        Service.FailoverDrillRunAsync(ServiceGroup, Drill, DrillRun, Arg.Any<IEnumerable<string>>(), null, false, null, null, Arg.Any<CancellationToken>())
            .Returns(OperationId);
        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--drill", Drill,
            "--drill-run", DrillRun, "--source-locations", SourceLocation);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).FailoverDrillRunAsync(ServiceGroup, Drill, DrillRun,
            Arg.Is<IEnumerable<string>>(locations => locations.SequenceEqual(new[] { SourceLocation })),
            null, false, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PreservesAutomaticFailoverFlag()
    {
        Service.FailoverDrillRunAsync(ServiceGroup, Drill, DrillRun, Arg.Any<IEnumerable<string>>(), null, true, null, null, Arg.Any<CancellationToken>())
            .Returns(OperationId);
        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--drill", Drill,
            "--drill-run", DrillRun, "--source-locations", SourceLocation, "--auto-failover");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(OperationId, ValidateAndDeserializeResponse(response,
            ResilienceManagementJsonContext.Default.DrillRunFailoverCommandResult).OperationId);
    }

    [Theory]
    [InlineData("allowed", "Allowed")]
    [InlineData("Unspecified", "Unspecified")]
    public async Task ExecuteAsync_ForwardsNormalizedUserConsent(string userConsent, string expected)
    {
        Service.FailoverDrillRunAsync(ServiceGroup, Drill, DrillRun, Arg.Any<IEnumerable<string>>(), null, false, expected, null, Arg.Any<CancellationToken>())
            .Returns(OperationId);
        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--drill", Drill,
            "--drill-run", DrillRun, "--source-locations", SourceLocation, "--user-consent", userConsent);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).FailoverDrillRunAsync(ServiceGroup, Drill, DrillRun,
            Arg.Any<IEnumerable<string>>(), null, false, expected, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUnknownUserConsent()
    {
        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--drill", Drill,
            "--drill-run", DrillRun, "--source-locations", SourceLocation, "--user-consent", "Denied");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--user-consent must be Unspecified or Allowed", response.Message, StringComparison.Ordinal);
        Assert.Empty(Service.ReceivedCalls());
    }

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

    [Theory]
    [InlineData("not-an-arm-id")]
    [InlineData("/subscriptions/x")]
    [InlineData("/not/a/real/id")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/resource1")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/other/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/resource1")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryResources/resource1")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/virtualMachines/vm1")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/22222222-2222-2222-2222-222222222222")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/other/providers/Microsoft.AzureResilienceManagement/drills/drill1/drillResources/22222222-2222-2222-2222-222222222222")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/drills/other/drillResources/22222222-2222-2222-2222-222222222222")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/drills/drill1/drillResources/resource1")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/drillResources/22222222-2222-2222-2222-222222222222")]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1")]
    public async Task ExecuteAsync_RejectsInvalidSelectedResourceId(string selectedResourceId)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--source-locations", SourceLocation,
            "--selected-resource-ids", selectedResourceId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("full drill-resource ID under the requested service group and drill", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceive().FailoverDrillRunAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(SelectedResourceId)]
    [InlineData("/providers/Microsoft.Management/serviceGroups/SG1/providers/Microsoft.AzureResilienceManagement/drills/DRILL1/drillResources/22222222-2222-2222-2222-222222222222")]
    public async Task ExecuteAsync_StartsFailoverAndReturnsAcceptedResult(string selectedResourceId)
    {
        Service.FailoverDrillRunAsync(
            ServiceGroup,
            Drill,
            DrillRun,
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<IEnumerable<string>?>(),
            true,
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(OperationId);

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--source-locations", SourceLocation,
            "--source-locations", "westus2-az2",
            "--selected-resource-ids", selectedResourceId,
            "--auto-failover", "true");

        Assert.True(response.Status == HttpStatusCode.OK, response.Message);
        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillRunFailoverCommandResult);
        Assert.True(result.Accepted);
        Assert.Equal(DrillRun, result.DrillRun);
        Assert.Equal(OperationId, result.OperationId);
        await Service.Received(1).FailoverDrillRunAsync(
            ServiceGroup,
            Drill,
            DrillRun,
            Arg.Is<IEnumerable<string>>(locations => locations.SequenceEqual(new[] { SourceLocation, "westus2-az2" })),
            Arg.Is<IEnumerable<string>>(resourceIds => resourceIds.SequenceEqual(new[] { selectedResourceId })),
            true,
            null,
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
