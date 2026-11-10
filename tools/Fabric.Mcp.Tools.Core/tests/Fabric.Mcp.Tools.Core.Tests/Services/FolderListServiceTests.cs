// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FolderListServiceTests()
{
    [Theory]
    [InlineData(null, null, "")]
    [InlineData(null, true, "?recursive=true")]
    [InlineData(null, false, "?recursive=false")]
    [InlineData(ParentFolderId, null, "?rootFolderId=" + ParentFolderId)]
    [InlineData(ParentFolderId, true, "?rootFolderId=" + ParentFolderId + "&recursive=true")]
    [InlineData(ParentFolderId, false, "?rootFolderId=" + ParentFolderId + "&recursive=false")]
    public static async Task List_SendsOneAuthenticatedRequestWithoutABodyAndWithOnlySuppliedFilters(string? root, bool? recursive, string query)
    {
        var page = JsonNode.Parse(Page())!;
        page["value"]![0]!["privateExtension"] = "private-content";
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(page.ToJsonString()));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.Location = new Uri("https://example.invalid/never-follow");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "not-an-operation");
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(CollectionUrl + query, request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("folder-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).ListFoldersAsync(
            Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant(), root is null ? null : Guid.Parse(root).ToString("N").ToUpperInvariant(),
            recursive, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Folder, Assert.Single(result.Folders));
        Assert.Null(result.ContinuationToken);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.FolderListCommandResult);
        Assert.Equal("folders", Assert.Single(output.EnumerateObject()).Name);
        Assert.Equal(["displayName", "id", "parentFolderId", "workspaceId"],
            output.GetProperty("folders")[0].EnumerateObject().Select(property => property.Name).Order());
        Assert.Equal(1, handler.CallCount);
        Assert.True(stream.Disposed);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public static async Task List_AcceptsAbsentOrNullContinuationOnEmptyAndNonemptyFinalPages(bool nullToken, bool nullUri, bool empty)
    {
        var page = JsonNode.Parse(Page(empty: empty))!.AsObject();
        if (nullToken)
        {
            page["continuationToken"] = null;
        }
        if (nullUri)
        {
            page["continuationUri"] = null;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(page.ToJsonString())));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));

        Assert.Equal(empty ? [] : new[] { Folder }, result.Folders);
        Assert.Null(result.ContinuationToken);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.FolderListCommandResult);
        Assert.False(output.TryGetProperty("continuationToken", out _));
        Assert.False(output.TryGetProperty("continuationUri", out _));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public static async Task List_ContinuesEmptyOrNonemptyPagesOnlyOnExplicitCallWithSameFilters(bool nullUri, bool empty)
    {
        var page = JsonNode.Parse(Page(DocumentedToken, empty: empty))!;
        if (nullUri)
        {
            page["continuationUri"] = null;
        }
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Null(request.Content);
            return Task.FromResult(Response(requests.Count == 1 ? page.ToJsonString() : """{"value":[]}"""));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        var url = $"{CollectionUrl}?rootFolderId={ParentFolderId}&recursive=false";

        var first = await service.ListFoldersAsync(WorkspaceId, ParentFolderId, false, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(empty ? [] : new[] { Folder }, first.Folders);
        Assert.Equal(DocumentedToken, first.ContinuationToken);
        Assert.Equal([url], requests);

        var second = await service.ListFoldersAsync(WorkspaceId, ParentFolderId, false, first.ContinuationToken, TestContext.Current.CancellationToken);

        Assert.Empty(second.Folders);
        Assert.Null(second.ContinuationToken);
        Assert.Equal([url, $"{url}&continuationToken={DocumentedToken}"], requests);
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData(DocumentedToken, DocumentedToken)]
    [InlineData("raw+/= &?#", "raw%2B%2F%3D%20%26%3F%23")]
    [InlineData(" raw ", "%20raw%20")]
    [InlineData("%3D%2f%26%3F%23%2B%25", "%3D%2f%26%3F%23%2B%25")]
    [InlineData("tail%2", "tail%252")]
    [InlineData("%G1%", "%25G1%25")]
    [InlineData("%2541%2526", "%2541%2526")]
    [InlineData("%41%7e%2D%5f%2E", "A~-_.")]
    [InlineData("\u00e9\U0001F680 /", "%C3%A9%F0%9F%9A%80%20%2F")]
    [InlineData("%0D%0A", "%0D%0A")]
    public static async Task List_PreservesOpaqueTokensAndEncodesOnlyRawSpans(string token, string encoded)
    {
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            Assert.Empty(request.RequestUri.Fragment);
            return Task.FromResult(Response(requests.Count == 1
                ? Page(token, $"{CollectionUrl}?continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(token)}")
                : """{"value":[]}"""));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());

        var first = await ReadAsync(service);
        Assert.Equal(token, first.ContinuationToken);
        Assert.Equal([CollectionUrl], requests);
        var second = await ReadAsync(service, first.ContinuationToken);

        Assert.Empty(second.Folders);
        Assert.Equal([CollectionUrl, $"{CollectionUrl}?continuationToken={encoded}"], requests);
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData("opaque/../cursor", "opaque/../cursor")]
    [InlineData("opaque/./cursor", "opaque/./cursor")]
    [InlineData("opaque/../cursor", "opaque/%2E%2E/cursor")]
    // cspell:disable-next-line
    [InlineData("opaque%2F..%2Fcursor", "opaque/../cursor")]
    public static async Task List_DoesNotTreatOpaqueQueryDotSegmentsAsPathTraversal(string token, string queryValue)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(new Uri(CollectionUrl).AbsolutePath, request.RequestUri!.AbsolutePath);
            return Task.FromResult(Response(Page(token, $"{CollectionUrl}?continuationToken={queryValue}")));
        });
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()), token);
        Assert.Equal(token, result.ContinuationToken);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null, null, "continuationToken=next")]
    [InlineData(null, null, "recursive=True&continuationToken=next")]
    [InlineData(null, true, "continuationToken=next&recursive=true")]
    [InlineData(null, false, "recursive=False&continuationToken=next")]
    [InlineData(ParentFolderId, false, "continuationToken=next")]
    [InlineData(ParentFolderId, false, "rootFolderId=" + ParentFolderId + "&continuationToken=next")]
    [InlineData(ParentFolderId, false, "continuationToken=next&recursive=false")]
    [InlineData(ParentFolderId, false, "recursive=false&continuationToken=next&rootFolderId=" + ParentFolderId)]
    [InlineData(ParentFolderId, true, "rootFolderId=" + ParentFolderId + "&recursive=true&continuationToken=next")]
    public static async Task List_AcceptsCursorOnlyOrConsistentFilterMetadataWithoutFollowingIt(string? root, bool? recursive, string query)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Page("next", $"{CollectionUrl}?{query}"))));
        using var client = new HttpClient(handler);

        var result = await new FabricCoreService(client, Credential()).ListFoldersAsync(
            WorkspaceId, root, recursive, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("next", result.ContinuationToken);
        Assert.DoesNotContain("continuationUri", JsonSerializer.Serialize(result, CoreJsonContext.Default.FolderListCommandResult));
        Assert.Equal(1, handler.CallCount);
    }

    public static TheoryData<string> UntrustedUris => new()
    {
        "",
        "https://example.invalid/private?continuationToken=next",
        CollectionUrl.Replace("https:", "http:", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("api.fabric.microsoft.com", "api.fabric.microsoft.com:444", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("api.fabric.microsoft.com", "user@api.fabric.microsoft.com", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace(WorkspaceId, FolderId, StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("/folders", "/./folders", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("/folders", "/private/../folders", StringComparison.Ordinal) + "?continuationToken=next",
        // cspell:disable-next-line
        CollectionUrl.Replace("/folders", "/%66olders", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("/folders", "\\folders", StringComparison.Ordinal) + "?continuationToken=next",
        "/v1/workspaces/" + WorkspaceId + "/folders?continuationToken=next",
        CollectionUrl + "/" + FolderId + "?continuationToken=next",
        CollectionUrl,
        CollectionUrl + "?continuationToken=wrong",
        CollectionUrl + "?continuationToken=next#private",
        CollectionUrl + "?continuationToken=next&extra=private",
        CollectionUrl + "?continuationToken=next&continuationToken=next",
        CollectionUrl + "?ContinuationToken=next",
        CollectionUrl + "?continuationToken=next&recursive",
        CollectionUrl + "?continuationToken=next&recursive=false&recursive=false",
        CollectionUrl + "?continuationToken=next&rootFolderId=" + ParentFolderId + "&rootFolderId=" + ParentFolderId,
        CollectionUrl + "?continuationToken=next&rootFolderId=" + FolderId,
        CollectionUrl + "?continuationToken=next&recursive=true",
        CollectionUrl + "?continuationToken=next&recursive=0",
        CollectionUrl + "?continuationToken=next&recursive=%20false",
        CollectionUrl + "?continuationToken=next&rootFolderId=private",
        CollectionUrl + "?continuationToken=next&rootFolderId=" + Guid.Empty,
        CollectionUrl + "?continuationToken=next&",
        CollectionUrl + "?recursive=false"
    };

    [Theory]
    [MemberData(nameof(UntrustedUris))]
    public static async Task List_RejectsUntrustedOrContradictoryContinuationUris(string uri) =>
        await AssertInvalidPageAsync(Page("next", uri), ParentFolderId, false);

    [Fact]
    public static async Task List_RejectsUriThatAddsARootNotRequestedOrChangesTheRecursiveDefault()
    {
        await AssertInvalidPageAsync(Page("next", $"{CollectionUrl}?continuationToken=next&rootFolderId={ParentFolderId}"));
        await AssertInvalidPageAsync(Page("next", $"{CollectionUrl}?continuationToken=next&recursive=false"));
    }

    [Theory]
    [InlineData("""{"continuationToken":""}""")]
    [InlineData("""{"continuationToken":" "}""")]
    [InlineData("""{"continuationToken":false}""")]
    [InlineData("""{"continuationToken":0}""")]
    [InlineData("""{"continuationToken":[]}""")]
    [InlineData("""{"continuationToken":{}}""")]
    [InlineData("""{"continuationToken":"private\r\n"}""")]
    [InlineData("""{"continuationToken":"\uD800"}""")]
    [InlineData("""{"continuationUri":""}""")]
    [InlineData("""{"continuationUri":false}""")]
    [InlineData("""{"continuationUri":0}""")]
    [InlineData("""{"continuationUri":[]}""")]
    [InlineData("""{"continuationUri":{}}""")]
    [InlineData("""{"continuationToken":null,"continuationUri":"https://example.invalid/private"}""")]
    [InlineData("""{"continuationToken":false,"continuationUri":null}""")]
    [InlineData("""{"continuationToken":"next","continuationUri":""}""")]
    [InlineData("""{"continuationToken":"next","continuationUri":42}""")]
    [InlineData("""{"continuationToken":"next","ContinuationToken":"another"}""")]
    public static async Task List_RejectsInvalidContinuationRatherThanImplyingCompletion(string metadata) =>
        await AssertInvalidPageAsync($"{{\"value\":[],{metadata[1..]}");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task List_RejectsEvenTrustedUriOnlyPages(bool nullToken)
    {
        var page = JsonNode.Parse(Page(uri: $"{CollectionUrl}?continuationToken=next"))!;
        if (nullToken)
        {
            page["continuationToken"] = null;
        }
        await AssertInvalidPageAsync(page.ToJsonString());
    }

    [Fact]
    public static async Task List_PreservesOrderRepeatedEntriesAndParentsOutsideThePageWithoutACountLimit()
    {
        var folders = Enumerable.Range(1, 1001).Select(index => Folder with
        {
            Id = Guid.Parse($"00000000-0000-4000-8000-{index:000000000000}"),
            DisplayName = $"Folder {index}"
        }).Reverse().ToList();
        folders.Add(folders[0]);
        var page = new JsonObject
        {
            ["value"] = new JsonArray(folders.Select(folder => JsonSerializer.SerializeToNode(folder, CoreJsonContext.Default.FabricFolder)).ToArray())
        };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(page.ToJsonString())));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));

        Assert.Equal(folders, result.Folders);
        Assert.Equal(1002, result.Folders.Count);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task List_AcceptsAbsentOrNullParentAndDoesNotInventPaths(bool explicitNull)
    {
        var page = JsonNode.Parse(Page())!;
        var folder = page["value"]![0]!.AsObject();
        folder.Remove("parentFolderId");
        if (explicitNull)
        {
            folder["parentFolderId"] = null;
        }
        folder["displayName"] = "Sales \u00e9\U0001F680";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(page.ToJsonString())));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));

        Assert.Equal(Folder with { ParentFolderId = null, DisplayName = "Sales \u00e9\U0001F680" }, Assert.Single(result.Folders));
        var output = JsonSerializer.Serialize(result, CoreJsonContext.Default.FolderListCommandResult);
        Assert.DoesNotContain("parentFolderId", output);
        Assert.DoesNotContain("path", output, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{private")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"value":null}""")]
    [InlineData("""{"value":{}}""")]
    [InlineData("""{"value":[null]}""")]
    [InlineData("""{"value":[{}]}""")]
    [InlineData("""{"value":[1]}""")]
    [InlineData("""{"value":[],"value":[]}""")]
    [InlineData("""{"value":[],"Value":[{}]}""")]
    // cspell:disable-next-line
    [InlineData("""{"value":[],"\u0076alue":[]}""")]
    [InlineData("""{"value":[],"extra":{"same":1,"same":2}}""")]
    [InlineData("""{"value":[],"extra":{"text":"\uD800"}}""")]
    [InlineData("""{"value":[],"extra":{"name\uDC00":"private"}}""")]
    public static async Task List_RejectsMalformedMissingDuplicateAndInvalidUnicodeMetadata(string json) =>
        await AssertInvalidPageAsync(json);

    [Theory]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("id", "\"private\"")]
    [InlineData("id", "null")]
    [InlineData("workspaceId", "\"" + FolderId + "\"")]
    [InlineData("workspaceId", "null")]
    [InlineData("displayName", "\" \"")]
    [InlineData("displayName", "null")]
    [InlineData("displayName", "42")]
    [InlineData("displayName", "\"\\uD800\"")]
    [InlineData("parentFolderId", "\"private\"")]
    [InlineData("parentFolderId", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("parentFolderId", "42")]
    public static async Task List_RejectsAnInvalidLaterRowWithoutReturningTheValidFirstRow(string name, string value)
    {
        var valid = JsonSerializer.Serialize(Folder, CoreJsonContext.Default.FabricFolder);
        var invalid = JsonNode.Parse(valid)!.AsObject();
        invalid.Remove(name);
        var invalidJson = invalid.ToJsonString();
        invalidJson = $"{invalidJson[..^1]},\"{name}\":{value}}}";
        await AssertInvalidPageAsync($"{{\"value\":[{valid},{invalidJson}]}}");
    }

    [Theory]
    [InlineData("id")]
    [InlineData("workspaceId")]
    [InlineData("displayName")]
    public static async Task List_RejectsMissingRequiredFields(string name)
    {
        var page = JsonNode.Parse(Page())!;
        page["value"]![0]!.AsObject().Remove(name);
        await AssertInvalidPageAsync(page.ToJsonString());
    }

    [Fact]
    public static async Task List_RejectsDuplicateRowPropertiesAndMalformedEscapedUnicode()
    {
        var row = JsonSerializer.Serialize(Folder, CoreJsonContext.Default.FabricFolder);
        foreach (var additional in new[] { "\"Id\":\"" + FolderId + "\"", "\"displayName\":\"other\"", "\"extra\":\"\\uD800\"" })
        {
            await AssertInvalidPageAsync($"{{\"value\":[{row[..^1]},{additional}}}]}}");
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("../folders")]
    [InlineData("https://example.invalid")]
    [InlineData(FolderId + "?query=private")]
    public static async Task List_ValidatesIdsBeforeAuthentication(string? value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);

        Assert.Equal("workspaceId", (await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListFoldersAsync(value!, cancellationToken: TestContext.Current.CancellationToken))).ParamName);
        if (value is not null)
        {
            Assert.Equal("rootFolderId", (await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ListFoldersAsync(WorkspaceId, value, cancellationToken: TestContext.Current.CancellationToken))).ParamName);
        }
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public static async Task List_RejectsInvalidTokensBeforeAuthentication()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        foreach (var token in new[] { "", " \u00a0", "private\0", "private\r\n", "private\t", "\uD800", "\uDC00" })
        {
            var error = await Assert.ThrowsAsync<ArgumentException>(() => ReadAsync(service, token));
            Assert.Equal("continuationToken", error.ParamName);
            Assert.DoesNotContain("private", error.Message);
        }
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task List_EnforcesExactUtf8TokenBudgetOnInputAndOutput(bool multibyte, int excess)
    {
        var token = new string(multibyte ? '\u00e9' : 'a', multibyte ? 4096 : 8192) + new string('x', excess);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Page(token))));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        if (excess == 0)
        {
            Assert.Equal(token, (await ReadAsync(service, token)).ContinuationToken);
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() => ReadAsync(service, token));
            Assert.Empty(credential.ReceivedCalls());
            Assert.Equal(0, handler.CallCount);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service));
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task List_EnforcesExactResponseByteBudgetWithoutTruncation(bool knownLength, int excess)
    {
        const int limit = 1024 * 1024;
        Assert.Equal(limit, FabricResponseBodyReader.DefaultMaxJsonBytes);
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes(Page()), limit + excess);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        if (knownLength)
        {
            response.Content.Headers.ContentLength = limit + excess;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());

        if (excess == 0)
        {
            Assert.Equal(Folder, Assert.Single((await ReadAsync(service)).Folders));
        }
        else
        {
            Assert.Contains("1 MiB", (await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service))).Message);
        }
        Assert.Equal(knownLength && excess > 0 ? 0 : limit + excess, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65)]
    public static async Task List_EnforcesThe64LevelDepthBudgetWithoutRetainingUnknownJson(int depth)
    {
        var json = "{\"value\":[],\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (depth == 64)
        {
            var result = await ReadAsync(service);
            Assert.Equal("{\"folders\":[]}", JsonSerializer.Serialize(result, CoreJsonContext.Default.FolderListCommandResult));
        }
        else
        {
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service));
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task List_RejectsInvalidUtf8EvenInOmittedFields()
    {
        var body = Encoding.UTF8.GetBytes("{\"value\":[],\"extra\":\"\u00e9\"}");
        body[Array.IndexOf(body, (byte)0xc3)] = 0xff;
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(body) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    [InlineData(HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public static async Task List_RetainsHttpStatusAndRetryGuidanceWithoutReadingErrorsOrFollowingUrls(HttpStatusCode status)
    {
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(status) { Content = content };
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("Retry-After", "45");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var error = await Assert.ThrowsAsync<FolderListRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));

        Assert.Equal(response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : status, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        Assert.True(content.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("45", 45L)]
    [InlineData("2147483647", 2147483647L)]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("-1", null)]
    [InlineData("1.5", null)]
    [InlineData("2147483648", null)]
    [InlineData("45, 60", null)]
    [InlineData("private", null)]
    public static async Task List_ReturnsOnlyValidatedRetrySeconds(string? header, long? expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Page(), retryAfter: header)));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(expected, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task List_PreservesRetryDateButRejectsMultipleRetryHeaders(bool multiple)
    {
        using var response = Response(Page());
        response.Headers.TryAddWithoutValidation("Retry-After", multiple ? ["45", "60"] : ["Tue, 01 Jan 2030 00:00:00 GMT"]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(multiple ? null : new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), result.RetryAfterUtc);
        Assert.Null(result.RetryAfterSeconds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("text/html")]
    [InlineData("application/octet-stream")]
    public static async Task List_RejectsUnsupportedMediaBeforeReading(string? mediaType)
    {
        using var content = new UnreadableHttpContent();
        content.Headers.ContentType = mediaType is null ? null : new(mediaType);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public static async Task List_DoesNotMisclassifyUnrelatedReaderFailuresAsMalformedJson()
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(Page()),
            (_, _) => throw new InvalidOperationException("private-reader-failure"));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.True(stream.Disposed);
    }

    [Fact]
    public static async Task List_AuthenticationFailureSendsNoHttp()
    {
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private")));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => ReadAsync(new FabricCoreService(client, credential)));
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("before")]
    [InlineData("authentication")]
    [InlineData("http")]
    [InlineData("headers")]
    [InlineData("body")]
    [InlineData("end-of-body")]
    public static async Task List_PropagatesCancellationAndDisposesAvailableResponses(string phase)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            if (phase == "authentication")
            {
                cancellation.Cancel();
            }
            return new AccessToken("folder-test-token", DateTimeOffset.MaxValue);
        });
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(Page()), (count, token) =>
        {
            if (phase == "body" || (phase == "end-of-body" && count == 0))
            {
                cancellation.Cancel();
                if (phase == "body")
                {
                    token.ThrowIfCancellationRequested();
                }
            }
            return ValueTask.CompletedTask;
        });
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            if (phase is "http" or "headers")
            {
                cancellation.Cancel();
            }
            return phase == "http" ? Task.FromCanceled<HttpResponseMessage>(token) : Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        if (phase == "before")
        {
            await cancellation.CancelAsync();
        }

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ReadAsync(new FabricCoreService(client, credential), cancellationToken: cancellation.Token));

        Assert.Equal(phase is "before" or "authentication" ? 0 : 1, handler.CallCount);
        if (phase is "headers" or "body" or "end-of-body")
        {
            Assert.True(stream.Disposed);
        }
        await credential.Received(phase == "before" ? 0 : 1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Fact]
    public static async Task List_IsolatesConcurrentIdentitiesWorkspacesFiltersAndPages()
    {
        var identity = new AsyncLocal<string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var requests = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            var workspace = request.RequestUri!.Segments[3].TrimEnd('/');
            Assert.Equal(workspace, request.Headers.Authorization?.Parameter);
            Assert.Equal($"?rootFolderId={workspace}&recursive=false&continuationToken={workspace}", request.RequestUri.Query);
            requests.Add(workspace);
            await Task.Yield();
            var folder = Folder with { WorkspaceId = Guid.Parse(workspace), ParentFolderId = Guid.Parse(workspace) };
            var json = JsonSerializer.Serialize(folder, CoreJsonContext.Default.FabricFolder);
            return Response($"{{\"value\":[{json}],\"continuationToken\":\"{workspace}\"}}");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            var workspace = Guid.NewGuid().ToString();
            identity.Value = workspace;
            var result = await service.ListFoldersAsync(workspace, workspace, false, workspace, TestContext.Current.CancellationToken);
            Assert.Equal(Guid.Parse(workspace), Assert.Single(result.Folders).WorkspaceId);
            Assert.Equal(Guid.Parse(workspace), result.Folders[0].ParentFolderId);
            Assert.Equal(workspace, result.ContinuationToken);
        }));

        Assert.Equal(8, handler.CallCount);
        Assert.Equal(8, requests.Distinct().Count());
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    private static async Task AssertInvalidPageAsync(string json, string? root = null, bool? recursive = null)
    {
        using var response = Response(json);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<JsonException>(() =>
            new FabricCoreService(client, Credential()).ListFoldersAsync(WorkspaceId, root, recursive, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("No partial page", error.Message);
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    private static Task<FolderListCommandResult> ReadAsync(
        FabricCoreService service, string? token = null, CancellationToken? cancellationToken = null) =>
        service.ListFoldersAsync(WorkspaceId, continuationToken: token, cancellationToken: cancellationToken ?? TestContext.Current.CancellationToken);
}
