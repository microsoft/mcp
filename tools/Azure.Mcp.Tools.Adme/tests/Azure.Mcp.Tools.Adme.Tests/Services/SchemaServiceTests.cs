// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
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
        var provider = CreateCredentialProvider(TestConstants.AccessToken);
        var service = new SchemaService(provider, new FakeHttpClientFactory(handler));

        var result = await service.GetSchemaAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            TestConstants.WellKind,
            TestConstants.Tenant,
            TestContext.Current.CancellationToken);

        Assert.Equal("Well", result.GetProperty("title").GetString());
        Assert.Equal(
            "/api/schema-service/v1/schema/osdu%3A" +
            "wks%3A" +
            "master-data--Well%3A1.0.0",
            handler.LastRequest!.RequestUri!.PathAndQuery);
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal(TestConstants.AccessToken, handler.LastRequest.Headers.Authorization.Parameter);
        Assert.Equal(TestConstants.DataPartition, handler.LastRequest.Headers.GetValues("data-partition-id").Single());
        await provider.Received(1).GetTokenCredentialAsync(TestConstants.Tenant, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListSchemasAsync_MapsFiltersToAdmeQueryAndDeserializesResponse()
    {
        var handler = JsonHandler(HttpStatusCode.OK, """
            {"schemaInfos":[{"schemaIdentity":{"id":"osdu:wks:master-data--Well:1.4.0"},"status":"PUBLISHED","scope":"SHARED","supersededBy":{"id":"osdu:wks:master-data--Well:2.0.0"}}],"offset":2,"count":1,"totalCount":3}
            """);
        var provider = CreateCredentialProvider();
        var service = new SchemaService(provider, new FakeHttpClientFactory(handler));

        var result = await service.ListSchemasAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            TestConstants.Tenant,
            TestConstants.WellAuthority,
            TestConstants.WellSource,
            TestConstants.WellEntityType,
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
        var schemaInfo = Assert.Single(result.SchemaInfos);
        Assert.Equal("osdu:wks:master-data--Well:1.4.0", schemaInfo.SchemaIdentity?.Id);
        Assert.Equal("osdu:wks:master-data--Well:2.0.0", schemaInfo.SupersededBy?.Id);
        var query = ParseQuery(handler.LastRequest!.RequestUri!.Query);
        Assert.Equal(TestConstants.WellAuthority, query["authority"]);
        Assert.Equal(TestConstants.WellSource, query["source"]);
        Assert.Equal(TestConstants.WellEntityType, query["entityType"]);
        Assert.Equal("PUBLISHED", query["status"]);
        Assert.Equal("SHARED", query["scope"]);
        Assert.Equal("1", query["schemaVersionMajor"]);
        Assert.Equal("4", query["schemaVersionMinor"]);
        Assert.Equal("0", query["schemaVersionPatch"]);
        Assert.Equal("true", query["latestVersion"]);
        Assert.Equal("2", query["offset"]);
        Assert.Equal("25", query["limit"]);
        await provider.Received(1).GetTokenCredentialAsync(TestConstants.Tenant, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListSchemasAsync_OmitsOptionalParametersWhenUnset()
    {
        var handler = JsonHandler(HttpStatusCode.OK, """{"schemaInfos":[],"offset":0,"count":0,"totalCount":0}""");
        var service = new SchemaService(CreateCredentialProvider(), new FakeHttpClientFactory(handler));

        await service.ListSchemasAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            null,
            null,
            TestConstants.WellEntityType,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null,
            TestContext.Current.CancellationToken);

        var query = ParseQuery(handler.LastRequest!.RequestUri!.Query);
        Assert.Single(query);
        Assert.Equal(TestConstants.WellEntityType, query["entityType"]);
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
            TestConstants.DataPartition,
            TestConstants.WellKind,
            null,
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetSchemaAsync_DoesNotExposeBackendResponseBodyOnFailure()
    {
        var handler = JsonHandler(HttpStatusCode.NotFound, "sensitive backend details");
        var service = new SchemaService(CreateCredentialProvider(), new FakeHttpClientFactory(handler));

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => service.GetSchemaAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            "osdu:wks:missing:1.0.0",
            null,
            TestContext.Current.CancellationToken));

        Assert.Equal((int)HttpStatusCode.NotFound, exception.Status);
        Assert.DoesNotContain("sensitive backend details", exception.Message);
    }

    private static IAzureTokenCredentialProvider CreateCredentialProvider(string token = "fake-token")
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1)));
        var provider = Substitute.For<IAzureTokenCredentialProvider>();
        provider.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
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
