// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Options;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Functions.UnitTests.Services;

/// <summary>
/// Tests for FunctionsService async HTTP methods using mocked HttpClient.
/// </summary>
public sealed class FunctionsServiceHttpTests
{
    private readonly ICacheService _cacheService;
    private readonly ILanguageMetadataProvider _languageMetadata;
    private readonly IManifestService _manifestService;
    private readonly ILogger<FunctionsService> _logger;
    private readonly IOptions<FunctionsOptions> _functionsOptions;

    // Derive URL patterns from default options to avoid duplication
    private static readonly FunctionsOptions s_defaultOptions = new();
    private static string PrimaryUrlHost => new Uri(s_defaultOptions.ManifestUrl).Host;
    private static string FallbackUrlHost => new Uri(s_defaultOptions.FallbackManifestUrl).Host;

    public FunctionsServiceHttpTests()
    {
        _cacheService = Substitute.For<ICacheService>();
        _languageMetadata = new LanguageMetadataProvider();
        _manifestService = Substitute.For<IManifestService>();
        _logger = Substitute.For<ILogger<FunctionsService>>();
        _functionsOptions = Microsoft.Extensions.Options.Options.Create(new FunctionsOptions());
    }

    private FunctionsService CreateService(IHttpClientFactory httpClientFactory) =>
        new(httpClientFactory, _languageMetadata, _manifestService, _cacheService, _logger);

    private ManifestService CreateManifestService(IHttpClientFactory httpClientFactory)
    {
        var manifestLogger = Substitute.For<ILogger<ManifestService>>();
        return new ManifestService(httpClientFactory, _cacheService, _functionsOptions, manifestLogger);
    }

    private static TemplateManifestEntry CreateTestEntry(string language = "python", string folderPath = "templates/python/HttpTrigger", string id = "HttpTrigger") =>
        new()
        {
            Id = id,
            DisplayName = "Test Template",
            Language = language,
            RepositoryUrl = "https://github.com/Azure/templates",
            FolderPath = folderPath,
            LongDescription = "Test description"
        };

    #region FetchManifestAsync Tests

