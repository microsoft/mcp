// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.ResilienceManagement;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals;

public class GoalResponseMappingTests
{
    [Theory]
    [InlineData("""{"goalAssignmentType":"Resiliency","goalTemplateId":"/legacy","provisioningState":"Succeeded"}""", null)]
    [InlineData("""{"requireZonalResiliency":false,"provisioningState":"Succeeded"}""", false)]
    [InlineData("""{"requireZonalResiliency":true,"provisioningState":"Succeeded"}""", true)]
    public void AssignmentMapping_PreservesPresenceAndLegacyFields(string properties, bool? expected)
    {
        var content = BinaryData.FromString($$"""{"properties":{{properties}}}""");
        var reader = new Utf8JsonReader(content.ToMemory().Span);
        var data = ((IJsonModel<GoalAssignmentData>)new GoalAssignmentData()).Create(ref reader, new ModelReaderWriterOptions("W"));
        Assert.NotNull(data);
        var mapped = ResilienceManagementService.MapGoalAssignment(data, content);

        Assert.Equal(expected, mapped.Properties!.RequireZonalResiliency);
        if (expected is null)
        {
            Assert.Equal("Resiliency", mapped.Properties.GoalAssignmentType);
            Assert.Equal("/legacy", mapped.Properties.GoalTemplateId);
        }
    }

    [Fact]
    public void MemberMapping_PreservesLegacyFieldsLostBySdkWireRead()
    {
        var content = BinaryData.FromString("""
            {"properties":{
              "resourceArmId":"/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/group/providers/Microsoft.Storage/storageAccounts/account",
              "highAvailabilityGoalParticipation":"Excluded",
              "disasterRecoveryAttestationStatus":"NotAttested",
              "serviceGroupMemberships":[{"membershipType":"Direct","serviceGroupId":"/providers/Microsoft.Management/serviceGroups/group"}]
            }}
            """);
        var reader = new Utf8JsonReader(content.ToMemory().Span);
        var data = ((IJsonModel<GoalMembersData>)new GoalMembersData()).Create(ref reader, new ModelReaderWriterOptions("W"));
        Assert.NotNull(data);
        var mapped = ResilienceManagementService.MapGoalResource(data, content);

        Assert.Equal("Excluded", mapped.Properties!.HighAvailabilityGoalParticipation);
        Assert.Equal("NotAttested", mapped.Properties.DisasterRecoveryAttestationStatus);
        Assert.Single(mapped.Properties.ServiceGroupMemberships!);
        Assert.Null(mapped.Properties.ZonalResiliency);
    }
}
