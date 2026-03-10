// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Helpers;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Subscription;

public class AzureCliProfileHelperTests
{
    [Fact]
    public void ParseDefaultSubscriptionId_ValidProfile_ReturnsDefaultId()
    {
        var profileJson = """
        {
            "subscriptions": [
                {
                    "id": "sub-1111-1111",
                    "name": "Subscription One",
                    "state": "Enabled",
                    "tenantId": "tenant-1111",
                    "isDefault": false
                },
                {
                    "id": "sub-2222-2222",
                    "name": "Subscription Two",
                    "state": "Enabled",
                    "tenantId": "tenant-2222",
                    "isDefault": true
                },
                {
                    "id": "sub-3333-3333",
                    "name": "Subscription Three",
                    "state": "Enabled",
                    "tenantId": "tenant-3333",
                    "isDefault": false
                }
            ]
        }
        """;

        var result = AzureCliProfileHelper.ParseDefaultSubscriptionId(profileJson);

        Assert.Equal("sub-2222-2222", result);
    }

    [Fact]
    public void ParseDefaultSubscriptionId_NoDefaultInProfile_ReturnsNull()
    {
        var profileJson = """
        {
            "subscriptions": [
                {
                    "id": "sub-1111-1111",
                    "name": "Subscription One",
                    "state": "Enabled",
                    "tenantId": "tenant-1111",
                    "isDefault": false
                }
            ]
        }
        """;

        var result = AzureCliProfileHelper.ParseDefaultSubscriptionId(profileJson);

        Assert.Null(result);
    }

    [Fact]
    public void ParseDefaultSubscriptionId_EmptySubscriptions_ReturnsNull()
    {
        var profileJson = """
        {
            "subscriptions": []
        }
        """;

        var result = AzureCliProfileHelper.ParseDefaultSubscriptionId(profileJson);

        Assert.Null(result);
    }

    [Fact]
    public void ParseDefaultSubscriptionId_NoSubscriptionsProperty_ReturnsNull()
    {
        var profileJson = """
        {
            "installationId": "some-id"
        }
        """;

        var result = AzureCliProfileHelper.ParseDefaultSubscriptionId(profileJson);

        Assert.Null(result);
    }

    [Fact]
    public void ParseDefaultSubscriptionId_MissingIdOnDefault_ReturnsNull()
    {
        var profileJson = """
        {
            "subscriptions": [
                {
                    "name": "Subscription One",
                    "isDefault": true
                }
            ]
        }
        """;

        var result = AzureCliProfileHelper.ParseDefaultSubscriptionId(profileJson);

        Assert.Null(result);
    }

    [Fact]
    public void GetAzureProfilePath_ReturnsExpectedPath()
    {
        var result = AzureCliProfileHelper.GetAzureProfilePath();

        Assert.Contains(".azure", result);
        Assert.EndsWith("azureProfile.json", result);
    }
}
