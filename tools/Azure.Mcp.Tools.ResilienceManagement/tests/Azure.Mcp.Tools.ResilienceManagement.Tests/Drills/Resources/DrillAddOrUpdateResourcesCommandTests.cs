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
    private const string IncludeJson = "[{\"id\":\"/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1\"}]";

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
                Arg.Any<RetryPolicyOptions?>(),
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
            Arg.Any<RetryPolicyOptions?>(),
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
            Arg.Any<RetryPolicyOptions?>(),
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
