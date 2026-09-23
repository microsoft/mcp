// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public sealed class GoalAssignmentUpdateResourcesCommandTests
    : CommandUnitTestsBase<GoalAssignmentUpdateResourcesCommand, IResilienceManagementService>
{
    internal const string Resources = """
        [{"id":"/providers/Microsoft.Management/serviceGroups/sg/providers/Microsoft.AzureResilienceManagement/goalAssignments/ga/goalResources/11111111-1111-1111-1111-111111111111","properties":{"resourceArmId":"/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm","highAvailabilityGoalParticipation":"Excluded","highAvailabilityAttestationStatus":"NotAttested"}}]
        """;

    [Fact]
    public void Constructor_HasMetadata()
    {
        Assert.Equal("update-resources", Command.GetCommand().Name);
        Assert.Contains("include or exclude", Command.GetCommand().Description);
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
    }

    [Theory]
    [InlineData("Excluded")]
    [InlineData("Included")]
    public async Task ExecuteAsync_PassesExactArguments(string participation)
    {
        var resources = Resources.Replace("Excluded", participation, StringComparison.Ordinal);
        Service.UpdateGoalAssignmentResourcesAsync("sg", "ga", resources, "tenant1", Arg.Any<CancellationToken>())
            .Returns(new GoalAssignmentOperationResult("11111111-1111-1111-1111-111111111111", "Accepted", false));
        var response = await ExecuteCommandAsync("--service-group", "sg", "--goal-assignment", "ga", "--resources", resources, "--tenant", "tenant1");
        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalAssignmentOperationResult);
        Assert.Equal("11111111-1111-1111-1111-111111111111", result.OperationId);
        Assert.False(result.HasCompleted);
        Assert.Equal("Accepted", result.Status);
        await Service.Received(1).UpdateGoalAssignmentResourcesAsync("sg", "ga", resources, "tenant1", TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("[null]")]
    [InlineData("[{}]")]
    [InlineData("not-json")]
    public async Task ExecuteAsync_RejectsMalformedPayload(string resources)
    {
        var response = await ExecuteCommandAsync("--service-group", "sg", "--goal-assignment", "ga", "--resources", resources);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("\"disasterRecoveryGoalParticipation\":\"Included\",\"disasterRecoveryAttestationStatus\":\"ManuallyAttested\",")]
    [InlineData("\"userConfirmationForHighAvailability\":[],")]
    [InlineData("\"userConfirmationForHighAvailability\":[{\"solutionDisplayName\":\"ZonePinnedVmWithZrsDisk\",\"confirmationStatus\":\"ApprovedByUser\",\"reasonForRequestingConfirmation\":\"ZonePinnedZrsDataDisksConditional\"}],")]
    public async Task ExecuteAsync_PreservesOptionalProperties(string properties)
    {
        var resources = Resources.Replace("\"properties\":{", "\"properties\":{" + properties, StringComparison.Ordinal);
        Service.UpdateGoalAssignmentResourcesAsync("sg", "ga", resources, null, Arg.Any<CancellationToken>())
            .Returns(new GoalAssignmentOperationResult("11111111-1111-1111-1111-111111111111", "Accepted", false));
        var response = await ExecuteCommandAsync("--service-group", "sg", "--goal-assignment", "ga", "--resources", resources);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateGoalAssignmentResourcesAsync("sg", "ga", resources, null, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("serviceGroups/sg/", "serviceGroups/other/")]
    [InlineData("goalAssignments/ga/", "goalAssignments/other/")]
    [InlineData("goalResources/11111111-1111-1111-1111-111111111111", "goalResources/not-a-guid")]
    [InlineData("Excluded", "excluded")]
    [InlineData("NotAttested", "Unknown")]
    [InlineData("resourceArmId", "unknownField")]
    [InlineData("\"properties\":{", "\"properties\":{\"exclusionReasonForHighAvailabilityGoals\":\"UserSelectedExclusion\",")]
    [InlineData("\"properties\":{", "\"properties\":{\"disasterRecoveryGoalParticipation\":\"invalid\",")]
    [InlineData("\"properties\":{", "\"properties\":{\"userConfirmationForHighAvailability\":[null],")]
    [InlineData("\"properties\":{", "\"properties\":{\"highAvailabilityGoalParticipation\":\"Included\",")]
    public async Task ExecuteAsync_RejectsInvalidResourceProperties(string oldValue, string newValue)
    {
        var response = await ExecuteCommandAsync("--service-group", "sg", "--goal-assignment", "ga", "--resources", Resources.Replace(oldValue, newValue, StringComparison.Ordinal));
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData("--service-group sg --goal-assignment ga")]
    [InlineData("--service-group sg --resources []")]
    [InlineData("--goal-assignment ga --resources []")]
    [InlineData("--service-group ../sg --goal-assignment ga --resources []")]
    public async Task ExecuteAsync_RejectsMissingOrUnsafeOptions(string arguments)
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await ExecuteCommandAsync(arguments)).Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsDuplicates()
    {
        var entry = Resources[1..^1];
        var response = await ExecuteCommandAsync("--service-group", "sg", "--goal-assignment", "ga", "--resources", $"[{entry},{entry}]");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsOversizedUtf8Payload()
    {
        var response = await ExecuteCommandAsync("--service-group", "sg", "--goal-assignment", "ga", "--resources", new string('\u00E9', 524_289));
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("1 MiB", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(409)]
    [InlineData(500)]
    [InlineData(0)]
    public async Task ExecuteAsync_SanitizesErrors(int status)
    {
        Exception exception = status == 0 ? new Exception("secret-provider-detail") : new RequestFailedException(status, "secret-provider-detail");
        Service.UpdateGoalAssignmentResourcesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);
        var response = await ExecuteCommandAsync("--service-group", "sg", "--goal-assignment", "ga", "--resources", Resources);
        Assert.Equal((HttpStatusCode)(status == 0 ? 500 : status), response.Status);
        Assert.DoesNotContain("secret-provider-detail", response.Message);
        Assert.Null(response.Results);
        Assert.Contains(Logger.ReceivedCalls(), call => call.GetMethodInfo().Name == "Log" && ReferenceEquals(call.GetArguments()[3], exception));
    }
}
