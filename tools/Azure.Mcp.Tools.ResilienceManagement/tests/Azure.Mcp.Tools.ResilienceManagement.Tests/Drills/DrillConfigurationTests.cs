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

    [Fact]
    public void Create_AcceptsUserAssignedIdentity()
    {
        const string id = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/identity";
        string goal = $$"""{"identity":{"type":"UserAssigned","userAssignedIdentity":"{{id}}"} }""";

        var properties = DrillConfiguration.Create(DrillKind.Zonal, goal, null, null);

        Assert.Equal(id, properties.GoalAssignmentProperties.Identity.UserAssignedIdentity.ToString());
    }
}
