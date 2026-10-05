// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Net;
using System.Text;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Azure.Mcp.Tools.AzureMigrate.Tests.TestSupport;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.PlatformLandingZone;

public sealed class PlatformLandingZoneServiceTests()
{
    [Fact]
    public async Task DownloadAsync_UsesRegisteredArtifactVersionAndWritesGeneratedArchive()
    {
        const string ProjectId = "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/test-rg/providers/Microsoft.Migrate/migrateProjects/test-project";
        const string ArtifactId = ProjectId + "/artifacts/plz-default";
        const string DownloadUrl = "https://test.blob.core.windows.net/artifacts/output.zip?sig=test-signature";
        const string Terraform = "terraform { required_version = \">= 1.9\" }";
        var context = new PlatformLandingZoneContext(
            "00000000-0000-0000-0000-000000000001", "test-rg", "test-project", "default");

        using var archiveStream = new MemoryStream();
        using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            await using var writer = new StreamWriter(archive.CreateEntry("main.tf").Open());
            await writer.WriteAsync(Terraform);
        }
        var archiveBytes = archiveStream.ToArray();

        (HttpMethod Method, string Url)[] expectedRequests =
        [
            (HttpMethod.Get, $"https://management.azure.com{ProjectId}/platformLandingZones/default?api-version=2026-02-01-preview"),
            (HttpMethod.Get, $"https://management.azure.com{ArtifactId}?api-version=2026-06-15-preview"),
            (HttpMethod.Post, $"https://management.azure.com{ArtifactId}/generateDownloadUrl?api-version=2026-06-15-preview"),
            (HttpMethod.Get, DownloadUrl)
        ];
        var requestIndex = 0;
        using var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.True(requestIndex < expectedRequests.Length, "Unexpected HTTP request.");
            var index = requestIndex++;
            Assert.Equal(expectedRequests[index].Method, request.Method);
            Assert.Equal(expectedRequests[index].Url, request.RequestUri!.AbsoluteUri);

            if (index == 3)
            {
                Assert.Null(request.Headers.Authorization);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(archiveBytes)
                };
            }

            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            if (index == 2)
            {
                Assert.NotNull(request.Content);
                Assert.Equal("application/json", request.Content.Headers.ContentType?.MediaType);
                Assert.Equal("{\"mode\":\"file\",\"path\":\"output.zip\"}",
                    await request.Content.ReadAsStringAsync(cancellationToken));
            }
            else
            {
                Assert.Null(request.Content);
            }

            var body = index switch
            {
                0 => $$$"""{"name":"default","properties":{"status":"Succeeded","provisioningState":"Succeeded","artifactId":"{{{ArtifactId}}}"}}""",
                1 => """{"properties":{"latestVersion":1}}""",
                _ => $$"""{"sasUrl":"{{DownloadUrl}}"}"""
            };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        });
        var azureService = Substitute.For<IAzureService>();
        var cloud = Substitute.For<IAzureCloudConfiguration>();
        cloud.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        azureService.CloudConfiguration.Returns(cloud);
        azureService.GetClient().Returns(_ => new HttpClient(handler, disposeHandler: false));
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.MaxValue)));
        azureService.GetTokenCredentialAsync(null, Arg.Any<CancellationToken>()).Returns(credential);
        var service = new PlatformLandingZoneService(azureService);
        var outputDirectory = Path.Combine(Path.GetTempPath(), $"plz-download-{Guid.NewGuid():N}");

        try
        {
            var files = await service.DownloadAsync(
                context, outputDirectory, includeDesignDocument: false, TestContext.Current.CancellationToken);

            var path = Assert.Single(files);
            Assert.Equal(Path.Combine(outputDirectory, "test-project-default-output.zip"), path);
            Assert.Equal(archiveBytes, await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
            using var downloaded = ZipFile.OpenRead(path);
            var entry = Assert.Single(downloaded.Entries);
            Assert.Equal("main.tf", entry.FullName);
            using var reader = new StreamReader(entry.Open());
            Assert.Equal(Terraform, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
            Assert.Equal(expectedRequests.Length, requestIndex);
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }
}
