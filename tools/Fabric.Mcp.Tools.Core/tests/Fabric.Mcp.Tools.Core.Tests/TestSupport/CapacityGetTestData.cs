// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class CapacityGetTestData
{
    public const string CapacityId = "96f3f0ff-4fe2-4712-b61b-05a456ba9357";
    public const string FullJson = """
        {
          "id": "96f3f0ff-4fe2-4712-b61b-05a456ba9357",
          "displayName": "F4 Capacity",
          "sku": "F4",
          "region": "West Central US",
          "state": "Active",
          "unknownProperty": "excluded-backend-property"
        }
        """;

    public static FabricCapacityMetadata CreateCapacity() => new()
    {
        Id = Guid.Parse(CapacityId),
        DisplayName = "F4 Capacity",
        Sku = "F4",
        Region = "West Central US",
        State = "Active"
    };

    public static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.MaxValue));
        return credential;
    }
}
