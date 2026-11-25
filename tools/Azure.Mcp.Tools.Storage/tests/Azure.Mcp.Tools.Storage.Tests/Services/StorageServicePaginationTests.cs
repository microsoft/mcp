// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Azure.Core;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Storage.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Storage.Tests.Services;

public sealed class StorageServicePaginationTests()
{
    [Theory]
    [InlineData("blobs", false)]
    [InlineData("blobs", true)]
    [InlineData("containers", false)]
    [InlineData("containers", true)]
    [InlineData("tables", false)]
    [InlineData("tables", true)]
    public async Task Listing_ReturnsOnlyOneFixedSizePageAndResumes(string operation, bool emptyFirstPage)
    {
        const string marker = "opaque +/=%& token";
        var requests = new List<Uri>();
        using var handler = new PaginationHttpMessageHandler(request =>
        {
            requests.Add(request.RequestUri!);
            var firstPage = requests.Count == 1;
            return CreateResponse(operation, firstPage ? emptyFirstPage ? 0 : 100 : 1, firstPage ? marker : null);
        });
        using var httpClient = new HttpClient(handler);
        var service = CreateService(httpClient);

        var first = await GetPageAsync(service, operation, null, TestContext.Current.CancellationToken);

        Assert.Equal(emptyFirstPage ? 0 : 100, first.Names.Count);
        Assert.False(string.IsNullOrEmpty(first.NextCursor));
        Assert.Single(requests);

        var last = await GetPageAsync(service, operation, first.NextCursor, TestContext.Current.CancellationToken);

        Assert.Single(last.Names);
        Assert.Null(last.NextCursor);
        Assert.Equal(2, requests.Count);
        foreach (var request in requests)
        {
            var query = HttpUtility.ParseQueryString(request.Query);
            // cspell:ignore maxresults
            Assert.Equal("100", query[operation == "tables" ? "$top" : "maxresults"]);
            if (operation != "tables")
            {
                Assert.Equal("prefix", query["prefix"]);
            }
        }

        var continuedQuery = HttpUtility.ParseQueryString(requests[1].Query);
        Assert.Equal(marker, continuedQuery[operation == "tables" ? "NextTableName" : "marker"]);
    }

    [Theory]
    [InlineData("blobs")]
    [InlineData("containers")]
    [InlineData("tables")]
    public async Task Listing_InvalidContinuationFailsWithoutRestarting(string operation)
    {
        var requests = 0;
        using var handler = new PaginationHttpMessageHandler(_ =>
        {
            requests++;
            if (requests == 1)
            {
                return CreateResponse(operation, 0, "expired-marker");
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(
                    operation == "tables"
                        ? """{"odata.error":{"code":"InvalidInput","message":{"lang":"en-US","value":"Invalid continuation."}}}"""
                        : "<Error><Code>InvalidMarker</Code><Message>Invalid continuation.</Message></Error>",
                    Encoding.UTF8, operation == "tables" ? "application/json" : "application/xml")
            };
        });
        using var httpClient = new HttpClient(handler);
        var service = CreateService(httpClient);
        var first = await GetPageAsync(service, operation, null, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<RequestFailedException>(() =>
            GetPageAsync(service, operation, first.NextCursor, TestContext.Current.CancellationToken));

        Assert.Equal(2, requests);
    }

    private static StorageService CreateService(HttpClient httpClient)
    {
        var azureService = AzureServiceTestHelpers.CreateAzureService();
        azureService.GetClient().Returns(httpClient);
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        return new StorageService(azureService);
    }

    private static async Task<(List<string> Names, string? NextCursor)> GetPageAsync(
        StorageService service, string operation, string? cursor, CancellationToken cancellationToken)
    {
        switch (operation)
        {
            case "blobs":
                var blobs = await service.GetBlobDetails(
                    "testaccount", "container", null, "prefix", cursor, "tenant", cancellationToken);
                return (blobs.Blobs.Select(blob => blob.Name).ToList(), blobs.NextCursor);
            case "containers":
                var containers = await service.GetContainerDetails(
                    "testaccount", null, "prefix", cursor, "tenant", cancellationToken);
                return (containers.Containers.Select(container => container.Name).ToList(), containers.NextCursor);
            case "tables":
                return await service.ListTables("testaccount", cursor, "tenant", cancellationToken);
            default:
                throw new ArgumentOutOfRangeException(nameof(operation));
        }
    }

    private static HttpResponseMessage CreateResponse(string operation, int count, string? marker)
    {
        if (operation == "tables")
        {
            var tables = string.Join(",", Enumerable.Range(0, count).Select(index => $"{{\"TableName\":\"Table{index:D3}\"}}"));
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"value\":[{tables}]}}", Encoding.UTF8, "application/json")
            };
            if (marker is not null)
            {
                response.Headers.Add("x-ms-continuation-NextTableName", marker);
            }
            return response;
        }

        var items = Enumerable.Range(0, count).Select(index => new XElement(
            operation == "blobs" ? "Blob" : "Container",
            new XElement("Name", $"prefix{index:D3}"),
            new XElement("Properties",
                new XElement("Last-Modified", "Wed, 04 Nov 2026 12:00:00 GMT"),
                new XElement("Etag", "\"etag\""),
                new XElement("Content-Length", 1),
                new XElement("Content-Type", "text/plain"),
                new XElement("BlobType", "BlockBlob"))));
        var xml = new XElement("EnumerationResults",
            new XAttribute("ServiceEndpoint", "https://testaccount.blob.core.windows.net/"),
            new XElement(operation == "blobs" ? "Blobs" : "Containers", items),
            new XElement("NextMarker", marker ?? string.Empty));
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(xml.ToString(), Encoding.UTF8, "application/xml")
        };
    }
}
