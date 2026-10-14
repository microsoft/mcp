// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricCoreHttpHelpersTests()
{
    // cspell:disable-next-line
    private const string DocumentedContinuationToken = "LDEsMTAwMDAwLDA%3D";

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("45", 45L)]
    [InlineData(" 45 ", 45L)]
    [InlineData("2147483647", 2147483647L)]
    public void GetRetryAfter_AcceptsNonnegativeDeltaSeconds(string header, long seconds)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", header));

        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);

        Assert.NotNull(retryAfter);
        Assert.Equal(TimeSpan.FromSeconds(seconds), retryAfter.Delta);
        Assert.Null(retryAfter.Date);
    }

    [Fact]
    public void GetRetryAfter_AcceptsHttpDateWithoutApplyingRetryPolicy()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", "Wed, 21 Oct 2015 07:28:00 GMT"));

        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);

        Assert.NotNull(retryAfter);
        Assert.Null(retryAfter.Delta);
        Assert.Equal(new DateTimeOffset(2015, 10, 21, 7, 28, 0, TimeSpan.Zero), retryAfter.Date);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData("1.5")]
    [InlineData("45, 60")]
    [InlineData("2147483648")]
    [InlineData("9223372036854775807")]
    [InlineData("999999999999999999999999999999999999999")]
    [InlineData("backend-error-text")]
    [InlineData("not-a-date")]
    public void GetRetryAfter_RejectsMissingOrInvalidValues(string? header)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        if (header is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", header));
        }

        Assert.Null(FabricCoreHttpHelpers.GetRetryAfter(response));
    }

    [Theory]
    [InlineData("45", "60")]
    [InlineData("45", "45")]
    [InlineData("45", "backend-error-text")]
    [InlineData("Wed, 21 Oct 2015 07:28:00 GMT", "60")]
    public void GetRetryAfter_RejectsMultipleHeaderValues(string first, string second)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", [first, second]));

        Assert.Null(FabricCoreHttpHelpers.GetRetryAfter(response));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("raw+/= &?#", "raw%2B%2F%3D%20%26%3F%23")]
    [InlineData("%3D%2f%26%3F%23%2B%25", "%3D%2f%26%3F%23%2B%25")]
    [InlineData("%3b%40%3A%5B%5D%24%2C%21%27%28%29%2A", "%3b%40%3A%5B%5D%24%2C%21%27%28%29%2A")]
    // cspell:disable-next-line
    [InlineData("x%2F+y%3D?b=two#frag&extra", "x%2F%2By%3D%3Fb%3Dtwo%23frag%26extra")]
    [InlineData("%G1%", "%25G1%25")]
    [InlineData("tail%2", "tail%252")]
    [InlineData("%2541%2526", "%2541%2526")]
    [InlineData("%41%7e%2D%5f%2E", "A~-_.")]
    [InlineData("\u00E9\U0001F680 /", "%C3%A9%F0%9F%9A%80%20%2F")]
    [InlineData("\r\n%0D%0A", "%0D%0A%0D%0A")]
    // cspell:disable-next-line
    [InlineData("https://example.invalid/next?x=1#fragment", "https%3A%2F%2Fexample.invalid%2Fnext%3Fx%3D1%23fragment")]
    public async Task EncodeContinuationToken_ProducesSafeCanonicalRequestUri(string token, string expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.NotNull(request.RequestUri);
            Assert.Equal(
                $"https://api.fabric.microsoft.com/v1/capacities?continuationToken={expected}&fixed=true",
                request.RequestUri.AbsoluteUri);
            Assert.Empty(request.RequestUri.Fragment);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        using var client = new HttpClient(handler);
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/capacities?continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(token)}&fixed=true";

        using var response = await client.GetAsync(url, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    // cspell:disable
    [InlineData("capacities", "https://api.fabric.microsoft.com/v1/capacities?continuationToken=LDEsMTAwMDAwLDA%3D")]
    [InlineData("workspaces", "https://api.fabric.microsoft.com/v1/workspaces?continuationToken=LDEsMTAwMDAwLDA%3D")]
    [InlineData("workspaces/cfafbeb1-8037-4d0c-896e-a46fb27ff229/items",
        "https://api.fabric.microsoft.com/v1/workspaces/cfafbeb1-8037-4d0c-896e-a46fb27ff229/items?continuationToken=LDEsMTAwMDAwLDA%3D")]
    // cspell:enable
    public async Task EncodeContinuationToken_MatchesOfficialFabricRequestUris(string resourcePath, string documentedUri)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(documentedUri, request.RequestUri?.AbsoluteUri);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        using var client = new HttpClient(handler);
        var url = $"{FabricEndpoints.FabricApiBaseUrl}/{resourcePath}?continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(DocumentedContinuationToken)}";

        using var response = await client.GetAsync(url, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public void Helpers_RejectNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => FabricCoreHttpHelpers.GetRetryAfter(null!));
        Assert.Throws<ArgumentNullException>(() => FabricCoreHttpHelpers.EncodeContinuationToken(null!));
    }
}
