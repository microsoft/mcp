// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:words slis

using System.ClientModel.Primitives;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills;

public sealed class DrillConfigurationTests
{
    [Fact]
    public void Create_EnforcesExactPayloadLimit()
    {
        const int limit = 1024 * 1024;
        const string goal = """{"identity":{"type":"SystemAssigned"}}""";
        string boundary = goal.PadRight(limit);

        Assert.NotNull(DrillConfiguration.Create(DrillKind.Zonal, boundary, null, null).GoalAssignmentProperties);
        ArgumentException error = Assert.Throws<ArgumentException>(() =>
            DrillConfiguration.Create(DrillKind.Zonal, boundary + " ", null, null));
        Assert.Contains("must not exceed 1 MB", error.Message);
    }

    [Fact]
    public void Update_AcceptsTwoDistinctSliSelections()
    {
        const string prefix = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg/providers/Microsoft.Monitor/slis/";
        string payload = $$"""
            {"slis":[{"sliId":"{{prefix}}availability","type":"Availability"},{"sliId":"{{prefix}}latency","type":"Latency"}]}
            """;
        var properties = DrillConfiguration.Update(null, null, payload);

        using JsonDocument document = JsonDocument.Parse(ModelReaderWriter.Write(properties, new ModelReaderWriterOptions("W")));
        Assert.Equal("sliMonitoringProperties", Assert.Single(document.RootElement.EnumerateObject()).Name);
        Assert.Equal(2, document.RootElement.GetProperty("sliMonitoringProperties").GetProperty("slis").GetArrayLength());
    }

    [Theory]
    [InlineData("null", null, "healthModelMonitoringProperties")]
    [InlineData(null, "null", "sliMonitoringProperties")]
    public void Update_SerializesOnlyExplicitMonitoringClear(string? healthModel, string? sli, string expectedProperty)
    {
        var properties = DrillConfiguration.Update(null, healthModel, sli);

        using JsonDocument document = JsonDocument.Parse(ModelReaderWriter.Write(properties, new ModelReaderWriterOptions("W")));
        JsonProperty property = Assert.Single(document.RootElement.EnumerateObject());
        Assert.Equal(expectedProperty, property.Name);
        Assert.Equal(JsonValueKind.Null, property.Value.ValueKind);
    }

    [Fact]
    public void Create_AcceptsServiceGroupScopedSli()
    {
        const string id = "/providers/Microsoft.Management/serviceGroups/fixture/providers/Microsoft.Monitor/slis/availability";
        string payload = $$"""
            {"identity":{"type":"SystemAssigned"},"slis":[{"sliId":"{{id}}","type":"Availability"}]}
            """;
        var properties = DrillConfiguration.Create(DrillKind.Zonal, null, null, payload);

        using JsonDocument document = JsonDocument.Parse(ModelReaderWriter.Write(properties, new ModelReaderWriterOptions("W")));
        Assert.Equal(id, document.RootElement.GetProperty("sliMonitoringProperties").GetProperty("slis")[0].GetProperty("sliId").GetString());
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("{")]
    [InlineData("""{"identity":{"type":"Unknown"}}""")]
    [InlineData("""{"identity":{"type":"UserAssigned"}}""")]
    [InlineData("""{"identity":{"type":"SystemAssigned"},"goalAssignmentId":"readonly"}""")]
    [InlineData("""{"identity":{"type":"SystemAssigned"},"identity":{"type":"None"}}""")]
    public void Create_RejectsInvalidGoalConfiguration(string payload)
    {
        Assert.Throws<ArgumentException>(() => DrillConfiguration.Create(DrillKind.Zonal, payload, null, null));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("""{"identity":{"type":"SystemAssigned"}}""")]
    [InlineData("""{"identity":{"type":"SystemAssigned"},"healthModelId":"not-an-arm-id"}""")]
    [InlineData("""{"healthModelId":"/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg/providers/Microsoft.CloudHealth/healthmodels/health"}""")]
    public void Create_RejectsInvalidHealthModelConfiguration(string payload)
    {
        Assert.Throws<ArgumentException>(() => DrillConfiguration.Create(DrillKind.Zonal, null, payload, null));
    }

