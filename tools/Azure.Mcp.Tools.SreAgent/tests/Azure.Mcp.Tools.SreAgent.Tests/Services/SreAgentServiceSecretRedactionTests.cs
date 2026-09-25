// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.SreAgent.Services;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Services;

/// <summary>
/// Verifies that <see cref="SreAgentService.RedactSecretsInPlace"/> and
/// <see cref="SreAgentService.SanitizeForErrorMessage"/> strip credentials before
/// connector ExtendedProperties or upstream error bodies leave the service.
/// </summary>
public class SreAgentServiceSecretRedactionTests
{
    [Fact]
    public void RedactSecretsInPlace_RedactsTopLevelBearerToken()
    {
        var properties = new Dictionary<string, object>
        {
            ["type"] = "http",
            ["endpoint"] = "https://example.com/mcp",
            ["bearerToken"] = "supersecret-token-value",
        };

        SreAgentService.RedactSecretsInPlace(properties);

        Assert.Equal("***", properties["bearerToken"]);
        Assert.Equal("https://example.com/mcp", properties["endpoint"]);
        Assert.Equal("http", properties["type"]);
    }

    [Theory]
    [InlineData("apiKey")]
    [InlineData("ApiKey")]
    [InlineData("APIKEY")]
    [InlineData("password")]
    [InlineData("secret")]
    [InlineData("clientSecret")]
    [InlineData("accessToken")]
    [InlineData("refreshToken")]
    [InlineData("token")]
    [InlineData("authorization")]
    public void RedactSecretsInPlace_RedactsCommonSecretKeys(string key)
    {
        var properties = new Dictionary<string, object> { [key] = "the-secret" };

        SreAgentService.RedactSecretsInPlace(properties);

        Assert.Equal("***", properties[key]);
    }

    [Fact]
    public void RedactSecretsInPlace_RedactsAuthorizationInsideHeadersDictionary()
    {
        var properties = new Dictionary<string, object>
        {
            ["type"] = "http",
            ["headers"] = new Dictionary<string, object>
            {
                ["Authorization"] = "Basic dXNlcjpwYXNz",
                ["X-Trace"] = "abc",
            },
        };

        SreAgentService.RedactSecretsInPlace(properties);

        var headers = Assert.IsType<Dictionary<string, object>>(properties["headers"]);
        Assert.Equal("***", headers["Authorization"]);
        Assert.Equal("abc", headers["X-Trace"]);
    }

    [Fact]
    public void RedactSecretsInPlace_RedactsAuthorizationInsideHeadersJsonElement()
    {
        // Simulates the deserialized shape coming back from ARM, where nested objects
        // arrive as JsonElement values rather than Dictionary<string, object>.
        var json = """
        {
          "type": "http",
          "headers": { "Authorization": "Bearer supersecret", "X-Trace": "abc" }
        }
        """;
        var element = JsonDocument.Parse(json).RootElement;
        var properties = new Dictionary<string, object>
        {
            ["type"] = element.GetProperty("type").GetString()!,
            ["headers"] = element.GetProperty("headers"),
        };

        SreAgentService.RedactSecretsInPlace(properties);

        var headers = Assert.IsType<Dictionary<string, object>>(properties["headers"]);
        Assert.Equal("***", headers["Authorization"]);
        Assert.Equal("abc", headers["X-Trace"]?.ToString());
    }

    [Fact]
    public void RedactSecretsInPlace_NullOrEmpty_NoOp()
    {
        SreAgentService.RedactSecretsInPlace(null);
        var empty = new Dictionary<string, object>();
        SreAgentService.RedactSecretsInPlace(empty);
        Assert.Empty(empty);
    }

    [Fact]
    public void SanitizeForErrorMessage_RedactsBearerTokenInJsonBody()
    {
        var body = """{"error":{"code":"BadRequest","message":"invalid"},"properties":{"bearerToken":"supersecret-token-value","endpoint":"https://x"}}""";

        var scrubbed = SreAgentService.SanitizeForErrorMessage(body, 1000);

        Assert.DoesNotContain("supersecret-token-value", scrubbed);
        Assert.Contains("\"bearerToken\":\"***\"", scrubbed);
        Assert.Contains("\"endpoint\":\"https://x\"", scrubbed);
    }

    [Theory]
    [InlineData("\"password\":\"hunter2\"")]
    [InlineData("\"apiKey\":\"pd-abcdef\"")]
    [InlineData("\"clientSecret\":\"abc.def.ghi\"")]
    [InlineData("\"accessToken\":\"eyJ.payload.sig\"")]
    [InlineData("\"refreshToken\":\"rt-12345\"")]
    public void SanitizeForErrorMessage_RedactsAssortedSecretJsonValues(string snippet)
    {
        var body = "{\"properties\":{" + snippet + "}}";

        var scrubbed = SreAgentService.SanitizeForErrorMessage(body, 1000);

        Assert.DoesNotContain(snippet, scrubbed);
        Assert.Contains("\"***\"", scrubbed);
    }

    [Fact]
    public void SanitizeForErrorMessage_RedactsAuthorizationHeaderInJson()
    {
        var body = """{"headers":{"Authorization":"Bearer eyJabc.def.ghi"}}""";

        var scrubbed = SreAgentService.SanitizeForErrorMessage(body, 1000);

        Assert.DoesNotContain("eyJabc.def.ghi", scrubbed);
        Assert.Contains("\"Authorization\":\"***\"", scrubbed);
    }

    [Fact]
    public void SanitizeForErrorMessage_RedactsRawAuthorizationHeaderForm()
    {
        var body = "Authorization: Bearer eyJabc.def.ghi";

        var scrubbed = SreAgentService.SanitizeForErrorMessage(body, 1000);

        Assert.DoesNotContain("eyJabc.def.ghi", scrubbed);
        Assert.Contains("***", scrubbed);
    }

    [Fact]
    public void SanitizeForErrorMessage_TruncatesAfterRedaction()
    {
        var body = new string('a', 500) + "\"bearerToken\":\"supersecret\"";

        var scrubbed = SreAgentService.SanitizeForErrorMessage(body, 100);

        Assert.True(scrubbed.Length <= 103); // 100 + "..."
        Assert.DoesNotContain("supersecret", scrubbed);
    }

    [Fact]
    public void SanitizeForErrorMessage_EmptyOrNull_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, SreAgentService.SanitizeForErrorMessage(null, 100));
        Assert.Equal(string.Empty, SreAgentService.SanitizeForErrorMessage(string.Empty, 100));
    }
}
