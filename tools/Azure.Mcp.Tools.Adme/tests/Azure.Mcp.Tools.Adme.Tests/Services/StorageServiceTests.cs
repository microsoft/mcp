// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Mcp.Tools.Adme.Models.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Azure.Mcp.Tools.Adme.Tests.TestSupport;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Services;

public sealed class StorageServiceTests
{
    private const string RecordId = "opendes:master-data--Well:W-99";
    private const string EscapedRecordId = "opendes%3Amaster-data--Well%3AW-99";

    [Fact]
    public async Task GetRecordAsync_SendsEscapedIdAuthenticationPartitionAndTenant()
    {
        var handler = JsonHandler(HttpStatusCode.OK, $$"""{"id":"{{RecordId}}","kind":"{{TestConstants.WellKind}}"}""");
        var provider = CreateCredentialProvider("token-abc", TestConstants.Tenant);
        var service = new StorageService(provider, new FakeHttpClientFactory(handler));

        var result = await service.GetRecordAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, RecordId, null, null,
            TestConstants.Tenant, TestContext.Current.CancellationToken);

        Assert.Equal(RecordId, result.Id);
        Assert.Equal($"/api/storage/v2/records/{EscapedRecordId}", handler.LastRequest!.RequestUri!.PathAndQuery);
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal("token-abc", handler.LastRequest.Headers.Authorization!.Parameter);
        Assert.Equal(TestConstants.DataPartition, handler.LastRequest.Headers.GetValues("data-partition-id").Single());
        await provider.Received(1).GetTokenCredentialAsync(TestConstants.Tenant, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRecordAsync_WithAttributes_AppendsRepeatedAttributeQueryParameters()
    {
        var handler = JsonHandler(HttpStatusCode.OK, $$"""{"id":"{{RecordId}}"}""");
        var service = CreateService(handler);

        await service.GetRecordAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, RecordId, null,
            ["data.Name", "data.WellID"], null, TestContext.Current.CancellationToken);

        Assert.Equal(
            $"/api/storage/v2/records/{EscapedRecordId}?attribute=data.Name&attribute=data.WellID",
            handler.LastRequest!.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task GetRecordAsync_WithVersion_BuildsVersionedPath()
    {
        const long version = 1704779151123456;
        var handler = JsonHandler(HttpStatusCode.OK, $$"""{"id":"{{RecordId}}","version":{{version}}}""");
        var service = CreateService(handler);

        var result = await service.GetRecordAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, RecordId, version, null, null,
            TestContext.Current.CancellationToken);

        Assert.Equal(version, result.Version);
        Assert.Equal($"/api/storage/v2/records/{EscapedRecordId}/{version}", handler.LastRequest!.RequestUri!.PathAndQuery);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetRecordAsync_RejectsMissingId(string? id)
    {
        var handler = JsonHandler(HttpStatusCode.OK, "{}");
        var service = CreateService(handler);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.GetRecordAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, id!, null, null, null,
            TestContext.Current.CancellationToken));

        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task GetRecordAsync_WithVersionAndAttributes_BuildsProjectedVersionedPath()
    {
        const long version = 1704779151123456;
        var handler = JsonHandler(HttpStatusCode.OK, "{}");
        var service = CreateService(handler);

        await service.GetRecordAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, RecordId, version,
            ["data.Name", "data.WellID"], null, TestContext.Current.CancellationToken);

        Assert.Equal(
            $"/api/storage/v2/records/{EscapedRecordId}/{version}?attribute=data.Name&attribute=data.WellID",
            handler.LastRequest!.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task ListRecordVersionsAsync_BuildsVersionsPathAndDeserializesResponse()
    {
        var handler = JsonHandler(HttpStatusCode.OK, $$"""{"recordId":"{{RecordId}}","versions":[1,2,3]}""");
        var service = CreateService(handler);

        var result = await service.ListRecordVersionsAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, RecordId, null,
            TestContext.Current.CancellationToken);

        Assert.Equal([1L, 2L, 3L], result.Versions);
        Assert.Equal($"/api/storage/v2/records/versions/{EscapedRecordId}", handler.LastRequest!.RequestUri!.PathAndQuery);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ListRecordVersionsAsync_RejectsMissingId(string? id)
    {
        var handler = JsonHandler(HttpStatusCode.OK, "{}");
        var service = CreateService(handler);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.ListRecordVersionsAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, id!, null,
            TestContext.Current.CancellationToken));

        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task QueryRecordsByKindAsync_MapsKindLimitAndCursorToQuery()
    {
        var handler = JsonHandler(HttpStatusCode.OK, $$"""{"cursor":"next","results":["{{RecordId}}"]}""");
        var service = CreateService(handler);

        var result = await service.QueryRecordsByKindAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, TestConstants.WellKind, 25,
            "prev-cursor", null, TestContext.Current.CancellationToken);

        Assert.Equal("next", result.Cursor);
        Assert.Equal(RecordId, Assert.Single(result.Results));
        var query = ParseQuery(handler.LastRequest!.RequestUri!.Query);
        Assert.Equal(TestConstants.WellKind, query["kind"]);
        Assert.Equal("25", query["limit"]);
        Assert.Equal("prev-cursor", query["cursor"]);
        Assert.Equal("application/json", handler.LastRequest.Content!.Headers.ContentType!.MediaType);
        Assert.Equal(string.Empty, handler.LastRequestBody);
    }

