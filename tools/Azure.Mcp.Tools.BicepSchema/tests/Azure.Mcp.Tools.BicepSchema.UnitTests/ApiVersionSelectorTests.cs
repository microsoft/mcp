// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.BicepSchema.Services.Support;
using Xunit;

namespace Azure.Mcp.Tools.BicepSchema.UnitTests;

public class ApiVersionSelectorTests
{
    [Theory]
    [InlineData("2021-01-02", new object[] { "2021-01-02", "2021-01-01" })]
    [InlineData("2021-01-01", new object[] { "2021-01-01" })]
    [InlineData("2021-11-01", new object[] { "2021-11-01", "2021-01-01" })]
    [InlineData("2021-01-01", new object[] { "2020-11-01", "2021-01-01" })]
    [InlineData("2024-01-01", new object[] { "2021-01-01", "2024-01-01", "2021-11-01" })]
    [InlineData("2021-01-02", new object[] { "2021-01-01", "2021-01-02", "2024-01-01-preview" })]
    [InlineData("2021-01-01", new object[] { "2021-01-01", "2021-01-01-beta", "2021-01-01-preview" })]
    [InlineData("2022-01-01-foo", new object[] { "2022-01-01-foo", "2021-01-01-beta", "2021-01-01-preview" })]
    public void SelectLatestStable(string expected, object[] apiVersions)
    {
        string selected = ApiVersionSelector.SelectLatestStable(apiVersions.Cast<string>());
        Assert.Equal(expected, selected);

    }
}
