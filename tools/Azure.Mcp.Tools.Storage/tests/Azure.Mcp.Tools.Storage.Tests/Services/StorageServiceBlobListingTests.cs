// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Web;
using Azure.Core;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Storage.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Storage.Tests.Services;

public sealed class StorageServiceBlobListingTests()
{
    // cspell:ignore AAEC
    private const string BlobEntry = """
        <Blob>
          <Name>folder/file.txt</Name>
          <Properties>
            <Creation-Time>Wed, 01 Jan 2025 00:00:00 GMT</Creation-Time>
            <Last-Modified>Wed, 01 Jan 2025 00:00:00 GMT</Last-Modified>
            <Etag>"test-etag"</Etag>
            <Content-Length>42</Content-Length>
            <Content-Type>text/plain</Content-Type>
            <Content-MD5>AAECAw==</Content-MD5>
            <BlobType>BlockBlob</BlobType>
            <LeaseStatus>unlocked</LeaseStatus>
            <LeaseState>available</LeaseState>
            <AccessTier>Hot</AccessTier>
          </Properties>
          <Metadata><category>test</category></Metadata>
        </Blob>
        """;

    [Fact]
    public async Task ListBlobs_Hierarchy_ReturnsOnePageOfBlobsAndPrefixes()
    {
        var requestCount = 0;
        const string continuationToken = "opaque +/=&token";
        using var httpClient = new HttpClient(new StorageTestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(1, ++requestCount);
            Assert.True(cancellationToken.CanBeCanceled);
            Assert.Equal("testaccount.blob.core.windows.net", request.RequestUri!.Host);
            Assert.Equal("/container", request.RequestUri.AbsolutePath);
            var query = HttpUtility.ParseQueryString(request.RequestUri.Query);
            Assert.Equal("list", query["comp"]);
            Assert.Equal("container", query["restype"]);
            Assert.Equal("folder/", query["prefix"]);
            Assert.Equal("/", query["delimiter"]);
            Assert.Equal("100", query["maxresults"]);
            Assert.Equal(continuationToken, query["marker"]);

            return CreateListResponse(
                "<BlobPrefix><Name>folder/subfolder/</Name></BlobPrefix>" + BlobEntry,
                "next +/=&token");
        }));
        var service = CreateService(httpClient);

        var result = await service.ListBlobs(
            "testaccount", "container", "/", "folder/", continuationToken,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, requestCount);
        Assert.Equal(["folder/subfolder/"], result.Prefixes);
        Assert.Equal(2, result.Blobs.Count + result.Prefixes.Count);
        Assert.Equal("next +/=&token", result.NextCursor);
        var blob = Assert.Single(result.Blobs);
        Assert.Equal("folder/file.txt", blob.Name);
        Assert.Equal(42, blob.ContentLength);
        Assert.Equal("text/plain", blob.ContentType);
        Assert.Equal("\"test-etag\"", blob.ETag);
        Assert.Equal("Block", blob.BlobType);
        Assert.Equal(new byte[] { 0, 1, 2, 3 }, blob.ContentHash);
        Assert.Equal("test", blob.Metadata["category"]);
        Assert.Equal("Hot", blob.AccessTier);
        Assert.Equal(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), blob.LastModified);
    }

    [Fact]
    public async Task ListBlobs_Hierarchy_ResumesAfterAnEmptyPage()
    {
        var requestCount = 0;
        using var httpClient = new HttpClient(new StorageTestHttpMessageHandler((request, _) =>
        {
            var query = HttpUtility.ParseQueryString(request.RequestUri!.Query);
            Assert.Equal("/", query["delimiter"]);
            Assert.Equal("100", query["maxresults"]);
            if (++requestCount == 1)
            {
                Assert.True(string.IsNullOrEmpty(query["marker"]));
                return CreateListResponse(string.Empty, "next +/=&token");
            }

            Assert.Equal("next +/=&token", query["marker"]);
            return CreateListResponse("<BlobPrefix><Name>folder/subfolder/</Name></BlobPrefix>" + BlobEntry);
        }));
        var service = CreateService(httpClient);
        var first = await service.ListBlobs("testaccount", "container", "/", "folder/", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Empty(first.Blobs);
        Assert.Empty(first.Prefixes);
        Assert.Equal("next +/=&token", first.NextCursor);
        Assert.Equal(1, requestCount);

        var last = await service.ListBlobs("testaccount", "container", "/", "folder/", first.NextCursor, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Single(last.Blobs);
        Assert.Equal(["folder/subfolder/"], last.Prefixes);
        Assert.Null(last.NextCursor);
        Assert.Equal(2, requestCount);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("::")]
    [InlineData(" ")]
    public async Task ListBlobs_EmptyPage_PreservesContinuationToken(string delimiter)
    {
        using var httpClient = new HttpClient(new StorageTestHttpMessageHandler((request, _) =>
        {
            var query = HttpUtility.ParseQueryString(request.RequestUri!.Query);
            Assert.Equal("folder +/\u96EA/", query["prefix"]);
            Assert.Equal(delimiter, query["delimiter"]);
            Assert.Equal("100", query["maxresults"]);
            Assert.Equal("previous +/=&token", query["marker"]);
            return CreateListResponse(string.Empty, "next-page");
        }));
        var service = CreateService(httpClient);

        var result = await service.ListBlobs(
            "testaccount", "container", delimiter, "folder +/\u96EA/", "previous +/=&token",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(result.Blobs);
        Assert.Empty(result.Prefixes);
        Assert.Equal("next-page", result.NextCursor);
    }

    [Fact]
    public async Task ListBlobs_EmptyFinalPage_ReturnsNoContinuationToken()
    {
        using var httpClient = new HttpClient(new StorageTestHttpMessageHandler((_, _) =>
            CreateListResponse(string.Empty)));
        var service = CreateService(httpClient);

        var result = await service.ListBlobs(
            "testaccount", "container", "/", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(result.Blobs);
        Assert.Empty(result.Prefixes);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task ListBlobs_EmptyDelimiter_ThrowsBeforeCreatingClient()
    {
        var azureService = AzureServiceTestHelpers.CreateAzureService();
        var service = new StorageService(azureService);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ListBlobs(
            "testaccount", "container", string.Empty, cancellationToken: TestContext.Current.CancellationToken));

        azureService.DidNotReceive().GetClient();
    }

    [Fact]
    public async Task ListBlobs_CanceledRequest_PropagatesCancellation()
    {
        using var httpClient = new HttpClient(new StorageTestHttpMessageHandler((_, _) =>
            throw new InvalidOperationException("A canceled listing must not send a request.")));
        var service = CreateService(httpClient);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ListBlobs(
            "testaccount", "container", "/", cancellationToken: cancellation.Token));
    }

    private static StorageService CreateService(HttpClient httpClient)
    {
        var azureService = AzureServiceTestHelpers.CreateAzureService();
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1))));
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        azureService.GetClient().Returns(httpClient);
        return new StorageService(azureService);
    }

    private static HttpResponseMessage CreateListResponse(string entries, string? nextMarker = null) => new(HttpStatusCode.OK)
    {
        Content = new StringContent($$"""
            <?xml version="1.0" encoding="utf-8"?>
            <EnumerationResults ServiceEndpoint="https://testaccount.blob.core.windows.net/" ContainerName="container">
              <Blobs>{{entries}}</Blobs>
              <NextMarker>{{WebUtility.HtmlEncode(nextMarker)}}</NextMarker>
            </EnumerationResults>
            """, Encoding.UTF8, "application/xml")
    };
}