    [Fact]
    public async Task QueryRecordsByKindAsync_OmitsCursorWhenUnset()
    {
        var handler = JsonHandler(HttpStatusCode.OK, """{"cursor":null,"results":[]}""");
        var service = CreateService(handler);

        await service.QueryRecordsByKindAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, TestConstants.WellKind, 10,
            null, null, TestContext.Current.CancellationToken);

        var query = ParseQuery(handler.LastRequest!.RequestUri!.Query);
        Assert.Equal(2, query.Count);
        Assert.False(query.ContainsKey("cursor"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task QueryRecordsByKindAsync_RejectsMissingKind(string? kind)
    {
        var handler = JsonHandler(HttpStatusCode.OK, "{}");
        var service = CreateService(handler);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.QueryRecordsByKindAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, kind!, 10, null, null,
            TestContext.Current.CancellationToken));

        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task FetchRecordsAsync_PostsToBatchEndpointWithoutConversionByDefault()
    {
        var handler = JsonHandler(HttpStatusCode.OK, """{"records":[],"notFound":[]}""");
        var service = CreateService(handler);

        await service.FetchRecordsAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, [RecordId], null, false, null,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("/api/storage/v2/query/records:batch", handler.LastRequest.RequestUri!.PathAndQuery);
        Assert.Equal("none", handler.LastRequest.Headers.GetValues("frame-of-reference").Single());
        Assert.Equal($$"""{"records":["{{RecordId}}"]}""", handler.LastRequestBody);
    }

    [Fact]
    public async Task FetchRecordsAsync_RequestsConversionWhenFrameOfReferenceEnabled()
    {
        var handler = JsonHandler(HttpStatusCode.OK, """{"records":[],"conversionStatuses":[]}""");
        var service = CreateService(handler);

        await service.FetchRecordsAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, [RecordId], null, true, null,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "units=SI;crs=wgs84;elevation=msl;azimuth=true north;dates=utc;",
            handler.LastRequest!.Headers.GetValues("frame-of-reference").Single());
    }

    [Fact]
    public async Task FetchRecordsAsync_WithAttributes_PostsProjectionToNonBatchEndpoint()
    {
        var handler = JsonHandler(HttpStatusCode.OK, """{"records":[]}""");
        var service = CreateService(handler);

        await service.FetchRecordsAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, [RecordId], ["data.Name"], false,
            null, TestContext.Current.CancellationToken);

