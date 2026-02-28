// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Helpers;

public class LiveTestSettingsTests
{
    [Fact]
    public void TryLoadTestSettings_WithInvalidTestMode_ReturnsFalseAndWritesErrorToConsole()
    {
        // Arrange - Create .testsettings.json in AppContext.BaseDirectory with invalid TestMode
        var testSettingsPath = Path.Combine(AppContext.BaseDirectory, LiveTestSettings.TestSettingsFileName);
        var backupPath = testSettingsPath + ".backup";

        // Backup existing file if present
        if (File.Exists(testSettingsPath))
        {
            File.Move(testSettingsPath, backupPath);
        }

        try
        {
            var invalidSettings = new
            {
                TestMode = "InvalidMode",
                TenantId = "test-tenant",
                SubscriptionId = "test-subscription"
            };
            File.WriteAllText(testSettingsPath, JsonSerializer.Serialize(invalidSettings));

            // Redirect Console.Error to capture the error message
            var originalError = Console.Error;
            using var errorWriter = new StringWriter();
            Console.SetError(errorWriter);

            try
            {
                // Act
                var result = LiveTestSettings.TryLoadTestSettings(out var settings);

                // Assert
                Assert.False(result);
                Assert.Null(settings);

                // Verify the error message was written to Console.Error
                var errorOutput = errorWriter.ToString();
                Assert.Contains("Invalid TestMode value", errorOutput);
                Assert.Contains(".testsettings.json", errorOutput);
                Assert.Contains("Live", errorOutput);
                Assert.Contains("Record", errorOutput);
                Assert.Contains("Playback", errorOutput);
            }
            finally
            {
                Console.SetError(originalError);
            }
        }
        finally
        {
            // Cleanup - remove test file and restore backup if it existed
            if (File.Exists(testSettingsPath))
            {
                File.Delete(testSettingsPath);
            }
            if (File.Exists(backupPath))
            {
                File.Move(backupPath, testSettingsPath);
            }
        }
    }

    [Theory]
    [InlineData("Live")]
    [InlineData("Record")]
    [InlineData("Playback")]
    [InlineData("live")]  // Test case-insensitive
    [InlineData("record")]
    [InlineData("playback")]
    public void TryLoadTestSettings_WithValidTestMode_LoadsSuccessfully(string testModeValue)
    {
        // Arrange - Create .testsettings.json in AppContext.BaseDirectory with valid TestMode
        var testSettingsPath = Path.Combine(AppContext.BaseDirectory, LiveTestSettings.TestSettingsFileName);
        var backupPath = testSettingsPath + ".backup";

        // Backup existing file if present
        if (File.Exists(testSettingsPath))
        {
            File.Move(testSettingsPath, backupPath);
        }

        try
        {
            var validSettings = new
            {
                TestMode = testModeValue,
                TenantId = "test-tenant",
                SubscriptionId = "test-subscription"
            };
            File.WriteAllText(testSettingsPath, JsonSerializer.Serialize(validSettings));

            // Act
            var result = LiveTestSettings.TryLoadTestSettings(out var settings);

            // Assert
            Assert.True(result);
            Assert.NotNull(settings);
            Assert.Equal("test-tenant", settings.TenantId);
            Assert.Equal("test-subscription", settings.SubscriptionId);
        }
        finally
        {
            // Cleanup - remove test file and restore backup if it existed
            if (File.Exists(testSettingsPath))
            {
                File.Delete(testSettingsPath);
            }
            if (File.Exists(backupPath))
            {
                File.Move(backupPath, testSettingsPath);
            }
        }
    }

    [Fact]
    public void TryLoadTestSettings_WhenFileNotFound_ReturnsFalse()
    {
        // Arrange - Ensure no .testsettings.json exists
        var testSettingsPath = Path.Combine(AppContext.BaseDirectory, LiveTestSettings.TestSettingsFileName);
        var backupPath = testSettingsPath + ".backup";

        // Backup and remove existing file if present
        if (File.Exists(testSettingsPath))
        {
            File.Move(testSettingsPath, backupPath);
        }

        try
        {
            // Act
            var result = LiveTestSettings.TryLoadTestSettings(out var settings);

            // Assert
            Assert.False(result);
            Assert.Null(settings);
        }
        finally
        {
            // Restore backup if it existed
            if (File.Exists(backupPath))
            {
                File.Move(backupPath, testSettingsPath);
            }
        }
    }
}
