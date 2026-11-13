// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

public sealed partial class ResilienceManagementServiceTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetGoalResource_PreservesRegionalParticipationWithoutInventingMissingData(bool includeRegional)
    {
        using var handler = new GoalResourceContractHandler(includeRegional);
        var service = CreateService(handler);
        GoalResourceInfo result = await service.GetGoalResourceAsync(
            "sg1", "goal1", GoalResourceContractHandler.ResourceName,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(GoalResourceContractHandler.ResourceId, result.Id);
        Assert.Equal("Excluded", result.Properties!.ZonalResiliency!.GoalParticipation);
        Assert.Equal(includeRegional, result.Properties.RegionalResiliency.HasValue);
        using JsonDocument serialized = JsonDocument.Parse(JsonSerializer.Serialize(
            result, ResilienceManagementJsonContext.Default.GoalResourceInfo));
        JsonElement properties = serialized.RootElement.GetProperty("properties");
        if (includeRegional)
        {
            Assert.Equal("Included", properties.GetProperty("regionalResiliency")
                .GetProperty("goalParticipation").GetString());
        }
        else
        {
            Assert.True(!properties.TryGetProperty("regionalResiliency", out JsonElement regional) ||
                regional.ValueKind == JsonValueKind.Null);
        }

        Uri request = Assert.Single(handler.Requests);
        Assert.Equal(GoalResourceContractHandler.ResourceId, request.AbsolutePath);
        Assert.Equal("2026-10-31-preview",
            System.Web.HttpUtility.ParseQueryString(request.Query)["api-version"]);
    }
}
