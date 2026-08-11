// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Plans;

public sealed class RecoveryPlanCreateCommandTests : CommandUnitTestsBase<RecoveryPlanCreateCommand, IResilienceManagementService>
{
    private const string UserAssignedIdentityResourceId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/uami";
    private const string ValidArgs = "--service-group sg1 --recovery-plan plan1 --plan-type Zonal --plan-description description --user-assigned-identity " + UserAssignedIdentityResourceId + " --default-group-description default";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData(ValidArgs, true)]
    [InlineData("--service-group sg1 --recovery-plan plan1 --plan-type Zonal --plan-description description", true)]
    [InlineData("--recovery-plan plan1 --plan-type Zonal --plan-description description --default-group-description default", false)]
    [InlineData("--service-group sg1 --plan-type Zonal --plan-description description --default-group-description default", false)]
    [InlineData("--service-group sg1 --recovery-plan plan1 --plan-description description --default-group-description default", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CreateRecoveryPlanAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RecoveryPlanKind>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(Element("plan1"));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [InlineData("plan")]
    [InlineData("1234567890123456789012345")]
    [InlineData("bad_name")]
    [InlineData("../plan")]
    public async Task ExecuteAsync_RejectsInvalidRecoveryPlanName(string recoveryPlan)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", recoveryPlan,
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--default-group-description", "default");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("5 to 24 characters", response.Message);
        await Service.DidNotReceive().CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsPlanDescriptionOver50Characters()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", new string('a', 51),
            "--default-group-description", "default");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("must not exceed 50 characters", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsRegionalPlanType()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--plan-type", "Regional",
            "--plan-description", "description",
            "--default-group-description", "default");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Only Zonal recovery plans are currently supported", response.Message);
        await Service.DidNotReceive().CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRecoveryPlanAndForwardsCompletePutOptions()
    {
        Service.CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            UserAssignedIdentityResourceId,
            "default",
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(ValidArgs);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryPlanCreateCommandResult);
        Assert.Equal("plan1", result.RecoveryPlan.GetProperty("name").GetString());
        await Service.Received(1).CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            UserAssignedIdentityResourceId,
            "default",
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsNullWhenDefaultGroupDescriptionIsOmitted()
    {
        Service.CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            UserAssignedIdentityResourceId,
            null,
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--user-assigned-identity", UserAssignedIdentityResourceId);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            UserAssignedIdentityResourceId,
            null,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictWithoutExposingProviderDetails()
    {
        ConfigureRequestFailure(HttpStatusCode.Conflict, "Provider-specific conflict details");

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("conflicts with the current resource state", response.Message);
        Assert.DoesNotContain("Provider-specific conflict details", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsMalformedUserAssignedIdentityResourceId()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--user-assigned-identity", "/subscriptions/id/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Microsoft.ManagedIdentity/userAssignedIdentities", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceive().CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsNullToCreateSystemAssignedIdentityWhenUserAssignedIdentityIsOmitted()
    {
        Service.CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            null,
            null,
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            null,
            null,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "Service group not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        ConfigureRequestFailure(status, providerDetails);

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Test error", response.Message);
    }

    private static JsonElement Element(string name)
        => JsonDocument.Parse($"{{\"id\":\"id1\",\"name\":\"{name}\"}}").RootElement.Clone();

    private void ConfigureRequestFailure(HttpStatusCode status, string message)
    {
        Service.CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, message));
    }
}