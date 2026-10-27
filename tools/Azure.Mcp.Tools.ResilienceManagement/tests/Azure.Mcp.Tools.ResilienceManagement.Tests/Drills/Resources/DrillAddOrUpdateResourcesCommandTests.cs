// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills.Resources;

public sealed class DrillAddOrUpdateResourcesCommandTests
    : CommandUnitTestsBase<DrillAddOrUpdateResourcesCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";
    private const string DrillResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/drills/drill1/drillResources/22222222-2222-2222-2222-222222222222";
    private const string IncludeJson = "[{\"id\":\"" + DrillResourceId + "\"}]";

    [Theory]
    [InlineData("--include-resources", "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1")]
    [InlineData("--update-resources", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1")]
    [InlineData("--include-resources", "/providers/Microsoft.Management/serviceGroups/other/providers/Microsoft.AzureResilienceManagement/drills/drill1/drillResources/22222222-2222-2222-2222-222222222222")]
    [InlineData("--update-resources", "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/drills/other/drillResources/22222222-2222-2222-2222-222222222222")]
    [InlineData("--include-resources", "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/drills/drill1/drillResources/resource1")]
    [InlineData("--include-resources", "not-an-arm-id")]
    public async Task ExecuteAsync_RejectsWrongResourceIdentifiers(string option, string id)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup, "--drill", Drill, "--fault-duration-minutes", "1",
            option, $"[{{\"id\":\"{id}\"}}]");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("drill-resource IDs", response.Message);
        await Service.DidNotReceiveWithAnyArgs().AddOrUpdateDrillResourcesAsync(default!, default!, default!, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("[\"/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1\"]")]
    [InlineData("[\"/providers/Microsoft.Management/serviceGroups/other/providers/Microsoft.AzureResilienceManagement/drills/drill1/drillResources/22222222-2222-2222-2222-222222222222\"]")]
    [InlineData("[\"/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/drills/other/drillResources/22222222-2222-2222-2222-222222222222\"]")]
    [InlineData("[\"/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/drills/drill1/drillResources/resource1\"]")]
    [InlineData("[\"/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/22222222-2222-2222-2222-222222222222\"]")]
    [InlineData("[\"not-an-arm-id\"]")]
    [InlineData("[\"\"]")]
    [InlineData("[42]")]
    public async Task ExecuteAsync_RejectsWrongExcludeIdentifiers(string excludeJson)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup, "--drill", Drill, "--fault-duration-minutes", "1",
            "--exclude-resources", excludeJson);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("drill-resource ID", response.Message);
        await Service.DidNotReceiveWithAnyArgs().AddOrUpdateDrillResourcesAsync(default!, default!, default!, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsExcludedDrillResourceIdsAndPreservesThem()
    {
        const string excludedId = "/providers/Microsoft.Management/serviceGroups/SG1/providers/Microsoft.AzureResilienceManagement/drills/DRILL1/drillResources/33333333-3333-3333-3333-333333333333";
        Service.AddOrUpdateDrillResourcesAsync(ServiceGroup, Drill, Arg.Any<AddOrUpdateResourcesContent>(), null, Arg.Any<CancellationToken>())
            .Returns(new DrillAddOrUpdateResourcesResult("operation1", false));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup, "--drill", Drill, "--fault-duration-minutes", "1",
            "--exclude-resources", $"[\"{excludedId}\"]");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).AddOrUpdateDrillResourcesAsync(
            ServiceGroup,
            Drill,
            Arg.Is<AddOrUpdateResourcesContent>(content =>
                content.ResourceLists.ExcludeResources.Count == 1 &&
                content.ResourceLists.ExcludeResources[0].ToString() == excludedId),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("add-or-update", command.Name);
        Assert.Contains("resources", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("--drill drill1 --fault-duration-minutes 10 --include-resources " + IncludeJson, false)]
    [InlineData("--service-group sg1 --fault-duration-minutes 10 --include-resources " + IncludeJson, false)]
    [InlineData("--service-group sg1 --drill drill1 --include-resources " + IncludeJson, false)]
    [InlineData("--service-group sg1 --drill drill1 --fault-duration-minutes 10", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.AddOrUpdateDrillResourcesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AddOrUpdateResourcesContent>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new DrillAddOrUpdateResourcesResult("operation1", false));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_StartsOperationAndReturnsOperationId()
    {
        Service.AddOrUpdateDrillResourcesAsync(
            ServiceGroup,
            Drill,
            Arg.Any<AddOrUpdateResourcesContent>(),
            "tenant1",
            Arg.Any<CancellationToken>())
            .Returns(new DrillAddOrUpdateResourcesResult("operation1", false));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--fault-duration-minutes", "10",
            "--include-resources", IncludeJson,
            "--tenant", "tenant1");

        var result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.DrillAddOrUpdateResourcesCommandResult);
        Assert.Equal("operation1", result.Result.OperationId);
        Assert.False(result.Result.HasCompleted);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresAtLeastOneResourceList()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--fault-duration-minutes", "10");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("at least one", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsNonPositiveFaultDuration()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--fault-duration-minutes", "0",
            "--include-resources", IncludeJson);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("greater than zero", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsIncludeResourceWithoutId()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--fault-duration-minutes", "10",
            "--include-resources", "[{\"faultProperties\":{}}]");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("id", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidForceValue()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--fault-duration-minutes", "10",
            "--include-resources", IncludeJson,
            "--force-inclusion-and-update", "Maybe");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Enable or Disable", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("--service-group", "sg/1")]
    [InlineData("--drill", "drill/1")]
    public async Task ExecuteAsync_RejectsInvalidPathSegments(string invalidOption, string invalidValue)
    {
        string serviceGroup = invalidOption == "--service-group" ? invalidValue : ServiceGroup;
        string drill = invalidOption == "--drill" ? invalidValue : Drill;

        var response = await ExecuteCommandAsync(
            "--service-group", serviceGroup,
            "--drill", drill,
            "--fault-duration-minutes", "10",
            "--include-resources", IncludeJson);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("single non-empty path segment", response.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "another drill operation")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "add or update failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.AddOrUpdateDrillResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AddOrUpdateResourcesContent>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--fault-duration-minutes", "10",
            "--include-resources", IncludeJson);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }
}