        Assert.Equal("/api/storage/v2/query/records", handler.LastRequest!.RequestUri!.PathAndQuery);
        Assert.False(handler.LastRequest.Headers.Contains("frame-of-reference"));
        Assert.Equal($$"""{"records":["{{RecordId}}"],"attributes":["data.Name"]}""", handler.LastRequestBody);
    }

    [Fact]
    public async Task FetchRecordsAsync_RejectsEmptyIds()
    {
        var handler = JsonHandler(HttpStatusCode.OK, "{}");
        var service = CreateService(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => service.FetchRecordsAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, [], null, false, null,
            TestContext.Current.CancellationToken));

        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task UpsertRecordsAsync_PutsRecordsAndDeserializesResponse()
    {
        var handler = JsonHandler(HttpStatusCode.OK, $$"""{"recordIds":["{{RecordId}}"],"recordCount":1}""");
        var service = CreateService(handler);

        var result = await service.UpsertRecordsAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, [CreateRecord()],
            TestConstants.Tenant, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest!.Method);
        Assert.Equal("/api/storage/v2/records", handler.LastRequest.RequestUri!.PathAndQuery);
        Assert.Contains(RecordId, handler.LastRequestBody);
        Assert.Equal(RecordId, Assert.Single(result.RecordIds!));
        Assert.Equal(1, result.RecordCount);
    }

    [Fact]
    public async Task UpsertRecordsAsync_RejectsEmptyRecords()
    {
        var handler = JsonHandler(HttpStatusCode.OK, "{}");
        var service = CreateService(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpsertRecordsAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, [], null,
            TestContext.Current.CancellationToken));

        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task DeleteRecordAsync_SendsDeleteWithEscapedIdAndAcceptsEmptyResponse()
    {
        var handler = JsonHandler(HttpStatusCode.NoContent, string.Empty);
        var service = CreateService(handler);

        await service.DeleteRecordAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, RecordId,
            TestConstants.Tenant, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest!.Method);
        Assert.Equal($"/api/storage/v2/records/{EscapedRecordId}", handler.LastRequest.RequestUri!.PathAndQuery);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeleteRecordAsync_RejectsMissingId(string? id)
    {
        var handler = JsonHandler(HttpStatusCode.NoContent, string.Empty);
        var service = CreateService(handler);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.DeleteRecordAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, id!, null,
            TestContext.Current.CancellationToken));

        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task GetRecordAsync_DoesNotExposeBackendResponseBodyOnFailure()
    {
        var handler = JsonHandler(HttpStatusCode.NotFound, "sensitive backend details");
        var service = CreateService(handler);

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => service.GetRecordAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, RecordId, null, null, null,
            TestContext.Current.CancellationToken));

        Assert.Equal((int)HttpStatusCode.NotFound, exception.Status);
        Assert.DoesNotContain("sensitive backend details", exception.Message);
    }

    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("https://sample.energy.azure.com.evil.example")]
    [InlineData("http://sample.energy.azure.com")]
    public async Task GetRecordAsync_RejectsUntrustedEndpoint(string endpoint)
    {
        var handler = JsonHandler(HttpStatusCode.OK, "{}");
        var service = CreateService(handler);

        await Assert.ThrowsAsync<System.Security.SecurityException>(() => service.GetRecordAsync(
            endpoint, TestConstants.DataPartition, RecordId, null, null, null,
            TestContext.Current.CancellationToken));
    }

    private static StorageService CreateService(StubHttpMessageHandler handler) =>
        new(CreateCredentialProvider(), new FakeHttpClientFactory(handler));

    private static IAzureTokenCredentialProvider CreateCredentialProvider(
        string token = "fake-token", string? tenant = null)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1)));
        var provider = Substitute.For<IAzureTokenCredentialProvider>();
        provider.GetTokenCredentialAsync(tenant, Arg.Any<CancellationToken>()).Returns(credential);
        return provider;
    }

    private static StubHttpMessageHandler JsonHandler(HttpStatusCode status, string content) =>
        new(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json"),
        });

    private static StorageRecord CreateRecord() => new()
    {
        Id = RecordId,
        Kind = TestConstants.WellKind,
        Acl = new RecordAcl
        {
            Viewers = [$"viewers@{TestConstants.DataPartition}"],
            Owners = [$"owners@{TestConstants.DataPartition}"],
        },
        Legal = new RecordLegal
        {
            LegalTags = [$"{TestConstants.DataPartition}-public-usa"],
            OtherRelevantDataCountries = ["US"],
        },
        Data = JsonDocument.Parse("""{"FacilityName":"Well 99"}""").RootElement.Clone(),
    };

    private static Dictionary<string, string> ParseQuery(string query) =>
        query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .ToDictionary(
                part => Uri.UnescapeDataString(part[0]),
                part => Uri.UnescapeDataString(part[1]));
}
