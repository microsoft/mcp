// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace CopilotCliTester.Tests;

public sealed class AgentRunnerRedactSecretsTests
{
    #region JWT Pattern

    [Theory]
    [InlineData("token: eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.abcdefghijklmnopqrstuvwxyz")]
    public void RedactSecrets_JwtToken_IsRedacted(string input)
    {
        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal("[REDACTED]", result);
    }

    [Theory]
    [InlineData("Authorization header contained eyJhbGciOiJSUzI1NiJ9.eyJpc3MiOiJleGFtcGxlIn0.signaturevalue1234 which should be hidden")]
    public void RedactSecrets_JwtInMiddleOfText_IsRedacted(string input)
    {
        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal("Authorization header contained [REDACTED] which should be hidden", result);
    }

    [Fact]
    public void RedactSecrets_MultipleJwts_AllRedacted()
    {
        var jwt1 = "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.abcdefghijklmnopqr";
        var jwt2 = "eyJhbGciOiJSUzI1NiJ9.eyJpc3MiOiJleGFtcGxlIn0.zyxwvutsrqponmlkji";

        var result = AgentRunner.RedactSecrets($"{jwt1} and {jwt2}");

        Assert.Equal("[REDACTED] and [REDACTED]", result);
    }

    #endregion

    #region Bearer Pattern

    [Theory]
    [InlineData("bearer ABCDEFghijklmnopqrstuvwxyz")]
    [InlineData("BEARER ABCDEFghijklmnopqrstuvwxyz")]
    [InlineData("Bearer ABCDEFghijklmnopqrstuvwxyz")]
    [InlineData("Bearer abcdefghijklmnopqrstuvwxyz1234")]
    [InlineData("Bearer abc.def-ghi_jkl~mno+pqr/stu")]
    public void RedactSecrets_BearerToken_CaseInsensitive(string input)
    {
        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal("[REDACTED]", result);
    }

    [Theory]
    [InlineData("Bearer short")]
    public void RedactSecrets_BearerTokenTooShort_NotRedacted(string input)
    {
        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal(input, result);
    }

    #endregion

    #region SecretKeyValue Pattern

    [Theory]
    [InlineData("password=my-super-secret-value")]
    [InlineData("secret=my-super-secret-value")]
    [InlineData("token=my-super-secret-value")]
    [InlineData("api_key=my-super-secret-value")]
    [InlineData("api-key=my-super-secret-value")]
    [InlineData("apikey=my-super-secret-value")]
    public void RedactSecrets_SecretKeywords_WithEquals_Redacted(string input)
    {
        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal("[REDACTED]", result);
    }

    [Theory]
    [InlineData("password: my-super-secret-value")]
    [InlineData("secret:  my-super-secret-value")]
    [InlineData("token:my-super-secret-value")]
    [InlineData("api_key: my-super-secret-value  ")]
    [InlineData("api-key: my-super-secret-value")]
    [InlineData("apikey: my-super-secret-value")]
    public void RedactSecrets_SecretKeywords_WithColon_Redacted(string input)
    {
        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal("[REDACTED]", result);
    }

    [Theory]
    [InlineData("PASSWORD=supersecretvalue1")]
    [InlineData("Secret=supersecretvalue1")]
    [InlineData("TOKEN=supersecretvalue1")]
    [InlineData("API_KEY=supersecretvalue1")]
    public void RedactSecrets_SecretKeywords_CaseInsensitive(string input)
    {
        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal("[REDACTED]", result);
    }

    [Theory]
    [InlineData("api_key = abcdef123456", "[REDACTED]")]
    [InlineData("password=short", "password=short")]
    [InlineData("password=this is a long secret value\nnext line is safe",
        "[REDACTED]\nnext line is safe")]
    [InlineData("password=\"my secret value with spaces\"", "[REDACTED]")]
    [InlineData("secret='my secret value with spaces'", "[REDACTED]")]
    public void RedactSecrets_SecretKeyWithSpacesAroundSeparator_Redacted(string input, string expected)
    {
        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("password=\"\"")]
    public void RedactSecrets_EmptyQuotedValue_Redacted(string input)
    {
        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal("[REDACTED]", result);
    }

    #endregion

    #region Mixed Content

    [Fact]
    public void RedactSecrets_JwtAndBearerAndKeyValue_AllRedacted()
    {
        var jwt = "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.abcdefghijklmnopqr";

        var input =
            $"Found {jwt} and Bearer abcdefghijklmnopqrstuvwxyz and password=mysecretpassword1";

        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal(
            "Found [REDACTED] and [REDACTED] and [REDACTED]",
            result);
    }

    [Theory]
    [InlineData(
        "secret=value-one-secret\ntoken=value-two-secret",
        "[REDACTED]\n[REDACTED]")]
    public void RedactSecrets_MultipleKeyValuePairsOnSeparateLines_AllRedacted(string input, string expected)
    {
        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal(expected, result);
    }

    #endregion

    #region Safe Text

    [Theory]
    [InlineData("This is just normal text with no secrets.")]
    [InlineData("The word password appeared but without a value assignment.")]
    [InlineData("")]
    public void RedactSecrets_PlainOrEmptyText_Unchanged(string input)
    {
        var result = AgentRunner.RedactSecrets(input);

        Assert.Equal(input, result);
    }

    #endregion
}
