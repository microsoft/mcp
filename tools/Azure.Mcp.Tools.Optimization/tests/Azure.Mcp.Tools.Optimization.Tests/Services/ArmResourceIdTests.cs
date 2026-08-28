// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Optimization.Services;
using Xunit;

namespace Azure.Mcp.Tools.Optimization.Tests.Services;

public class ArmResourceIdTests
{
    private const string ResourceId =
        "/subscriptions/be97033d-a8d6-4d02-a9b7-8c2d253ea34b/resourcegroups/contoso-demo/providers/microsoft.compute/virtualmachines/contoso-demo-vm-001";

    [Theory]
    [InlineData(ResourceId, ResourceId)]
    [InlineData(ResourceId + "/providers/microsoft.advisor", ResourceId)]
    [InlineData(ResourceId + "/providers/Microsoft.Advisor", ResourceId)]
    [InlineData(ResourceId + "/providers/microsoft.advisor/recommendations/11111111-1111-1111-1111-111111111111", ResourceId)]
    public void StripAdvisorRecommendationSuffix_RemovesAdvisorSegment(string input, string expected)
    {
        Assert.Equal(expected, ArmResourceId.StripAdvisorRecommendationSuffix(input));
    }

    [Fact]
    public void StripAdvisorRecommendationSuffix_DoesNotMatchSimilarNamespace()
    {
        var input = ResourceId + "/providers/microsoft.advisorx/things/thing1";
        Assert.Equal(input, ArmResourceId.StripAdvisorRecommendationSuffix(input));
    }
}
