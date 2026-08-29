// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Tools.Adme.Models.Schema;
using Azure.Mcp.Tools.Adme.Services;
using Azure.Mcp.Tools.Adme.Tests.TestSupport;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Services;

public sealed class SchemaServiceTests
{
    [Fact]
    public async Task GetSchemaAsync_SendsEscapedKindAuthenticationAndPartition()
    {
        var handler = JsonHandler(HttpStatusCode.OK, """{"title":"Well"}""");
        var provider = CreateCredentialProvider("token-abc");
        var service = new SchemaService(provider, new FakeHttpClientFactory(handler));

        var result = await service.GetSchemaAsync(
            "https://sample.energy.azure.com",
            "opendes",
            "osdu:wks:master-data--Well:1.0.0",
            TestContext.Current.CancellationToken);

        Assert.Equal("Well", result.GetProperty("title").GetString());
        Assert.Equal(
            "/api/schema-service/v1/schema/osdu%3Awks%3Amaster-data--Well%3A1.0.0",
            handler.LastRequest!.RequestUri!.PathAndQuery);
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("token-abc", handler.LastRequest.Headers.Authorization.Parameter);
        Assert.Equal("opendes", handler.LastRequest.Headers.GetValues("data-partition-id").Single());
        await provider.Received(1).GetTokenCredentialAsync(null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListSchemasAsync_MapsFiltersToAdmeQueryAndDeserializesResponse()
    {
        var handler = JsonHandler(HttpStatusCode.OK, """
            {"schemaInfos":[{"schemaIdentity":{"id":"osdu:wks:master-data--Well:1.4.0"},"status":"PUBLISHED","scope":"SHARED"}],"offset":2,"count":1,"totalCount":3}
            """);
        var service = new SchemaService(CreateCredentialProvider(), new FakeHttpClientFactory(handler));

        var result = await service.ListSchemasAsync(
            "https://sample.energy.azure.com",
            "opendes",
            "osdu",
            "wks",
            "master-data--Well",
            SchemaStatus.PUBLISHED,
            SchemaScope.SHARED,
            1,
            4,
            0,
            true,
            2,
            25,
            TestContext.Current.CancellationToken);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal("osdu:wks:master-data--Well:1.4.0", Assert.Single(result.SchemaInfos).SchemaIdentity?.Id);
        var query = ParseQuery(handler.LastRequest!.RequestUri!.Query);
        Assert.Equal("osdu", query["authority"]);
        Assert.Equal("wks", query["source"]);
        Assert.Equal("master-data--Well", query["entityType"]);
        Assert.Equal("PUBLISHED", query["status"]);
        Assert.Equal("SHARED", query["scope"]);
        Assert.Equal("1", query["schemaVersionMajor"]);
        Assert.Equal("4", query["schemaVersionMinor"]);
        Assert.Equal("0", query["schemaVersionPatch"]);
        Assert.Equal("true", query["latestVersion"]);
        Assert.Equal("2", query["offset"]);
        Assert.Equal("25", query["limit"]);
    }

    [Fact]
    public async Task ListSchemasAsync_OmitsOptionalFiltersWhenUnset()
    {
        var handler = JsonHandler(HttpStatusCode.OK, """{"schemaInfos":[],"offset":0,"count":0,"totalCount":0}""");
        var service = new SchemaService(CreateCredentialProvider(), new FakeHttpClientFactory(handler));

        await service.ListSchemasAsync(
            "https://sample.energy.azure.com",
            "opendes",
            null,
            null,
            "master-data--Well",
            null,
            null,
            null,
            null,
            null,
            false,
            0,
            100,
            TestContext.Current.CancellationToken);

        var query = ParseQuery(handler.LastRequest!.RequestUri!.Query);
        Assert.Equal(3, query.Count);
        Assert.Equal("master-data--Well", query["entityType"]);
        Assert.Equal("0", query["offset"]);
        Assert.Equal("100", query["limit"]);
    }

    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("https://sample.energy.azure.com.evil.example")]
    [InlineData("https://sample.oep.ppe.azure-int.net.evil.example")]
    [InlineData("http://sample.energy.azure.com")]
    [InlineData("http://sample.oep.ppe.azure-int.net")]
    public async Task GetSchemaAsync_RejectsUntrustedEndpoint(string endpoint)
    {
        var service = new SchemaService(
            CreateCredentialProvider(),
            new FakeHttpClientFactory(JsonHandler(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<System.Security.SecurityException>(() => service.GetSchemaAsync(
            endpoint,
            "opendes",
            "osdu:wks:master-data--Well:1.0.0",
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetSchemaAsync_DoesNotExposeBackendResponseBodyOnFailure()
    {
        var handler = JsonHandler(HttpStatusCode.NotFound, "sensitive backend details");
        var service = new SchemaService(CreateCredentialProvider(), new FakeHttpClientFactory(handler));

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetSchemaAsync(
            "https://sample.energy.azure.com",
            "opendes",
            "osdu:wks:missing:1.0.0",
            TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.DoesNotContain("sensitive backend details", exception.Message);
    }

    private static IAzureTokenCredentialProvider CreateCredentialProvider(string token = "fake-token")
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1)));
        var provider = Substitute.For<IAzureTokenCredentialProvider>();
        provider.GetTokenCredentialAsync(null, Arg.Any<CancellationToken>()).Returns(credential);
        return provider;
    }

    private static StubHttpMessageHandler JsonHandler(HttpStatusCode status, string content) =>
        new(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json"),
        });

    private static Dictionary<string, string> ParseQuery(string query) =>
        query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .ToDictionary(
                part => Uri.UnescapeDataString(part[0]),
                part => Uri.UnescapeDataString(part[1]));
}
