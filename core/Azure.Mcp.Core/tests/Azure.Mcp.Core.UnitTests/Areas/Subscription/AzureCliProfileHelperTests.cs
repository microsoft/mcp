// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Helpers;
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
    public void GetAzureProfilePath_WhenUserProfileAvailable_ReturnsExpectedPath()
    {
        var result = AzureCliProfileHelper.GetAzureProfilePath();

        // In containerized/CI environments, user profile may not be available
        if (string.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)))
        {
            Assert.Null(result);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Contains(".azure", result);
            Assert.EndsWith("azureProfile.json", result);
        }
    }

    [Fact]
    public void GetAzureProfilePath_ReturnsNullOrValidPath()
    {
        var result = AzureCliProfileHelper.GetAzureProfilePath();

        // The method must either return null (empty user profile) or a valid absolute path
        if (result != null)
        {
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.Contains(".azure", result);
            Assert.EndsWith("azureProfile.json", result);
            Assert.True(Path.IsPathRooted(result), "Path should be absolute, not relative");
        }
    }
}