    [Fact]
    public async Task FetchManifestAsync_ReturnsTemplates_WhenCdnReturnsValidJson()
    {
        // Arrange
        var manifest = new TemplateManifest
        {
            Version = "1.0",
            Templates = [CreateTestEntry()]
        };
        var json = JsonSerializer.Serialize(manifest, FunctionsJsonContext.Default.TemplateManifest);
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateManifestService(httpClientFactory);

        // Act
        var result = await service.FetchManifestAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Templates);
        Assert.Equal("python", result.Templates[0].Language);
    }

    [Fact]
    public async Task FetchManifestAsync_ThrowsInvalidOperationException_WhenCdnReturns404()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("Not Found", HttpStatusCode.NotFound);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateManifestService(httpClientFactory);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.FetchManifestAsync(CancellationToken.None));
    }

    [Fact]
    public async Task FetchManifestAsync_ThrowsInvalidOperationException_WhenJsonMalformed()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("{ invalid json }", HttpStatusCode.OK);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateManifestService(httpClientFactory);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.FetchManifestAsync(CancellationToken.None));
    }

    [Fact]
    public async Task FetchManifestAsync_ReturnsCached_WhenCacheHasValidData()
    {
        // Arrange
        var cachedManifest = new TemplateManifest
        {
            Version = "cached",
            Templates = [CreateTestEntry("cached-lang")]
        };
        _cacheService.GetAsync<TemplateManifest>("functions", "manifest", Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<TemplateManifest?>(cachedManifest));

        var handler = new MockHttpMessageHandler("should not be called", HttpStatusCode.OK);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateManifestService(httpClientFactory);

        // Act
        var result = await service.FetchManifestAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cached", result.Version);
        Assert.False(handler.WasCalled); // Should use cache, not HTTP
    }

    [Fact]
    public async Task FetchManifestAsync_FetchesFresh_WhenCacheHasEmptyTemplates()
    {
        // Arrange - cache has manifest but with empty templates (corrupted)
        var corruptedCache = new TemplateManifest { Version = "corrupted", Templates = [] };
        _cacheService.GetAsync<TemplateManifest>("functions", "manifest", Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<TemplateManifest?>(corruptedCache));

        var freshManifest = new TemplateManifest
        {
            Version = "fresh",
            Templates = [CreateTestEntry()]
        };
        var json = JsonSerializer.Serialize(freshManifest, FunctionsJsonContext.Default.TemplateManifest);
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateManifestService(httpClientFactory);

        // Act
        var result = await service.FetchManifestAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fresh", result.Version);
        Assert.True(handler.WasCalled); // Should fetch fresh data
    }

    [Fact]
    public async Task FetchManifestAsync_UsesFallback_WhenPrimaryFails()
    {
        // Arrange - primary returns 404, fallback returns valid manifest
        var fallbackManifest = new TemplateManifest
        {
            Version = "fallback",
            Templates = [CreateTestEntry("fallback-lang")]
        };
        var fallbackJson = JsonSerializer.Serialize(fallbackManifest, FunctionsJsonContext.Default.TemplateManifest);

        var responses = new Dictionary<string, (string Content, HttpStatusCode Status)>
        {
            [PrimaryUrlHost] = ("Not Found", HttpStatusCode.NotFound),
            [FallbackUrlHost] = (fallbackJson, HttpStatusCode.OK)
        };
        var handler = new MultiResponseHttpMessageHandler(responses);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateManifestService(httpClientFactory);

        // Act
        var result = await service.FetchManifestAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fallback", result.Version);
        Assert.Single(result.Templates);
    }

    [Fact]
    public async Task FetchManifestAsync_UsesFallback_WhenPrimaryReturnsMalformedJson()
    {
        // Arrange - primary returns malformed JSON, fallback returns valid
        var fallbackManifest = new TemplateManifest
        {
            Version = "fallback-after-malformed",
            Templates = [CreateTestEntry()]
        };
        var fallbackJson = JsonSerializer.Serialize(fallbackManifest, FunctionsJsonContext.Default.TemplateManifest);

        var responses = new Dictionary<string, (string Content, HttpStatusCode Status)>
        {
            [PrimaryUrlHost] = ("{ invalid json }", HttpStatusCode.OK),
            [FallbackUrlHost] = (fallbackJson, HttpStatusCode.OK)
        };
        var handler = new MultiResponseHttpMessageHandler(responses);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateManifestService(httpClientFactory);

        // Act
        var result = await service.FetchManifestAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fallback-after-malformed", result.Version);
    }

    [Fact]
    public async Task FetchManifestAsync_Throws_WhenBothPrimaryAndFallbackFail()
    {
        // Arrange - both URLs return 404
        var responses = new Dictionary<string, (string Content, HttpStatusCode Status)>
        {
            [PrimaryUrlHost] = ("Primary Not Found", HttpStatusCode.NotFound),
            [FallbackUrlHost] = ("Fallback Not Found", HttpStatusCode.NotFound)
        };
        var handler = new MultiResponseHttpMessageHandler(responses);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateManifestService(httpClientFactory);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.FetchManifestAsync(CancellationToken.None));

        Assert.Contains("primary", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fallback", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FetchManifestAsync_UsesPrimary_WhenPrimarySucceeds()
    {
        // Arrange - primary succeeds, fallback should not be called
        var primaryManifest = new TemplateManifest
        {
            Version = "primary",
            Templates = [CreateTestEntry("primary-lang")]
        };
        var primaryJson = JsonSerializer.Serialize(primaryManifest, FunctionsJsonContext.Default.TemplateManifest);

        var responses = new Dictionary<string, (string Content, HttpStatusCode Status)>
        {
            [PrimaryUrlHost] = (primaryJson, HttpStatusCode.OK),
            [FallbackUrlHost] = ("should not be called", HttpStatusCode.InternalServerError)
        };
        var handler = new MultiResponseHttpMessageHandler(responses);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateManifestService(httpClientFactory);

        // Act
        var result = await service.FetchManifestAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("primary", result.Version);
    }

    #endregion

    #region FetchTemplateFilesViaArchiveAsync Tests

    [Fact]
    public async Task FetchTemplateFilesViaArchiveAsync_ExtractsFiles_FromZipball()
    {
        // Arrange - create a real ZIP in memory
        var zipBytes = CreateTestZipArchive(new Dictionary<string, string>
        {
            ["Azure-repo-abc123/templates/python/function.py"] = "def main(): pass",
            ["Azure-repo-abc123/templates/python/host.json"] = "{\"version\": \"2.0\"}"
        });
        var handler = new MockHttpMessageHandler(zipBytes, HttpStatusCode.OK);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateService(httpClientFactory);

        // Act
        var result = await service.FetchTemplateFilesViaArchiveAsync(
            "https://github.com/Azure/repo",
            "templates/python",
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.FileName == "function.py");
        Assert.Contains(result, f => f.FileName == "host.json");
    }

    [Fact]
    public async Task FetchTemplateFilesViaArchiveAsync_SkipsPathTraversal_InZipEntries()
    {
        // Arrange - ZIP with malicious path traversal entry
        var zipBytes = CreateTestZipArchive(new Dictionary<string, string>
        {
            ["Azure-repo-abc123/templates/python/safe.py"] = "safe content",
            ["Azure-repo-abc123/templates/../../../etc/passwd"] = "malicious content"
        });
        var handler = new MockHttpMessageHandler(zipBytes, HttpStatusCode.OK);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateService(httpClientFactory);

        // Act
        var result = await service.FetchTemplateFilesViaArchiveAsync(
            "https://github.com/Azure/repo",
            ".",
            CancellationToken.None);

        // Assert - should skip the path traversal entry
        Assert.DoesNotContain(result, f => f.FileName.Contains(".."));
        Assert.DoesNotContain(result, f => f.FileName.Contains("passwd"));
    }

    [Fact]
    public async Task FetchTemplateFilesViaArchiveAsync_SkipsOversizedFiles()
    {
        // Arrange - ZIP with a file larger than 1MB limit
        var largeContent = new string('x', 2_000_000); // 2MB
        var zipBytes = CreateTestZipArchive(new Dictionary<string, string>
        {
            ["Azure-repo-abc123/small.txt"] = "small",
            ["Azure-repo-abc123/large.txt"] = largeContent
        });
        var handler = new MockHttpMessageHandler(zipBytes, HttpStatusCode.OK);
        var httpClientFactory = CreateHttpClientFactory(handler);
        var service = CreateService(httpClientFactory);

        // Act
        var result = await service.FetchTemplateFilesViaArchiveAsync(
            "https://github.com/Azure/repo",
            ".",
            CancellationToken.None);

        // Assert - should skip the oversized file
        Assert.Single(result);
        Assert.Equal("small.txt", result[0].FileName);
    }

    #endregion

    #region Tree API Error Handling Tests

    [Fact]
    public async Task GetFunctionTemplateAsync_ThrowsInvalidOperationException_WhenRateLimited()
    {
        // Arrange - mock rate limit response for Tree API
        // Include X-RateLimit-Remaining: 0 to indicate rate limiting (not just permission denied)
        var handler = new MockHttpMessageHandlerWithHeaders(
            "Rate limit exceeded",
            HttpStatusCode.Forbidden,
            new Dictionary<string, string>
            {
                ["X-RateLimit-Remaining"] = "0",
                ["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds().ToString()
            });

        var httpClientFactory = CreateHttpClientFactory(handler);

        // Set up manifest with a template entry
        var manifest = new TemplateManifest
        {
            Version = "1.0",
            Templates = [CreateTestEntry("python", "templates/python/HttpTrigger")]
        };
        _manifestService.FetchManifestAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(manifest));

        var service = CreateService(httpClientFactory);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetFunctionTemplateAsync("python", "HttpTrigger", null, TemplateOutput.New, CancellationToken.None));

        Assert.Contains("rate limit", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetFunctionTemplateAsync_ThrowsInvalidOperationException_WhenNetworkFails()
    {
        // Arrange - mock network failure
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        var httpClientFactory = CreateHttpClientFactory(handler);

        var manifest = new TemplateManifest
        {
            Version = "1.0",
            Templates = [CreateTestEntry("python", "templates/python/HttpTrigger")]
        };
        _manifestService.FetchManifestAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(manifest));

        var service = CreateService(httpClientFactory);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetFunctionTemplateAsync("python", "HttpTrigger", null, TemplateOutput.New, CancellationToken.None));

        Assert.Contains("network", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetFunctionTemplateAsync_ThrowsInvalidOperationException_WhenEmptyTreeReturned()
    {
        // Arrange - mock empty tree response
        var emptyTree = new GitHubTreeResponse { Sha = "abc", Tree = [], Truncated = false };
        var json = JsonSerializer.Serialize(emptyTree, FunctionsJsonContext.Default.GitHubTreeResponse);
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var httpClientFactory = CreateHttpClientFactory(handler);

        var manifest = new TemplateManifest
        {
            Version = "1.0",
            Templates = [CreateTestEntry("python", "templates/python/HttpTrigger")]
        };
        _manifestService.FetchManifestAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(manifest));

        var service = CreateService(httpClientFactory);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetFunctionTemplateAsync("python", "HttpTrigger", null, TemplateOutput.New, CancellationToken.None));

        Assert.Contains("empty", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Helper Methods

    private static IHttpClientFactory CreateHttpClientFactory(HttpMessageHandler handler)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler));
        return factory;
    }

    private static byte[] CreateTestZipArchive(Dictionary<string, string> files)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var (path, content) in files)
            {
                var entry = archive.CreateEntry(path);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream);
                writer.Write(content);
            }
        }

        return memoryStream.ToArray();
    }

    #endregion

    #region Mock HTTP Handlers

    /// <summary>
    /// Simple mock HTTP handler that returns a fixed response.
    /// Creates a new response for each request to avoid disposal issues.
    /// </summary>
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string? _stringContent;
        private readonly byte[]? _byteContent;
        private readonly HttpStatusCode _statusCode;
        public bool WasCalled { get; private set; }

        public MockHttpMessageHandler(string content, HttpStatusCode statusCode)
        {
            _stringContent = content;
            _statusCode = statusCode;
        }

        public MockHttpMessageHandler(byte[] content, HttpStatusCode statusCode)
        {
            _byteContent = content;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = _byteContent is not null
                    ? new ByteArrayContent(_byteContent)
                    : new StringContent(_stringContent!, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// Mock HTTP handler that returns different responses based on URL patterns.
    /// </summary>
    private sealed class MultiResponseHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, (string Content, HttpStatusCode Status)> _responses;

        public MultiResponseHttpMessageHandler(Dictionary<string, (string Content, HttpStatusCode Status)> responses)
        {
            _responses = responses;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri?.ToString() ?? "";

            foreach (var (pattern, (content, status)) in _responses)
            {
                if (url.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(new HttpResponseMessage(status)
                    {
                        Content = new StringContent(content, Encoding.UTF8, "application/json")
                    });
                }
            }

            // Default: 404
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not Found")
            });
        }
    }

    /// <summary>
    /// Mock HTTP handler with custom response headers (for rate limit testing).
    /// </summary>
    private sealed class MockHttpMessageHandlerWithHeaders : HttpMessageHandler
    {
        private readonly string _content;
        private readonly HttpStatusCode _statusCode;
        private readonly Dictionary<string, string> _headers;

        public MockHttpMessageHandlerWithHeaders(string content, HttpStatusCode statusCode, Dictionary<string, string> headers)
        {
            _content = content;
            _statusCode = statusCode;
            _headers = headers;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, Encoding.UTF8, "application/json")
            };

            foreach (var (key, value) in _headers)
            {
                response.Headers.TryAddWithoutValidation(key, value);
            }

            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// Mock HTTP handler that throws an exception (for network failure testing).
    /// </summary>
    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw _exception;
        }
    }

    #endregion
}
