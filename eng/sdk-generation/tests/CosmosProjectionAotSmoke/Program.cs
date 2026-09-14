// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;

const string subscriptionId = "00000000-0000-0000-0000-000000000000";
const string resourceGroupName = "test-rg";
const string accountName = "test-account";

var transport = new CosmosMockTransport(subscriptionId, resourceGroupName, accountName);
var options = new ArmClientOptions { Transport = transport };
var client = new ArmClient(new StaticTokenCredential(), subscriptionId, options);
var subscription = client.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));

CosmosDBAccountResource? account = null;
await foreach (var candidate in subscription.GetCosmosDBAccountsAsync())
{
    account = candidate;
    break;
}

if (account is null || account.Data.Name != accountName)
{
    throw new InvalidOperationException("The projected SDK did not deserialize the expected account.");
}

var keys = await account.GetKeysAsync();
if (keys.Value.PrimaryMasterKey != "primary-key")
{
    throw new InvalidOperationException("The projected SDK did not deserialize the expected account key.");
}

if (!transport.SawAccountList || !transport.SawKeyList)
{
    throw new InvalidOperationException("The projected management operations were not executed.");
}

Console.WriteLine("Cosmos projected SDK Native AOT smoke test passed.");

internal sealed class StaticTokenCredential : TokenCredential
{
    private static readonly AccessToken s_token = new("token", DateTimeOffset.MaxValue);

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) => s_token;

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) => ValueTask.FromResult(s_token);
}

internal sealed class CosmosMockTransport(string subscriptionId, string resourceGroupName, string accountName) : HttpPipelineTransport
{
    private readonly HttpClientTransport _requestFactory = new();

    public bool SawAccountList { get; private set; }
    public bool SawKeyList { get; private set; }

    public override Request CreateRequest() => _requestFactory.CreateRequest();

    public override void Process(HttpMessage message) => message.Response = CreateResponse(message.Request);

    public override ValueTask ProcessAsync(HttpMessage message)
    {
        message.Response = CreateResponse(message.Request);
        return ValueTask.CompletedTask;
    }

    private MockResponse CreateResponse(Request request)
    {
        var path = request.Uri.Path;
        if (path.EndsWith("/providers/Microsoft.DocumentDB/databaseAccounts", StringComparison.Ordinal))
        {
            ValidateRequest(request, RequestMethod.Get);
            SawAccountList = true;
            return MockResponse.Json($$"""
                {
                  "value": [
                    {
                      "id": "/subscriptions/{{subscriptionId}}/resourceGroups/{{resourceGroupName}}/providers/Microsoft.DocumentDB/databaseAccounts/{{accountName}}",
                      "name": "{{accountName}}",
                      "type": "Microsoft.DocumentDB/databaseAccounts",
                      "location": "westus",
                      "properties": {
                        "provisioningState": "Succeeded",
                        "databaseAccountOfferType": "Standard",
                        "locations": []
                      }
                    }
                  ]
                }
                """);
        }

        if (path.EndsWith($"/databaseAccounts/{accountName}/listKeys", StringComparison.Ordinal))
        {
            ValidateRequest(request, RequestMethod.Post);
            SawKeyList = true;
            return MockResponse.Json("""
                {
                  "primaryMasterKey": "primary-key",
                  "secondaryMasterKey": "secondary-key",
                  "primaryReadonlyMasterKey": "readonly-primary",
                  "secondaryReadonlyMasterKey": "readonly-secondary"
                }
                """);
        }

        return MockResponse.Json("{}", 404);
    }

    private static void ValidateRequest(Request request, RequestMethod expectedMethod)
    {
        if (request.Method != expectedMethod || !request.Uri.Query.Contains("api-version=2026-03-15", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Unexpected request: {request.Method} {request.Uri}");
        }
    }
}

internal sealed class MockResponse(int status, Stream content) : Response
{
    private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Content-Type"] = "application/json"
    };

    public static MockResponse Json(string json, int status = 200) => new(status, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));

    public override int Status => status;
    public override string ReasonPhrase => status == 200 ? "OK" : "Not Found";
    public override Stream? ContentStream { get => content; set => throw new NotSupportedException(); }
    public override string ClientRequestId { get; set; } = Guid.NewGuid().ToString();

    public override void Dispose() => content.Dispose();

    protected override bool TryGetHeader(string name, out string value) => _headers.TryGetValue(name, out value!);

    protected override bool TryGetHeaderValues(string name, out IEnumerable<string> values)
    {
        if (_headers.TryGetValue(name, out var value))
        {
            values = [value];
            return true;
        }
        values = null!;
        return false;
    }

    protected override bool ContainsHeader(string name) => _headers.ContainsKey(name);

    protected override IEnumerable<HttpHeader> EnumerateHeaders() => _headers.Select(header => new HttpHeader(header.Key, header.Value));
}
