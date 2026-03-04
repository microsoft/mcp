// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Fabric.Mcp.Tools.OneLake.Tests.TestSupport;
using Xunit;

namespace Fabric.Mcp.Tools.OneLake.Tests.Services;

public class OneLakeServiceUserAgentTests
{
    [Fact]
    public async Task ReadFileAsync_AttachesOneLakeMcpUserAgent()
    {
        // Arrange
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.RequestUri is { } uri && uri.Query.Contains("comp=list", StringComparison.OrdinalIgnoreCase))
            {
                const string xml = """
<?xml version="1.0" encoding="utf-8"?>
<EnumerationResults>
    <Blobs>
        <BlobPrefix>
            <Name>item.Lakehouse/</Name>
            <Properties>
                <Creation-Time>2024-01-01T00:00:00Z</Creation-Time>
                <Last-Modified>2024-01-01T00:00:00Z</Last-Modified>
            </Properties>
            <Metadata>
                <ArtifactId>item.Lakehouse</ArtifactId>
            </Metadata>
        </BlobPrefix>
    </Blobs>
</EnumerationResults>
""";

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(xml, Encoding.UTF8, "application/xml")
                };
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent("sample"u8.ToArray())
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentLength = 6;
            return response;
        });

        using var httpClient = new HttpClient(handler);
        var credential = new FakeTokenCredential();
        var service = new OneLakeService(httpClient, credential);

        var options = new BlobDownloadOptions { IncludeInlineContent = true };

        // Act
        await service.ReadFileAsync("workspace", "item", "Files/path/to/file.txt", options, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(handler.LastRequest);
        Assert.True(handler.LastRequest!.Headers.TryGetValues("User-Agent", out var values));
        var userAgent = string.Join(" ", values);
        Assert.Contains("OneLake MCP", userAgent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListOneLakeWorkspacesXmlAsync_AttachesOneLakeMcpUserAgent()
    {
        // Arrange
        var handler = new CapturingHttpMessageHandler(request =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<Containers />", Encoding.UTF8, "application/xml")
            };
            return response;
        });

        using var httpClient = new HttpClient(handler);
        var credential = new FakeTokenCredential();
        var service = new OneLakeService(httpClient, credential);

        // Act
        await service.ListOneLakeWorkspacesXmlAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(handler.LastRequest);
        Assert.True(handler.LastRequest!.Headers.TryGetValues("User-Agent", out var values));
        var userAgent = string.Join(" ", values);
        Assert.Contains("OneLake MCP", userAgent, StringComparison.Ordinal);
    }
}
