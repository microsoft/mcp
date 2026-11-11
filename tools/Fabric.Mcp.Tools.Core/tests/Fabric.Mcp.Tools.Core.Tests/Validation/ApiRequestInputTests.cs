// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Validation;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Validation;

public sealed class ApiRequestInputTests
{
    [Theory]
    [InlineData(null, "fabric")]
    [InlineData("", "fabric")]
    [InlineData("fabric", "fabric")]
    [InlineData(" PowerBI ", "powerbi")]
    public void ParseApi_SelectsAllowlistedTarget(string? api, string expected)
    {
        var target = ApiRequestInput.ParseApi(api);

        Assert.Equal(expected, target.Name);
        Assert.Equal(expected == "fabric" ? "api.fabric.microsoft.com" : "api.powerbi.com", target.Host);
        Assert.Equal(
            expected == "fabric" ? "https://api.fabric.microsoft.com/.default" : "https://analysis.windows.net/powerbi/api/.default",
            Assert.Single(target.Scopes));
    }

    [Theory]
    [InlineData("azure")]
    [InlineData("https://api.fabric.microsoft.com")]
    [InlineData("api.powerbi.com")]
    public void ParseApi_RejectsOtherApis(string api) =>
        Assert.Equal(ApiRequestInput.ApiMessage, Assert.Throws<ApiRequestInputException>(() => ApiRequestInput.ParseApi(api)).Message);

    [Theory]
    [InlineData("fabric", "/v1/workspaces")]
    [InlineData("fabric", "/v1/connections?continuationToken=abc%2Bdef%3D%3D")]
    [InlineData("fabric", "/v1/admin/workspaces?type=Workspace&state=Active")]
    [InlineData("fabric", "/v1/workspaces/cfafbeb1-8037-4d0c-896e-a46fb27ff229/items/a%20b")]
    [InlineData("fabric", "/v1/admin/items?q=a/b?c")]
    [InlineData("powerbi", "/v1.0/myorg/gateways")]
    [InlineData("powerbi", "/v1.0/myorg/datasets/5b218778-e7a5-4d73-8187-f10824047715/refreshes?$top=10")]
    [InlineData("powerbi", "/v1.0/myorg/admin/activityevents?startDateTime='2024-01-01T00:00:00'&endDateTime='2024-01-01T23:59:59'")]
    [InlineData("powerbi", "/v1.0/myorg/admin/groups?$top=100&$filter=state%20eq%20'Active'")]
    public void BuildUri_AcceptsRelativeAllowlistedPaths(string api, string path)
    {
        var target = ApiRequestInput.ParseApi(api);

        var uri = ApiRequestInput.BuildUri(target, path);

        Assert.Equal("https", uri.Scheme);
        Assert.Equal(target.Host, uri.Host);
        Assert.Equal(path, uri.PathAndQuery);
        Assert.True(ApiRequestInput.IsValidPath(target, path));
    }

    [Theory]
    [InlineData("fabric", null)]
    [InlineData("fabric", "")]
    [InlineData("fabric", "/v1/")]
    [InlineData("fabric", "v1/workspaces")]
    [InlineData("fabric", "/v2/workspaces")]
    [InlineData("fabric", "/V1/workspaces")]
    [InlineData("fabric", "/v1.0/myorg/gateways")]
    [InlineData("powerbi", "/v1/workspaces")]
    [InlineData("powerbi", "/v1.0/gateways")]
    [InlineData("powerbi", "/v1.0/myorg")]
    [InlineData("fabric", "https://api.fabric.microsoft.com/v1/workspaces")]
    [InlineData("fabric", "//evil.example/v1/workspaces")]
    [InlineData("fabric", "/v1//workspaces")]
    [InlineData("fabric", "/v1/workspaces/")]
    [InlineData("fabric", "/v1/./workspaces")]
    [InlineData("fabric", "/v1/../v2/workspaces")]
    [InlineData("fabric", "/v1/workspaces/..")]
    [InlineData("fabric", "/v1/%2e%2e/admin")]
    [InlineData("fabric", "/v1/workspaces%2Fadmin")]
    [InlineData("fabric", "/v1/workspaces%5Cadmin")]
    [InlineData("fabric", "/v1/workspaces%252F")]
    [InlineData("fabric", "/v1/workspaces%0A")]
    [InlineData("fabric", "/v1/workspaces%zz")]
    [InlineData("fabric", "/v1/workspaces?x=%2")]
    [InlineData("fabric", "/v1/workspaces\\admin")]
    [InlineData("fabric", "/v1/work spaces")]
    [InlineData("fabric", "/v1/workspaces\t")]
    [InlineData("fabric", "/v1/workspaces#fragment")]
    [InlineData("fabric", "/v1/wörkspaces")]
    [InlineData("fabric", "/v1/workspaces?name=wörk")]
    [InlineData("fabric", "/v1/workspaces\u0000")]
    public void BuildUri_RejectsUnsafePathsWithFixedMessage(string api, string? path)
    {
        var target = ApiRequestInput.ParseApi(api);

        var exception = Assert.Throws<ApiRequestInputException>(() => ApiRequestInput.BuildUri(target, path));

        Assert.Equal(ApiRequestInput.PathMessage, exception.Message);
        Assert.False(ApiRequestInput.IsValidPath(target, path));
    }

