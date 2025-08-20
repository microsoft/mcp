// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.BicepSchema.UnitTests;

public class ApiVersionComparerTests
{
    [Fact]
    public void TestApiVersionOrder()
    {
        var apiVersions = new SortedSet<string>(ApiVersionComparer.Instance)
        {
            "2021-02-01-preview",
            "2021-01-01",
            "2021-01-02",
            "2021-02-01",
            "2021-02-01-alpha",
            "2021-02-01-privatepreview",
            "2024-11-01",
        };

        var expectedOrder = new List<string>
        {
            "2021-01-01",
            "2021-01-02",
            "2021-02-01-alpha",
            "2021-02-01-preview",
            "2021-02-01-privatepreview",
            "2021-02-01",
            "2024-11-01",
        };

        Assert.Equal(apiVersions, expectedOrder);
    }
}
