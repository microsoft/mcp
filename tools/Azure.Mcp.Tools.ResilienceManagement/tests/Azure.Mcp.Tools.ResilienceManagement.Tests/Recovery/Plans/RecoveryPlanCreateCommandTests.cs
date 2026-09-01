// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Plans;

public sealed class RecoveryPlanCreateCommandTests : CommandUnitTestsBase<RecoveryPlanCreateCommand, IResilienceManagementService>
{
    private const string UserAssignedIdentityResourceId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/testIdentity";
    private const string ValidArgs = "--service-group sg1 --recoveryplan plan1 --plan-type Zonal --plan-description description --identity-type UserAssigned --user-assigned-identity " + UserAssignedIdentityResourceId + " --default-group-description default";

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
    [InlineData("--service-group sg1 --recoveryplan plan1 --plan-type Zonal --plan-description description --identity-type SystemAssigned", true)]
    [InlineData("--service-group sg1 --recoveryplan plan1 --plan-type Zonal --identity-type SystemAssigned", true)]
    [InlineData("--service-group sg1 --recoveryplan plan1 --plan-type Zonal --plan-description description", false)]
    [InlineData("--recoveryplan plan1 --plan-type Zonal --plan-description description --default-group-description default", false)]
    [InlineData("--service-group sg1 --plan-type Zonal --plan-description description --default-group-description default", false)]
    [InlineData("--service-group sg1 --recoveryplan plan1 --plan-description description --default-group-description default", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CreateRecoveryPlanAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RecoveryPlanKind>(),
                Arg.Any<string?>(),
                Arg.Any<RecoveryPlanIdentityKind>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Element("plan1"));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReportsMissingIdentityType()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("Missing Required options: --identity-type", response.Message);
        await Service.DidNotReceive().CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanIdentityKind>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            cancellationToken: TestContext.Current.CancellationToken);
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
            "--recoveryplan", recoveryPlan,
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--default-group-description", "default");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("5 to 24 characters", response.Message);
        await Service.DidNotReceive().CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanIdentityKind>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidServiceGroupName()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "../sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--default-group-description", "default");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("service group name", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceive().CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanIdentityKind>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("plan1")]
    [InlineData("123456789012345678901234")]
    public async Task ExecuteAsync_AcceptsRecoveryPlanNameBoundaryLengths(string recoveryPlan)
    {
        Service.CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            recoveryPlan,
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanIdentityKind>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Element(recoveryPlan));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", recoveryPlan,
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("four")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task ExecuteAsync_RejectsPlanDescriptionOutsideAllowedLength(string planDescription)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", planDescription,
            "--identity-type", "SystemAssigned",
            "--default-group-description", "default");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("5 to 50 characters", response.Message);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("12345678901234567890123456789012345678901234567890")]
    public async Task ExecuteAsync_AcceptsPlanDescriptionBoundaryLengths(string planDescription)
    {
        Service.CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            planDescription,
            Arg.Any<RecoveryPlanIdentityKind>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", planDescription,
            "--identity-type", "SystemAssigned");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("four")]
    [InlineData("     ")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task ExecuteAsync_RejectsDefaultGroupDescriptionOutsideAllowedLength(string defaultGroupDescription)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--default-group-description", defaultGroupDescription);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("default recovery group description must be 5 to 50 characters", response.Message);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("12345678901234567890123456789012345678901234567890")]
    public async Task ExecuteAsync_AcceptsDefaultGroupDescriptionBoundaryLengths(string defaultGroupDescription)
    {
        Service.CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanIdentityKind>(),
            Arg.Any<string?>(),
            defaultGroupDescription,
            Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--default-group-description", defaultGroupDescription);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUnsupportedRegionalPlanTypeDuringBinding()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Regional",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--default-group-description", "default");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid --plan-type 'Regional'", response.Message);
        Assert.Contains("Zonal", response.Message);
        await Service.DidNotReceive().CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanIdentityKind>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRecoveryPlanAndForwardsCompletePutOptions()
    {
        Service.CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.UserAssigned,
            UserAssignedIdentityResourceId,
            "default",
            null,
            null,
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(ValidArgs);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryPlanCreateCommandResult);
        Assert.Equal("plan1", result.RecoveryPlan.Name);
        await Service.Received(1).CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.UserAssigned,
            UserAssignedIdentityResourceId,
            "default",
            null,
            null,
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsNullWhenDefaultGroupDescriptionIsOmitted()
    {
        Service.CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.UserAssigned,
            UserAssignedIdentityResourceId,
            null,
            null,
            null,
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "UserAssigned",
            "--user-assigned-identity", UserAssignedIdentityResourceId);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.UserAssigned,
            UserAssignedIdentityResourceId,
            null,
            null,
            null,
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsAdditionalGroups()
    {
        Service.CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.SystemAssigned,
            null,
            null,
            null,
            Arg.Is<IReadOnlyList<RecoveryPlanGroupInput>?>(groups =>
                groups != null &&
                groups.Count == 1 &&
                groups[0].GroupUniqueId == null &&
                groups[0].OrderId == 1 &&
                groups[0].Description == "Second recovery group"),
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--additional-groups", "[{\"orderId\":1,\"description\":\"Second recovery group\"}]");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.SystemAssigned,
            null,
            null,
            null,
            Arg.Is<IReadOnlyList<RecoveryPlanGroupInput>?>(groups => groups != null && groups.Count == 1),
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("{}", "JSON array")]
    [InlineData("[{\"orderId\":2,\"description\":\"Second recovery group\"}]", "sequential starting at 1")]
    [InlineData("[{\"orderId\":1,\"description\":\"four\"}]", "contain 5 to 50 characters")]
    [InlineData("[{\"orderId\":1,\"description\":\"     \"}]", "contain 5 to 50 characters")]
    [InlineData("[{\"orderId\":15,\"description\":\"Fifteenth recovery group\"}]", "between 1 and 14")]
    [InlineData("[{\"orderId\":1,\"description\":\"Second recovery group\",\"groupUniqueId\":\"not-a-guid\"}]", "must be a GUID")]
    public async Task ExecuteAsync_RejectsInvalidAdditionalGroups(string additionalGroups, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--additional-groups", additionalGroups);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsDefaultAndAdditionalGroupActions()
    {
        const string runbookId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Automation/automationAccounts/account/runbooks/runbook";
        Service.CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.SystemAssigned,
            null,
            null,
            null,
            Arg.Is<IReadOnlyList<RecoveryPlanGroupInput>?>(groups =>
                groups != null &&
                groups[0].PreActions != null &&
                groups[0].PreActions![0].Type == RecoveryPlanGroupActionKind.CustomRunbook),
            Arg.Is<IReadOnlyList<RecoveryPlanGroupActionInput>?>(actions =>
                actions != null &&
                actions[0].Type == RecoveryPlanGroupActionKind.ManualAction),
            Arg.Is<IReadOnlyList<RecoveryPlanGroupActionInput>?>(actions => actions != null && actions.Count == 0),
            Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--default-group-pre-actions", "[{\"type\":\"ManualAction\",\"name\":\"Confirm-failover\",\"description\":\"Wait for approval\",\"timeoutInMinutes\":60}]",
            "--default-group-post-actions", "[]",
            "--additional-groups", $"[{{\"orderId\":1,\"description\":\"Second recovery group\",\"preActions\":[{{\"type\":\"CustomRunbook\",\"name\":\"Prepare-database\",\"timeoutInMinutes\":30,\"actionResourceId\":\"{runbookId}\",\"parameters\":{{\"mode\":\"safe\"}}}}]}}]");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("[{\"type\":\"Unknown\",\"name\":\"Action\",\"timeoutInMinutes\":10}]", "ManualAction or CustomRunbook")]
    [InlineData("[{\"type\":\"1\",\"name\":\"Action\",\"timeoutInMinutes\":10}]", "ManualAction or CustomRunbook")]
    [InlineData("[{\"type\":\"ManualAction\",\"name\":\"ab\",\"timeoutInMinutes\":10}]", "3 to 24 character name")]
    [InlineData("[{\"type\":\"ManualAction\",\"name\":\"Invalid name\",\"timeoutInMinutes\":10}]", "only letters, numbers, or hyphens")]
    [InlineData("[{\"type\":\"ManualAction\",\"name\":\"Action\",\"timeoutInMinutes\":0}]", "positive integer")]
    [InlineData("[{\"type\":\"CustomRunbook\",\"name\":\"Action\",\"timeoutInMinutes\":10}]", "requires actionResourceId")]
    [InlineData("[{\"type\":\"CustomRunbook\",\"name\":\"Action\",\"timeoutInMinutes\":10,\"actionResourceId\":\"not-a-resource-id\"}]", "valid Azure resource ID")]
    [InlineData("[{\"type\":\"CustomRunbook\",\"name\":\"Action\",\"timeoutInMinutes\":10,\"actionResourceId\":\"/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account\"}]", "automationAccounts/runbooks")]
    public async Task ExecuteAsync_RejectsInvalidDefaultGroupActions(string actions, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--default-group-pre-actions", actions);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AllowsEmptyActionInstructions()
    {
        Service.CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.SystemAssigned,
            null,
            null,
            null,
            null,
            Arg.Is<IReadOnlyList<RecoveryPlanGroupActionInput>?>(actions => actions != null && actions[0].Description == string.Empty),
            null,
            Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--default-group-pre-actions", "[{\"type\":\"ManualAction\",\"name\":\"Action\",\"description\":\"\",\"timeoutInMinutes\":10}]");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_AllowsNullCustomRunbookParameters()
    {
        const string runbookId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Automation/automationAccounts/account/runbooks/runbook";
        Service.CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.SystemAssigned,
            null,
            null,
            null,
            null,
            Arg.Is<IReadOnlyList<RecoveryPlanGroupActionInput>?>(actions => actions != null && actions[0].Parameters == null),
            null,
            Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--default-group-pre-actions", $"[{{\"type\":\"CustomRunbook\",\"name\":\"Action\",\"timeoutInMinutes\":10,\"actionResourceId\":\"{runbookId}\",\"parameters\":null}}]");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsActionInstructionsOver100Characters()
    {
        string actions = $"[{{\"type\":\"ManualAction\",\"name\":\"Action\",\"description\":\"{new string('a', 101)}\",\"timeoutInMinutes\":10}}]";

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--default-group-pre-actions", actions);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("must not exceed 100 characters", response.Message, StringComparison.OrdinalIgnoreCase);
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
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "UserAssigned",
            "--user-assigned-identity", "/subscriptions/id/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Microsoft.ManagedIdentity/userAssignedIdentities", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceive().CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanIdentityKind>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsNullForSystemAssignedIdentity()
    {
        Service.CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.SystemAssigned,
            null,
            null,
            null,
            null,
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.SystemAssigned,
            null,
            null,
            null,
            null,
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsSystemAndUserAssignedIdentity()
    {
        Service.CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.SystemAndUserAssigned,
            UserAssignedIdentityResourceId,
            null,
            null,
            null,
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAndUserAssigned",
            "--user-assigned-identity", UserAssignedIdentityResourceId);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateRecoveryPlanAsync(
            "sg1",
            "plan1",
            RecoveryPlanKind.Zonal,
            "description",
            RecoveryPlanIdentityKind.SystemAndUserAssigned,
            UserAssignedIdentityResourceId,
            null,
            null,
            null,
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(RecoveryPlanIdentityKind.UserAssigned)]
    [InlineData(RecoveryPlanIdentityKind.SystemAndUserAssigned)]
    public async Task ExecuteAsync_RejectsIdentityTypeWithoutRequiredUserAssignedResourceId(RecoveryPlanIdentityKind identityType)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", identityType.ToString());

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--user-assigned-identity is required", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUserAssignedIdentityForSystemAssignedType()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--plan-type", "Zonal",
            "--plan-description", "description",
            "--identity-type", "SystemAssigned",
            "--user-assigned-identity", UserAssignedIdentityResourceId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not allowed", response.Message);
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
            Arg.Any<RecoveryPlanIdentityKind>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsTimeoutExceptionToGatewayTimeout()
    {
        Service.CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanIdentityKind>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException("Internal timeout details"));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.Status);
        Assert.Contains("timed out", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Internal timeout details", response.Message);
    }

    private static RecoveryPlanInfo Element(string name) => new(
        "id1",
        name,
        "Zonal",
        "description",
        null,
        null,
        new RecoveryPlanIdentityInfo("UserAssigned", [UserAssignedIdentityResourceId]),
        new RecoveryPlanGroupInfo("12345678-9012-3456-7890-123456789012", 0, "default"),
        []);

    private void ConfigureRequestFailure(HttpStatusCode status, string message)
    {
        Service.CreateRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanKind>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryPlanIdentityKind>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, message));
    }
}
