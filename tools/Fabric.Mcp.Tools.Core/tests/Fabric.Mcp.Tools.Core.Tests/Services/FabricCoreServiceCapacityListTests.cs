// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public class FabricCoreServiceCapacityListTests()
{
    private const string CapacityUrl = "https://api.fabric.microsoft.com/v1/capacities";

    [Fact]
    public async Task ListCapacitiesAsync_ReturnsOneTypedPageUsingInjectedCredential()
    {
        using var handler = new FabricCoreHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(CapacityUrl, request.RequestUri?.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("capacity-test-token", request.Headers.Authorization?.Parameter);
            Assert.Contains("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.Null(request.Content);
            Assert.True(cancellationToken.CanBeCanceled);
            return Task.FromResult(CapacityListTestData.CreateResponse(CapacityListTestData.FullPage));
        });
        using var client = new HttpClient(handler);
        var credential = CapacityListTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        var page = await service.ListCapacitiesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var capacity = Assert.Single(page.Value);
        Assert.Equal(Guid.Parse(CapacityListTestData.CapacityId), capacity.Id);
        Assert.Equal("Finance Capacity", capacity.DisplayName);
        Assert.Equal("FutureSku", capacity.Sku);
        Assert.Equal("Future Region", capacity.Region);
        Assert.Equal("FutureState", capacity.State);
        Assert.Equal(CapacityListTestData.ContinuationToken, page.ContinuationToken);
        Assert.Equal(CapacityListTestData.ContinuationUri, page.ContinuationUri);
        Assert.Equal(1, handler.CallCount);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("ABCsMTAwMDAwLDA%3D", "ABCsMTAwMDAwLDA%3D")]
    [InlineData(CapacityListTestData.ContinuationToken, CapacityListTestData.ContinuationToken)]
    [InlineData("ABCsMTAwMDAwLDA=", "ABCsMTAwMDAwLDA%3D")]
    [InlineData("a+/=", "a%2B%2F%3D")]
    [InlineData("a%2b%2f%3d", "a%2b%2f%3d")]
    [InlineData("%41%7e%2D%5f%2E", "A~-_.")]
    [InlineData("a%252B%2526", "a%252B%2526")]
    [InlineData("%G1%2%", "%25G1%252%25")]
    [InlineData("x%2f+raw%broken", "x%2f%2Braw%25broken")]
    [InlineData("x&continuationToken=evil#fragment?", "x%26continuationToken%3Devil%23fragment%3F")]
    // cspell:disable-next-line
    [InlineData("https://untrusted.invalid/next?a=b", "https%3A%2F%2Funtrusted.invalid%2Fnext%3Fa%3Db")]
    [InlineData(";?=&/#\\", "%3B%3F%3D%26%2F%23%5C")]
    [InlineData("x\r\nHeader: value", "x%0D%0AHeader%3A%20value")]
    [InlineData("\u00e9 \U0001F600", "%C3%A9%20%F0%9F%98%80")]
    public async Task ListCapacitiesAsync_PreservesQueryMeaningWithoutDoubleEncoding(string token, string encoded)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.NotNull(request.RequestUri);
            Assert.Equal($"{CapacityUrl}?continuationToken={encoded}", request.RequestUri.AbsoluteUri);
            Assert.Equal("api.fabric.microsoft.com", request.RequestUri.Host);
            Assert.Equal("/v1/capacities", request.RequestUri.AbsolutePath);
            Assert.Equal($"?continuationToken={encoded}", request.RequestUri.Query);
            Assert.Empty(request.RequestUri.Fragment);
            return Task.FromResult(CapacityListTestData.CreateResponse());
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        var page = await service.ListCapacitiesAsync(token, TestContext.Current.CancellationToken);

        Assert.Empty(page.Value);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("raw+/=%G1", null)]
    [InlineData(null, "https://untrusted.invalid/next")]
    [InlineData("%41%7e%2D%5f%2E", null)]
    [InlineData(CapacityListTestData.ContinuationToken, CapacityListTestData.ContinuationUri)]
    [InlineData("ABCsMTAwMDAwLDA%3D", "https://api.fabric.microsoft.com/v1/workspaces/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/items?continuationToken=ABCsMTAwMDAwLDA%3D")]
    public async Task ListCapacitiesAsync_ReturnsEmptyPageAndUnchangedContinuationWithoutFollowingUri(string? token, string? uri)
    {
        var payload = JsonSerializer.Serialize(
            new CapacityListResponse { Value = [], ContinuationToken = token, ContinuationUri = uri },
            CoreJsonContext.Default.CapacityListResponse);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(CapacityListTestData.CreateResponse(payload)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        var page = await service.ListCapacitiesAsync("previous-page", TestContext.Current.CancellationToken);

        Assert.Empty(page.Value);
        Assert.Equal(token, page.ContinuationToken);
        Assert.Equal(uri, page.ContinuationUri);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ListCapacitiesAsync_ReturnsCompleteFinalPageWithoutInventingContinuation()
    {
        const string payload = """
            {
              "value": [
                {
                  "id": "96f3f0ff-4fe2-4712-b61b-05a456ba9357",
                  "displayName": "F4 Capacity",
                  "sku": "F4",
                  "region": "West Central US",
                  "state": "Active"
                },
                {
                  "id": "0b9a4952-b5e7-4a55-8739-3e7251a2fd43",
                  "displayName": "F8 Capacity",
                  "sku": "F8",
                  "region": "West Central US",
                  "state": "Inactive"
                }
              ]
            }
            """;
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(CapacityListTestData.CreateResponse(payload)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        var page = await service.ListCapacitiesAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["F4 Capacity", "F8 Capacity"], page.Value.Select(static capacity => capacity.DisplayName));
        Assert.Equal(["Active", "Inactive"], page.Value.Select(static capacity => capacity.State));
        Assert.Null(page.ContinuationToken);
        Assert.Null(page.ContinuationUri);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\r\n\t")]
    public async Task ListCapacitiesAsync_RejectsBlankTokenBeforeAuthentication(string token)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(CapacityListTestData.CreateResponse()));
        using var client = new HttpClient(handler);
        var credential = CapacityListTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ListCapacitiesAsync(token, TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("""{"value":null}""")]
    [InlineData("""{"value":{}}""")]
    [InlineData("""{"value":[null]}""")]
    [InlineData("""{"value":[],"continuationToken":1}""")]
    [InlineData("""{"value":[],"continuationUri":[]}""")]
    public async Task ListCapacitiesAsync_RejectsInvalidPages(string payload)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(CapacityListTestData.CreateResponse(payload)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => service.ListCapacitiesAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("id", false)]
    [InlineData("id", true)]
    [InlineData("displayName", false)]
    [InlineData("displayName", true)]
    [InlineData("sku", false)]
    [InlineData("sku", true)]
    [InlineData("region", false)]
    [InlineData("region", true)]
    [InlineData("state", false)]
    [InlineData("state", true)]
    public async Task ListCapacitiesAsync_RejectsMissingOrNullRequiredMetadata(string field, bool explicitNull)
    {
        var capacity = JsonNode.Parse(CapacityListTestData.CapacityJson)!.AsObject();
        if (explicitNull)
        {
            capacity[field] = null;
        }
        else
        {
            capacity.Remove(field);
        }
        var payload = $$"""{"value":[{{capacity.ToJsonString()}}]}""";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(CapacityListTestData.CreateResponse(payload)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => service.ListCapacitiesAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("id", "not-a-uuid")]
    [InlineData("displayName", "")]
    [InlineData("sku", " ")]
    [InlineData("region", "\t")]
    [InlineData("state", "\r\n")]
    public async Task ListCapacitiesAsync_RejectsInvalidRequiredMetadata(string field, string value)
    {
        var capacity = JsonNode.Parse(CapacityListTestData.CapacityJson)!.AsObject();
        capacity[field] = value;
        var payload = $$"""{"value":[{{capacity.ToJsonString()}}]}""";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(CapacityListTestData.CreateResponse(payload)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => service.ListCapacitiesAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.Redirect)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    public async Task ListCapacitiesAsync_RejectsNon200StatusWithoutLeakingBodyOrRetrying(HttpStatusCode status)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(CapacityListTestData.CreateResponse("private-backend-detail", status)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.ListCapacitiesAsync(cancellationToken: TestContext.Current.CancellationToken));

        var expectedStatus = (int)status is >= 200 and < 300 ? HttpStatusCode.BadGateway : status;
        Assert.Equal(expectedStatus, exception.StatusCode);
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("0", "Wait at least 0 seconds before retrying")]
    [InlineData("120", "Wait at least 120 seconds before retrying")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(null, null)]
    [InlineData("-1", null)]
    [InlineData("private-header-detail", null)]
    [InlineData("99999999999999999999999999", null)]
    [InlineData("1, 2", null)]
    public async Task ListCapacitiesAsync_OnlyExposesValidatedRetryAfter(string? header, string? expectedGuidance)
    {
        using var response = CapacityListTestData.CreateResponse("private-backend-detail", HttpStatusCode.TooManyRequests);
        if (header is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", header));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            service.ListCapacitiesAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.DoesNotContain("private-", exception.Message);
        if (expectedGuidance is not null)
        {
            Assert.Contains(expectedGuidance, exception.Message);
        }
        else
        {
            Assert.Equal("Unable to list Fabric capacities.", exception.Message);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ListCapacitiesAsync_IgnoresMultipleRetryAfterValues()
    {
        using var response = CapacityListTestData.CreateResponse("private-backend-detail", HttpStatusCode.TooManyRequests);
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", new[] { "120", "private-header-detail" }));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.ListCapacitiesAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Equal("Unable to list Fabric capacities.", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ListCapacitiesAsync_PreCanceledCallDoesNotAuthenticateOrSend()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(CapacityListTestData.CreateResponse()));
        using var client = new HttpClient(handler);
        var credential = CapacityListTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ListCapacitiesAsync(cancellationToken: cancellation.Token));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task ListCapacitiesAsync_CancelsDuringCredentialAcquisition()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = CapacityListTestData.CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                cancellation.Cancel();
                return ValueTask.FromCanceled<AccessToken>(call.ArgAt<CancellationToken>(1));
            });
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(CapacityListTestData.CreateResponse()));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ListCapacitiesAsync(cancellationToken: cancellation.Token));

        Assert.Equal(0, handler.CallCount);
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Fact]
    public async Task ListCapacitiesAsync_CancelsDuringHttpSend()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, cancellationToken) =>
        {
            cancellation.Cancel();
            return Task.FromCanceled<HttpResponseMessage>(cancellationToken);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ListCapacitiesAsync(cancellationToken: cancellation.Token));

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ListCapacitiesAsync_CancelsDuringDeserializationAndDisposesStream()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new CapacityListCancellationStream(cancellation);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ListCapacitiesAsync(cancellationToken: cancellation.Token));

        Assert.Equal(cancellation.Token, stream.ReadCancellationToken);
        Assert.True(stream.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, CapacityListTestData.EmptyPage)]
    [InlineData(HttpStatusCode.OK, "not-json")]
    [InlineData(HttpStatusCode.Forbidden, "private-backend-detail")]
    public async Task ListCapacitiesAsync_DisposesRequestResponseAndContent(HttpStatusCode status, string payload)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
        HttpRequestMessage? capturedRequest = null;
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            capturedRequest = request;
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        if (status != HttpStatusCode.OK)
        {
            await Assert.ThrowsAsync<HttpRequestException>(() => service.ListCapacitiesAsync(cancellationToken: TestContext.Current.CancellationToken));
        }
        else if (payload == "not-json")
        {
            await Assert.ThrowsAsync<JsonException>(() => service.ListCapacitiesAsync(cancellationToken: TestContext.Current.CancellationToken));
        }
        else
        {
            await service.ListCapacitiesAsync(cancellationToken: TestContext.Current.CancellationToken);
        }

        Assert.False(stream.CanRead);
        Assert.NotNull(capturedRequest);
        using var replacementContent = new StringContent("replacement");
        Assert.Throws<ObjectDisposedException>(() => capturedRequest.Content = replacementContent);
        Assert.Throws<ObjectDisposedException>(() => response.Content = replacementContent);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ListCapacitiesAsync_KeepsConcurrentRequestsAndCredentialsIsolated()
    {
        var requests = new ConcurrentDictionary<string, string>();
        var bothStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.NotNull(request.RequestUri);
            Assert.NotNull(request.Headers.Authorization?.Parameter);
            requests[request.RequestUri.Query] = request.Headers.Authorization.Parameter;
            if (requests.Count == 2)
            {
                bothStarted.TrySetResult();
            }
            await bothStarted.Task.WaitAsync(cancellationToken);
            return CapacityListTestData.CreateResponse();
        });
        using var client = new HttpClient(handler);
        var credential = CapacityListTestData.CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("identity-1", DateTimeOffset.MaxValue), new AccessToken("identity-2", DateTimeOffset.MaxValue));
        var service = new FabricCoreService(client, credential);

        await Task.WhenAll(
            service.ListCapacitiesAsync("first", TestContext.Current.CancellationToken),
            service.ListCapacitiesAsync("second", TestContext.Current.CancellationToken));

        Assert.Equal("identity-1", requests["?continuationToken=first"]);
        Assert.Equal("identity-2", requests["?continuationToken=second"]);
        Assert.Equal(2, handler.CallCount);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
        await credential.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistingPostOperations_KeepRequestAndResultBehavior(bool createItem)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("capacity-test-token", request.Headers.Authorization?.Parameter);
            Assert.NotNull(request.Content);
            Assert.Equal("application/json", request.Content.Headers.ContentType?.MediaType);
            using var document = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
            if (createItem)
            {
                Assert.Equal("https://api.fabric.microsoft.com/v1/workspaces/workspace/items", request.RequestUri?.AbsoluteUri);
                Assert.Equal("New item", document.RootElement.GetProperty("displayName").GetString());
                Assert.Equal("Lakehouse", document.RootElement.GetProperty("type").GetString());
                return CapacityListTestData.CreateResponse("""{"id":"created-item","displayName":"New item","type":"Lakehouse"}""");
            }
            Assert.Equal("https://api.fabric.microsoft.com/v1/catalog/search", request.RequestUri?.AbsoluteUri);
            Assert.Equal("Finance", document.RootElement.GetProperty("search").GetString());
            Assert.Equal("raw+token%3D", document.RootElement.GetProperty("continuationToken").GetString());
            return CapacityListTestData.CreateResponse("""{"value":[{"id":"catalog-item"}],"continuationToken":"next-page"}""");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        if (createItem)
        {
            var item = await service.CreateItemAsync(
                "workspace", new CreateItemRequest { DisplayName = "New item", Type = "Lakehouse" }, TestContext.Current.CancellationToken);
            Assert.Equal("created-item", item.Id);
        }
        else
        {
            var page = await service.SearchCatalogAsync(
                new CatalogSearchRequest { Search = "Finance", ContinuationToken = "raw+token%3D" }, TestContext.Current.CancellationToken);
            Assert.Equal("catalog-item", Assert.Single(page.Value).Id);
            Assert.Equal("next-page", page.ContinuationToken);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistingPostOperations_KeepExistingFailureBehavior(bool createItem)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(CapacityListTestData.CreateResponse("existing-error", HttpStatusCode.BadRequest)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityListTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            if (createItem)
            {
                await service.CreateItemAsync("workspace", new CreateItemRequest(), TestContext.Current.CancellationToken);
            }
            else
            {
                await service.SearchCatalogAsync(new CatalogSearchRequest(), TestContext.Current.CancellationToken);
            }
        });

        Assert.Contains("status 400", exception.Message);
        Assert.Contains("existing-error", exception.Message);
        Assert.Null(exception.StatusCode);
        Assert.Equal(1, handler.CallCount);
    }
}
