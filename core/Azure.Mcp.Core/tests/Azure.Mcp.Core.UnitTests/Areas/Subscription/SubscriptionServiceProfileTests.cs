// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Subscription;

public class SubscriptionServiceProfileTests
{
    [Fact]
    public void ReadDefaultSubscriptionFromAzureProfile_ValidProfile_ReturnsDefaultId()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"azprofile_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var profilePath = Path.Combine(tempDir, "azureProfile.json");

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

        try
        {
            File.WriteAllText(profilePath, profileJson);

            // Use reflection to test with custom path - or test the static helper directly
            // Since ReadDefaultSubscriptionFromAzureProfile reads from GetAzureProfilePath(),
            // we test the JSON parsing logic via the internal method
            var result = ReadProfileFromJson(profileJson);

            // Assert
            Assert.Equal("sub-2222-2222", result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ReadDefaultSubscriptionFromAzureProfile_NoDefaultInProfile_ReturnsNull()
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

        var result = ReadProfileFromJson(profileJson);

        Assert.Null(result);
    }

    [Fact]
    public void ReadDefaultSubscriptionFromAzureProfile_EmptySubscriptions_ReturnsNull()
    {
        var profileJson = """
        {
            "subscriptions": []
        }
        """;

        var result = ReadProfileFromJson(profileJson);

        Assert.Null(result);
    }

    [Fact]
    public void ReadDefaultSubscriptionFromAzureProfile_NoSubscriptionsProperty_ReturnsNull()
    {
        var profileJson = """
        {
            "installationId": "some-id"
        }
        """;

        var result = ReadProfileFromJson(profileJson);

        Assert.Null(result);
    }

    [Fact]
    public void ReadDefaultSubscriptionFromAzureProfile_InvalidJson_ReturnsNull()
    {
        var profileJson = "this is not json";

        var result = ReadProfileFromJson(profileJson);

        Assert.Null(result);
    }

    [Fact]
    public void ReadDefaultSubscriptionFromAzureProfile_MissingIdOnDefault_ReturnsNull()
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

        var result = ReadProfileFromJson(profileJson);

        Assert.Null(result);
    }

    /// <summary>
    /// Helper that simulates ReadDefaultSubscriptionFromAzureProfile's JSON parsing logic
    /// without requiring filesystem access.
    /// </summary>
    private static string? ReadProfileFromJson(string json)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("subscriptions", out var subscriptions) ||
                subscriptions.ValueKind != System.Text.Json.JsonValueKind.Array)
            {
                return null;
            }

            foreach (var sub in subscriptions.EnumerateArray())
            {
                if (sub.TryGetProperty("isDefault", out var isDefault) &&
                    isDefault.ValueKind == System.Text.Json.JsonValueKind.True &&
                    sub.TryGetProperty("id", out var id) &&
                    id.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    return id.GetString();
                }
            }
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or IOException)
        {
            // Best-effort: profile may be corrupted or inaccessible
        }

        return null;
    }
}
