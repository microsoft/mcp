// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.AppLens.Services;
using Xunit;

namespace Azure.Mcp.Tools.AppLens.Tests.Services;

public class AppLensServiceTests
{
    [Theory]
    [InlineData("invalid-json")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"properties":null}""")]
    [InlineData("""{"properties":{}}""")]
    [InlineData("""{"properties":{"dataset":{}}}""")]
    [InlineData("""{"properties":{"dataset":[]}}""")]
    [InlineData("""{"properties":{"dataset":[null]}}""")]
    [InlineData("""{"properties":{"dataset":[{}]}}""")]
    [InlineData("""{"properties":{"dataset":[{"table":null}]}}""")]
    [InlineData("""{"properties":{"dataset":[{"table":{}}]}}""")]
    [InlineData("""{"properties":{"dataset":[{"table":{"rows":{}}}]}}""")]
    [InlineData("""{"properties":{"dataset":[{"table":{"rows":[]}}]}}""")]
    [InlineData("""{"properties":{"dataset":[{"table":{"rows":[null]}}]}}""")]
    [InlineData("""{"properties":{"dataset":[{"table":{"rows":[[]]}}]}}""")]
    [InlineData("""{"properties":{"dataset":[{"table":{"rows":[["session","token"]]}}]}}""")]
    [InlineData("""{"properties":{"dataset":[{"table":{"rows":[[null,"token",3600]]}}]}}""")]
    [InlineData("""{"properties":{"dataset":[{"table":{"rows":[["session",null,3600]]}}]}}""")]
    [InlineData("""{"properties":{"dataset":[{"table":{"rows":[["session","token","3600"]]}}]}}""")]
    [InlineData("""{"properties":{"dataset":[{"table":{"rows":[["session","token",2147483648]]}}]}}""")]
    public void ParseGetTokenResponse_MalformedResponse_ThrowsJsonException(string response)
    {
        Assert.ThrowsAny<JsonException>(() => AppLensService.ParseGetTokenResponse(response));
    }

    [Fact]
    public void ParseGetTokenResponse_ValidResponse_ReturnsFirstSessionRow()
    {
        const string response = """
            {
                "properties": {
                    "dataset": [{
                        "table": {
                            "rows": [
                                ["session", "token", 3600],
                                ["other-session", "other-token", 7200]
                            ]
                        }
                    }]
                }
            }
            """;

        var session = AppLensService.ParseGetTokenResponse(response);

        Assert.Equal("session", session.SessionId);
        Assert.Equal("token", session.Token);
        Assert.Equal(3600, session.ExpiresIn);
        Assert.Empty(session.ResourceId);
    }
}
