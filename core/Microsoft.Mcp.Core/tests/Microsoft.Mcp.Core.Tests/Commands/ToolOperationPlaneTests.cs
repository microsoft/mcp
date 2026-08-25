// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

public sealed class ToolOperationPlaneTests
{
    [Theory]
    [InlineData(ToolOperationPlane.Unspecified, "unspecified")]
    [InlineData(ToolOperationPlane.Data, "data")]
    [InlineData(ToolOperationPlane.Control, "control")]
    [InlineData(ToolOperationPlane.Both, "both")]
    [InlineData(ToolOperationPlane.NotApplicable, "notApplicable")]
    public void ToJsonValue_ReturnsStableValue(ToolOperationPlane operationPlane, string expected)
    {
        Assert.Equal(expected, operationPlane.ToJsonValue());
    }

    public static TheoryData<ToolOperationPlane[], ToolOperationPlane> AggregationCases => new()
    {
        { [ToolOperationPlane.Data], ToolOperationPlane.Data },
        { [ToolOperationPlane.Control], ToolOperationPlane.Control },
        { [ToolOperationPlane.Data, ToolOperationPlane.Control], ToolOperationPlane.Both },
        { [ToolOperationPlane.Both, ToolOperationPlane.Data], ToolOperationPlane.Both },
        { [ToolOperationPlane.NotApplicable], ToolOperationPlane.NotApplicable },
        { [ToolOperationPlane.NotApplicable, ToolOperationPlane.Data], ToolOperationPlane.Data },
        { [ToolOperationPlane.Data, ToolOperationPlane.Unspecified], ToolOperationPlane.Unspecified }
    };

    [Theory]
    [MemberData(nameof(AggregationCases))]
    public void Aggregate_ReturnsExpectedPlane(ToolOperationPlane[] operationPlanes, ToolOperationPlane expected)
    {
        Assert.Equal(expected, ToolOperationPlaneExtensions.Aggregate(operationPlanes));
    }
}
