// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Helpers;

public class LiveTestSettingsTests
{
    [Fact]
    public void JsonDeserialization_WithInvalidTestMode_ThrowsJsonException()
    {
        // Arrange - Create JSON with an invalid TestMode value
        var json = """{"TestMode": "InvalidMode", "TenantId": "test-tenant"}""";

        // Act & Assert - Verify JsonException is thrown
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<LiveTestSettingsProxy>(json, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            }));

        // Verify the error message mentions TestMode
        Assert.Contains("TestMode", exception.Message);
    }

    [Theory]
    [InlineData("Live", TestMode.Live)]
    [InlineData("Record", TestMode.Record)]
    [InlineData("Playback", TestMode.Playback)]
    [InlineData("live", TestMode.Live)]  // Test case-insensitive
    [InlineData("record", TestMode.Record)]
    [InlineData("playback", TestMode.Playback)]
    public void JsonDeserialization_WithValidTestMode_LoadsSuccessfully(string testModeValue, TestMode expectedMode)
    {
        // Arrange - Create JSON with valid TestMode value
        var json = $$$"""{"TestMode": "{{{testModeValue}}}", "TenantId": "test-tenant", "SubscriptionId": "test-subscription"}""";

        // Act - Deserialize the JSON
        var settings = JsonSerializer.Deserialize<LiveTestSettingsProxy>(json, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });

        // Assert - Verify the settings were loaded correctly
        Assert.NotNull(settings);
        Assert.Equal(expectedMode, settings.TestMode);
        Assert.Equal("test-tenant", settings.TenantId);
        Assert.Equal("test-subscription", settings.SubscriptionId);
    }

    /// <summary>
    /// Proxy class for testing LiveTestSettings JSON deserialization without file I/O
    /// </summary>
    private class LiveTestSettingsProxy
    {
        public TestMode TestMode { get; set; } = TestMode.Live;
        public string TenantId { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
    }
}
