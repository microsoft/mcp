// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore fchild

using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public sealed class GoalAssignmentValidationTests
{
    private const string ResourceId = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1";
    private const string ServiceLevelResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.Monitor/slis/sli1";

    public static TheoryData<string> InvalidPayloads => new()
    {
        "",
        " ",
        "{",
        "null",
        "{}",
        "[]",
        "[null]",
        "[1]",
        "[[]]",
        "[{}]",
        """[{"properties":null}]""",
        """[{"properties":[]}]""",
        """[{"properties":{"resourceArmId":null,"zonalResiliency":{"goalParticipation":"Included"}}}]""",
        """[{"properties":{"resourceArmId":7,"zonalResiliency":{"goalParticipation":"Included"}}}]""",
        """[{"properties":{"resourceArmId":"","zonalResiliency":{"goalParticipation":"Included"}}}]""",
        """[{"properties":{"resourceArmId":"invalid","zonalResiliency":{"goalParticipation":"Included"}}}]""",
        Wrap("""null"""),
        Wrap("""[]"""),
        Wrap("""{}"""),
        Wrap("""{"goalParticipation":null}"""),
        Wrap("""{"goalParticipation":true}"""),
        Wrap("""{"goalParticipation":"included"}"""),
        Wrap("""{"goalParticipation":"Unknown"}"""),
        Wrap("""{"attestationStatus":null}"""),
        Wrap("""{"attestationStatus":"Attested"}"""),
        Wrap("""{"goalParticipation":"Included","goalParticipation":"Excluded"}"""),
        Wrap("""{"goalParticipation":"Included","unknown":true}"""),
        Wrap("""{"userConfirmation":null}"""),
        Wrap("""{"userConfirmation":{}}"""),
        Wrap("""{"userConfirmation":[]}"""),
        Wrap("""{"userConfirmation":[null]}"""),
        Wrap("""{"userConfirmation":[{}]}"""),
        Wrap("""{"userConfirmation":[{"solutionDisplayName":"Unknown","confirmationStatus":"ApprovedByUser"}]}"""),
        Wrap("""{"userConfirmation":[{"solutionDisplayName":"ZonePinnedVmWithZrsDisk","confirmationStatus":"Approved"}]}"""),
        Wrap("""{"userConfirmation":[{"solutionDisplayName":"ZonePinnedVmWithZrsDisk","confirmationStatus":null}]}"""),
        Wrap("""{"userConfirmation":[{"solutionDisplayName":"ZonePinnedVmWithZrsDisk","confirmationStatus":"ApprovedByUser","reasonForRequestingConfirmation":null}]}"""),
        Wrap("""{"userConfirmation":[{"solutionDisplayName":"ZonePinnedVmWithZrsDisk","confirmationStatus":"ApprovedByUser","reasonForRequestingConfirmation":"Unknown"}]}"""),
        Wrap("""{"userConfirmation":[{"solutionDisplayName":"ZonePinnedVmWithZrsDisk","confirmationStatus":"ApprovedByUser","unknown":false}]}"""),
        Wrap("""{"userConfirmation":[{"solutionDisplayName":"ZonePinnedVmWithZrsDisk","confirmationStatus":"ApprovedByUser","confirmationStatus":"RejectedByUser"}]}"""),
        Wrap("""{"userConfirmation":[{"solutionDisplayName":"ZonePinnedVmWithZrsDisk","confirmationStatus":"ApprovedByUser"},{"solutionDisplayName":"ZonePinnedVmWithZrsDisk","confirmationStatus":"RejectedByUser"}]}"""),
        $$"""[{"unknown":true,"properties":{"resourceArmId":"{{ResourceId}}","zonalResiliency":{"goalParticipation":"Included"} } }]""",
        $$"""[{"properties":{"resourceArmId":"{{ResourceId}}","unknown":true,"zonalResiliency":{"goalParticipation":"Included"} } }]""",
        $$"""[{"properties":{"resourceArmId":"{{ResourceId}}","resourceArmId":"{{ResourceId}}","zonalResiliency":{"goalParticipation":"Included"} } }]""",
        $$"""[{"properties":{"resourceArmId":"{{ResourceId}}","zonalResiliency":{"goalParticipation":"Included"} },"properties":{} }]""",
        $$"""[{"properties":{"resourceArmId":"{{ResourceId}}","zonalResiliency":{"goalParticipation":"Included"},"zonalResiliency":{} } }]""",
        $$"""[{"properties":{"resourceArmId":"{{ResourceId}}","zonalResiliency":{"goalParticipation":"Included"} } },{"properties":{"resourceArmId":"{{ResourceId.ToUpperInvariant()}}","zonalResiliency":{"goalParticipation":"Excluded"} } }]"""
    };

    [Theory]
    [MemberData(nameof(InvalidPayloads))]
    public void CreateResourcesContent_RejectsMalformedOrNonGaPayload(string payload) =>
        Assert.Throws<ArgumentException>(() => GoalAssignmentValidation.CreateResourcesContent(payload));

    [Theory]
    [InlineData("Included", "NotAttested")]
    [InlineData("Excluded", "ManuallyAttested")]
    public void CreateResourcesContent_PreservesParticipationAndAttestation(string participation, string attestation)
    {
        var content = GoalAssignmentValidation.CreateResourcesContent(
            Wrap($$"""{"goalParticipation":"{{participation}}","attestationStatus":"{{attestation}}"}"""));

        using var document = JsonDocument.Parse(ModelReaderWriter.Write(content, new ModelReaderWriterOptions("W")).ToString());
        var resources = document.RootElement.GetProperty("resources");
        Assert.Equal(1, resources.GetArrayLength());
        var properties = resources[0].GetProperty("properties");
        Assert.Equal(ResourceId, properties.GetProperty("resourceArmId").GetString());
        var zonal = properties.GetProperty("zonalResiliency");
        Assert.Equal(participation, zonal.GetProperty("goalParticipation").GetString());
        Assert.Equal(attestation, zonal.GetProperty("attestationStatus").GetString());
    }

    [Theory]
    [InlineData("ZonePinnedVmWithZrsDisk", "ApprovedByUser", "ZonePinnedZrsDataDisksConditional")]
    [InlineData("VmInMultiZoneVmss", "ApprovalPending", "VmInMultiZoneScaleSetStatelessOnly")]
    [InlineData("ZonePinnedVmWithZrsDisk", "ApprovalNotNeeded", null)]
    [InlineData("VmInMultiZoneVmss", "RejectedByUser", null)]
    public void CreateResourcesContent_PreservesConfirmationWireValues(string solution, string status, string? reason)
    {
        string reasonProperty = reason is null ? "" : $",\"reasonForRequestingConfirmation\":\"{reason}\"";
        string payload = Wrap($$"""{"userConfirmation":[{"solutionDisplayName":"{{solution}}","confirmationStatus":"{{status}}"{{reasonProperty}}}]}""");

        var content = GoalAssignmentValidation.CreateResourcesContent(payload);

        using var document = JsonDocument.Parse(ModelReaderWriter.Write(content, new ModelReaderWriterOptions("W")).ToString());
        var confirmation = document.RootElement.GetProperty("resources")[0].GetProperty("properties")
            .GetProperty("zonalResiliency").GetProperty("userConfirmation")[0];
        Assert.Equal(solution, confirmation.GetProperty("solutionDisplayName").GetString());
        Assert.Equal(status, confirmation.GetProperty("confirmationStatus").GetString());
        if (reason is not null)
        {
            Assert.Equal(reason, confirmation.GetProperty("reasonForRequestingConfirmation").GetString());
        }
        else
        {
            Assert.False(confirmation.TryGetProperty("reasonForRequestingConfirmation", out _));
        }
    }

    [Fact]
    public void CreateResourcesContent_EnforcesExactUtf8ByteLimit()
    {
        string payload = Wrap("""{"goalParticipation":"Included"}""");
        string atLimit = payload + new string(' ', 1_048_576 - Encoding.UTF8.GetByteCount(payload));

        Assert.NotNull(GoalAssignmentValidation.CreateResourcesContent(atLimit));
        Assert.Throws<ArgumentException>(() => GoalAssignmentValidation.CreateResourcesContent(atLimit + " "));
        Assert.Throws<ArgumentException>(() => GoalAssignmentValidation.CreateResourcesContent(
            payload.Replace("vm1", new string('界', 350_000), StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-an-arm-id")]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111")]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg")]
    [InlineData(ResourceId + "?query=value")]
    [InlineData(ResourceId + "#fragment")]
    [InlineData(ResourceId + "%2fchild")]
    [InlineData(ResourceId + " bad")]
    public void ValidateResourceIds_RejectsInvalidEntries(string? id)
    {
        var validation = new ValidationResult();

        GoalAssignmentValidation.ValidateResourceIds([id!], validation);

        Assert.NotEmpty(validation.Errors);
    }

    [Fact]
    public void ValidateResourceIds_RejectsCaseInsensitiveDuplicates()
    {
        var validation = new ValidationResult();

        GoalAssignmentValidation.ValidateResourceIds([ResourceId, ResourceId.ToUpperInvariant()], validation);

        Assert.NotEmpty(validation.Errors);
    }

    [Fact]
    public void ValidateResourceIds_AllowsOmittedEmptyAndValidLists()
    {
        foreach (IReadOnlyList<string>? ids in new IReadOnlyList<string>?[] { null, [], [ResourceId] })
        {
            var validation = new ValidationResult();
            GoalAssignmentValidation.ValidateResourceIds(ids, validation);
            Assert.Empty(validation.Errors);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData(ResourceId)]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg2/providers/Microsoft.Monitor/slis/sli1")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.Monitor/other/sli1")]
    [InlineData("/providers/Microsoft.Monitor/slis/sli1")]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/sg1/providers/Microsoft.Monitor/slis/sli1")]
    [InlineData(ServiceLevelResourceId + "/children/child1")]
    [InlineData(ServiceLevelResourceId + "?query=value")]
    [InlineData(ServiceLevelResourceId + "#fragment")]
    [InlineData(ServiceLevelResourceId + "%2fchild")]
    public void ValidateServiceLevelResourceIds_RejectsInvalidOrWrongScopeOrType(string? id)
    {
        var validation = new ValidationResult();

        GoalAssignmentValidation.ValidateServiceLevelResourceIds([id!], "sg1", validation);

        Assert.NotEmpty(validation.Errors);
    }

    [Fact]
    public void ValidateServiceLevelResourceIds_RejectsCaseInsensitiveDuplicates()
    {
        var validation = new ValidationResult();

        GoalAssignmentValidation.ValidateServiceLevelResourceIds(
            [ServiceLevelResourceId, ServiceLevelResourceId.ToUpperInvariant()], "sg1", validation);

        Assert.NotEmpty(validation.Errors);
    }

    [Fact]
    public void ValidateServiceLevelResourceIds_AllowsOmittedEmptyAndSameGroupIndicators()
    {
        foreach (IReadOnlyList<string>? ids in new IReadOnlyList<string>?[]
        {
            null,
            [],
            [ServiceLevelResourceId],
            [ServiceLevelResourceId, ServiceLevelResourceId + "2"],
            [ServiceLevelResourceId.ToUpperInvariant()]
        })
        {
            var validation = new ValidationResult();

            GoalAssignmentValidation.ValidateServiceLevelResourceIds(ids, "sg1", validation);

            Assert.Empty(validation.Errors);
        }
    }

    private static string Wrap(string zonal) =>
        $$"""[{"properties":{"resourceArmId":"{{ResourceId}}","zonalResiliency":{{zonal}} } }]""";
}