    [Fact]
    public void BuildUri_RejectsOverlongPaths()
    {
        var path = "/v1/" + new string('a', ApiRequestInput.MaxPathLength);

        Assert.False(ApiRequestInput.IsValidPath(ApiRequestTarget.Fabric, path));
    }

    [Theory]
    [InlineData(null, ApiRequestInput.DefaultMaxResponseBytes)]
    [InlineData(1, 1)]
    [InlineData(ApiRequestInput.MaxResponseBytesLimit, ApiRequestInput.MaxResponseBytesLimit)]
    public void ParseMaxResponseBytes_AppliesDefaultAndLimit(int? value, int expected) =>
        Assert.Equal(expected, ApiRequestInput.ParseMaxResponseBytes(value));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(ApiRequestInput.MaxResponseBytesLimit + 1)]
    public void ParseMaxResponseBytes_RejectsOutOfRangeValues(int value) =>
        Assert.Throws<ApiRequestInputException>(() => ApiRequestInput.ParseMaxResponseBytes(value));

    [Theory]
    [InlineData("fabric", "https://api.fabric.microsoft.com/v1/workspaces?continuationToken=abc", "/v1/workspaces?continuationToken=abc")]
    [InlineData("fabric", "HTTPS://API.FABRIC.MICROSOFT.COM/v1/workspaces?continuationToken=abc", "/v1/workspaces?continuationToken=abc")]
    [InlineData("fabric", "/v1/workspaces?continuationToken=abc", "/v1/workspaces?continuationToken=abc")]
    [InlineData("powerbi", "https://api.powerbi.com/v1.0/myorg/admin/groups?$skiptoken=abc", "/v1.0/myorg/admin/groups?$skiptoken=abc")]
    public void GetRelativeLink_ReturnsSameHostRelativePath(string api, string link, string expected) =>
        Assert.Equal(expected, ApiRequestInput.GetRelativeLink(ApiRequestInput.ParseApi(api), link));

    [Theory]
    [InlineData("fabric", null)]
    [InlineData("fabric", "")]
    [InlineData("fabric", "https://evil.example/v1/workspaces")]
    [InlineData("fabric", "https://api.fabric.microsoft.com.evil.example/v1/workspaces")]
    [InlineData("fabric", "https://api.fabric.microsoft.com@evil.example/v1/workspaces")]
    [InlineData("fabric", "https://api.fabric.microsoft.com:8443/v1/workspaces")]
    [InlineData("fabric", "http://api.fabric.microsoft.com/v1/workspaces")]
    [InlineData("fabric", "https://api.powerbi.com/v1.0/myorg/gateways")]
    [InlineData("fabric", "//evil.example/v1/workspaces")]
    [InlineData("fabric", "v1/workspaces")]
    [InlineData("fabric", "https://api.fabric.microsoft.com/v1/../admin")]
    [InlineData("fabric", "https://api.fabric.microsoft.com/v2/workspaces")]
    [InlineData("powerbi", "https://api.fabric.microsoft.com/v1/workspaces")]
    public void GetRelativeLink_DropsOtherHostsAndUnsafePaths(string api, string? link) =>
        Assert.Null(ApiRequestInput.GetRelativeLink(ApiRequestInput.ParseApi(api), link));
}
