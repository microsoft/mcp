// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Models;

public class CapacityListSerializationTests()
{
    [Fact]
    public void SourceGeneratedOutput_ContainsOnlyTheCapacityContract()
    {
        var page = JsonSerializer.Deserialize(CapacityListTestData.FullPage, CoreJsonContext.Default.CapacityListResponse);
        Assert.NotNull(page);

        var result = new CapacityListCommandResult(page.Value, page.ContinuationToken, page.ContinuationUri);
        var json = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.CapacityListCommandResult);

        Assert.Equal(
            ["capacities", "continuationToken", "continuationUri"],
            json.EnumerateObject().Select(static property => property.Name));
        var capacity = Assert.Single(json.GetProperty("capacities").EnumerateArray());
        Assert.Equal(
            ["id", "displayName", "sku", "region", "state"],
            capacity.EnumerateObject().Select(static property => property.Name));
        Assert.Equal(CapacityListTestData.CapacityId, capacity.GetProperty("id").GetString());
        Assert.Equal("FutureState", capacity.GetProperty("state").GetString());
        Assert.Equal(CapacityListTestData.ContinuationToken, json.GetProperty("continuationToken").GetString());
        Assert.Equal(CapacityListTestData.ContinuationUri, json.GetProperty("continuationUri").GetString());
        Assert.DoesNotContain("private-", json.GetRawText());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("raw+/=%2f%G1", null)]
    [InlineData("%41%7e%2D%5f%2E", null)]
    [InlineData(null, "https://untrusted.invalid/next?opaque=%3D")]
    [InlineData("", "")]
    public void SourceGeneratedOutput_PreservesIndependentContinuationStrings(string? token, string? uri)
    {
        var result = new CapacityListCommandResult([], token, uri);
        var json = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.CapacityListCommandResult);

        Assert.Equal(0, json.GetProperty("capacities").GetArrayLength());
        Assert.Equal(token is not null, json.TryGetProperty("continuationToken", out var tokenProperty));
        Assert.Equal(uri is not null, json.TryGetProperty("continuationUri", out var uriProperty));
        if (token is not null)
        {
            Assert.Equal(token, tokenProperty.GetString());
        }
        if (uri is not null)
        {
            Assert.Equal(uri, uriProperty.GetString());
        }
    }
}
