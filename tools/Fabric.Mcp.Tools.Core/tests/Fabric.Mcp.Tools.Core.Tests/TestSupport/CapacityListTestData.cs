// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class CapacityListTestData
{
    public const string CapacityId = "96f3f0ff-4fe2-4712-b61b-05a456ba9357";
    // cspell:disable-next-line
    public const string ContinuationToken = "LDEsMTAwMDAwLDA%3D";
    public const string ContinuationUri = "https://api.fabric.microsoft.com/v1/capacities?continuationToken=LDEsMTAwMDAwLDA%3D";
    public const string EmptyPage = """{"value":[]}""";

    public const string CapacityJson = """
        {
          "id": "96f3f0ff-4fe2-4712-b61b-05a456ba9357",
          "displayName": "Finance Capacity",
          "sku": "FutureSku",
          "region": "Future Region",
          "state": "FutureState",
          "excludedWorkloadDetails": "private-capacity-detail"
        }
        """;

    public static string FullPage => $$"""
        {
          "value": [{{CapacityJson}}],
          "continuationToken": "{{ContinuationToken}}",
          "continuationUri": "{{ContinuationUri}}"
        }
        """;

    public static CapacityListResponse CreatePage() => new()
    {
        Value =
        [
            new()
            {
                Id = Guid.Parse(CapacityId),
                DisplayName = "Finance Capacity",
                Sku = "FutureSku",
                Region = "Future Region",
                State = "FutureState"
            }
        ],
        ContinuationToken = ContinuationToken,
        ContinuationUri = ContinuationUri
    };

    public static HttpResponseMessage CreateResponse(string payload = EmptyPage, HttpStatusCode status = HttpStatusCode.OK) => new(status)
    {
        Content = new StringContent(payload, Encoding.UTF8, "application/json")
    };

    public static TokenCredential CreateCredential(string token = "capacity-test-token")
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(token, DateTimeOffset.MaxValue));
        return credential;
    }
}