    [Theory]
    [InlineData("""{"slis":[]}""")]
    [InlineData("""{"slis":[{}]}""")]
    [InlineData("""{"slis":[{"sliId":"bad","type":"Availability"}]}""")]
    [InlineData("""{"slis":[{}, {}, {}]}""")]
    [InlineData("""{"slis":null}""")]
    [InlineData("""{"identity":null}""")]
    public void Update_RejectsInvalidSliConfiguration(string payload)
    {
        Assert.Throws<ArgumentException>(() => DrillConfiguration.Update(null, null, payload));
    }

    [Theory]
    [InlineData("sli-one", "Availability", "sli-two", "Availability")]
    [InlineData("sli-one", "Availability", "SLI-ONE", "Latency")]
    [InlineData("sli-one", "Other", "sli-two", "Latency")]
    public void Update_RejectsDuplicateOrInvalidSliSelections(string firstId, string firstType, string secondId, string secondType)
    {
        const string prefix = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg/providers/Microsoft.Monitor/slis/";
        string payload = $$"""
            {"slis":[{"sliId":"{{prefix + firstId}}","type":"{{firstType}}"},{"sliId":"{{prefix + secondId}}","type":"{{secondType}}"}]}
            """;
        Assert.Throws<ArgumentException>(() => DrillConfiguration.Update(null, null, payload));
    }

    [Fact]
    public void Update_RejectsClearingGoalAssignmentProperties()
    {
        Assert.Throws<ArgumentException>(() => DrillConfiguration.Update("null", null, null));
    }

    private const string IdentityId = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id1";

    [Theory]
    [InlineData("SystemAssigned")]
    [InlineData("None")]
    public void Update_RejectsUserAssignedIdentityWhenTypeExcludesUserAssigned(string type)
    {
        string identity = IdentityJson(type);

        var ex1 = Assert.Throws<ArgumentException>(() => DrillConfiguration.Update(null, null, null, chaos: identity));
        var ex2 = Assert.Throws<ArgumentException>(() => DrillConfiguration.Update(null, null, null, recoveryPlan: identity));
        var ex3 = Assert.Throws<ArgumentException>(() => DrillConfiguration.Update(null, null, null, monitoring: identity));
        Assert.All([ex1, ex2, ex3], ex => Assert.Contains("only when identity type includes UserAssigned", ex.Message));
    }

    [Theory]
    [InlineData("SystemAssigned")]
    [InlineData("None")]
    public void Create_RejectsUserAssignedIdentityWhenTypeExcludesUserAssigned(string type)
    {
        string identity = IdentityJson(type);
        string chaos = IdentityJson(type, ",\"chaosResourceIdentityForFaults\":{\"type\":\"SystemAssigned\"}");

        Assert.Throws<ArgumentException>(() => DrillConfiguration.Create(DrillKind.Zonal, null, null, null, chaos: chaos));
        Assert.Throws<ArgumentException>(() => DrillConfiguration.Create(DrillKind.Zonal, null, null, null, recoveryPlan: identity));
        Assert.Throws<ArgumentException>(() => DrillConfiguration.Create(DrillKind.Zonal, null, null, null, monitoring: identity));
    }

    [Fact]
    public void Update_RejectsUserAssignedIdentityOnChaosFaultIdentity()
    {
        string chaos = "{\"chaosResourceIdentityForFaults\":{\"type\":\"SystemAssigned\",\"userAssignedIdentity\":\"" + IdentityId + "\"}}";

        Assert.Throws<ArgumentException>(() => DrillConfiguration.Update(null, null, null, chaos: chaos));
    }

    [Theory]
    [InlineData("UserAssigned")]
    [InlineData("SystemAssigned, UserAssigned")]
    public void Update_AcceptsUserAssignedIdentityWhenTypeIncludesUserAssigned(string type)
    {
        string identity = IdentityJson(type);

        Assert.NotNull(DrillConfiguration.Update(null, null, null, monitoring: identity));
    }

    private static string IdentityJson(string type, string extraProperties = "")
        => "{\"identity\":{\"type\":\"" + type + "\",\"userAssignedIdentity\":\"" + IdentityId + "\"}" + extraProperties + "}";

    [Fact]
    public void Create_AcceptsUserAssignedIdentity()
    {
        const string id = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/identity";
        string goal = $$"""{"identity":{"type":"UserAssigned","userAssignedIdentity":"{{id}}"} }""";

        var properties = DrillConfiguration.Create(DrillKind.Zonal, goal, null, null);

        Assert.Equal(id, properties.GoalAssignmentProperties.Identity.UserAssignedIdentity.ToString());
    }
}
