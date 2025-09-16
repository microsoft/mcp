// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Speech.LiveTests;

[Trait("Toolset", "Speech")]
[Trait("Category", "Live")]
public class SpeechCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_handle_missing_audio_file_gracefully()
    {
        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", "non-existent-test-audio.wav" }, // Intentionally non-existent for testing
                { "language", "en-US" },
                { "format", "simple" }
            });

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_recognize_speech_from_real_audio_file()
    {
        // This test validates complete end-to-end speech recognition with the test-audio.wav file from TestResources
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "test-audio.wav");

        // STRICT REQUIREMENT: The test audio file MUST exist in TestResources
        Assert.True(File.Exists(testAudioFile),
            $"Test audio file not found at: {testAudioFile}. Please ensure test-audio.wav exists in TestResources folder.");

        var fileInfo = new FileInfo(testAudioFile);
        Output.WriteLine($"Using test audio file: {testAudioFile}");
        Output.WriteLine($"Test audio file size: {fileInfo.Length} bytes");
        Assert.True(fileInfo.Length > 0, "Test audio file must not be empty");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        // Test with the real audio file - expect successful speech recognition
        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "simple" }
            });

        // STRICT REQUIREMENT: Speech recognition must return a result
        Assert.NotNull(result);

        var resultText = result.ToString();
        Output.WriteLine($"Speech recognition result: {resultText}");

        // Validate the result structure
        Assert.NotNull(resultText);
        Assert.NotEmpty(resultText);

        // Parse the JSON result to check the recognition outcome
        try
        {
            var jsonResult = JsonDocument.Parse(resultText);
            var resultObject = jsonResult.RootElement;

            if (resultObject.TryGetProperty("result", out var resultProperty))
            {
                var reason = resultProperty.TryGetProperty("reason", out var reasonProperty)
                    ? reasonProperty.GetString()
                    : "Unknown";

                var text = resultProperty.TryGetProperty("text", out var textProperty)
                    ? textProperty.GetString()
                    : "";

                Output.WriteLine($"Recognition reason: {reason}");
                Output.WriteLine($"Recognition text: '{text}'");

                // For a real audio file, we expect the specific recognized text
                var expectedText = "By voice is my passport. Verify me.";

                // Assert that we got the exact expected text from the test audio file
                Assert.Equal(expectedText, text);

                Output.WriteLine($"✅ Successfully recognized expected speech: '{text}'");
            }
            else
            {
                Assert.Fail("Result JSON does not contain expected 'result' property");
            }
        }
        catch (JsonException ex)
        {
            Assert.Fail($"Failed to parse speech recognition result as JSON: {ex.Message}");
        }

        Output.WriteLine("✅ Speech recognition completed successfully with real audio file");
    }

    [Fact]
    public async Task Should_recognize_speech_with_detailed_format()
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "test-audio.wav");
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "detailed" }
            });

        Assert.NotNull(result);
        var resultText = result.ToString();
        Assert.NotNull(resultText);
        Output.WriteLine($"Detailed format result: {resultText}");

        // Parse the JSON result to verify detailed format structure
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;

        Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

        // Detailed format should include additional properties like NBest, offset, or duration
        var hasNBest = resultProperty.TryGetProperty("NBest", out _) ||
                      resultProperty.TryGetProperty("nBest", out _);
        var hasOffset = resultProperty.TryGetProperty("offset", out _);
        var hasDuration = resultProperty.TryGetProperty("duration", out _);

        Assert.True(hasNBest || hasOffset || hasDuration,
                   "Detailed format should include NBest, offset, or duration properties");

        Output.WriteLine("✅ Detailed format recognition completed successfully");
    }

    [Theory]
    [InlineData("es-ES")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    public async Task Should_handle_different_languages_gracefully(string language)
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "test-audio.wav");
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", language },
                { "format", "simple" }
            });

        // Even with wrong language, should get a result (might be NoMatch or different text)
        Assert.NotNull(result);

        var resultText = result.ToString();
        Assert.NotNull(resultText);
        Output.WriteLine($"Language {language} result: {resultText}");

        // Validate JSON structure is correct
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;
        Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

        Output.WriteLine($"✅ Language {language} test completed");
    }

    [Theory]
    [InlineData("masked")]
    [InlineData("removed")]
    [InlineData("raw")]
    public async Task Should_apply_profanity_filtering_correctly(string profanityOption)
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "test-audio.wav");
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "simple" },
                { "profanity", profanityOption }
            });

        Assert.NotNull(result);
        var resultText = result.ToString();
        Assert.NotNull(resultText);
        Output.WriteLine($"Profanity option '{profanityOption}' result: {resultText}");

        // Validate JSON structure
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;
        Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

        Output.WriteLine($"✅ Profanity filtering '{profanityOption}' test completed");
    }

    [Fact]
    public async Task Should_use_phrase_hints_effectively()
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "test-audio.wav");
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "simple" },
                { "phrases", new[] { "passport", "verify", "voice" } }
            });

        Assert.NotNull(result);
        var resultText = result.ToString();
        Assert.NotNull(resultText);
        Output.WriteLine($"Phrase hints result: {resultText}");

        // Validate JSON structure
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;
        Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

        var text = resultProperty.TryGetProperty("text", out var textProperty)
            ? textProperty.GetString()
            : "";

        // With phrase hints, we should still get the expected text
        Assert.Contains("passport", text?.ToLower() ?? "");

        Output.WriteLine("✅ Phrase hints test completed");
    }

    [Fact]
    public async Task Should_handle_invalid_endpoint_gracefully()
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "test-audio.wav");
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var invalidEndpoint = "https://invalid-endpoint.cognitiveservices.azure.com/";

        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", invalidEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "simple" }
            });

        // Should handle error gracefully - either return null or error response
        if (result != null)
        {
            var resultText = result.ToString();
            Assert.NotNull(resultText);
            Output.WriteLine($"Invalid endpoint result: {resultText}");

            // Invalid endpoint might return "NoMatch" or contain "error" - either is acceptable
            Assert.True(resultText.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                       resultText.Contains("NoMatch", StringComparison.OrdinalIgnoreCase),
                       "Result should contain either 'error' or 'NoMatch' for invalid endpoint");
        }

        Output.WriteLine("✅ Invalid endpoint error handling test completed");
    }

    [Fact]
    public async Task Should_handle_empty_audio_file_gracefully()
    {
        // Create a temporary empty file
        var emptyAudioFile = Path.GetTempFileName();
        File.WriteAllText(emptyAudioFile, ""); // Empty file

        try
        {
            var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

            var result = await CallToolAsync(
                "azmcp_speech_stt_recognize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "file", emptyAudioFile },
                    { "language", "en-US" },
                    { "format", "simple" }
                });

            // Should handle empty file gracefully
            if (result != null)
            {
                var resultText = result.ToString();
                Assert.NotNull(resultText);
                Output.WriteLine($"Empty file result: {resultText}");

                // Parse to ensure valid JSON structure
                var jsonResult = JsonDocument.Parse(resultText);
                var resultObject = jsonResult.RootElement;

                if (resultObject.TryGetProperty("result", out var resultProperty))
                {
                    var reason = resultProperty.TryGetProperty("reason", out var reasonProperty)
                        ? reasonProperty.GetString()
                        : "";

                    // Empty file might result in "NoMatch" or similar
                    Output.WriteLine($"Empty file recognition reason: {reason}");
                }
            }

            Output.WriteLine("✅ Empty file handling test completed");
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(emptyAudioFile))
            {
                File.Delete(emptyAudioFile);
            }
        }
    }

    [Fact]
    public async Task Should_handle_retry_policy_correctly()
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "test-audio.wav");
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "simple" },
                { "retry-max-retries", 3 },
                { "retry-delay", 1000 }
            });

        Assert.NotNull(result);
        var resultText = result.ToString();
        Assert.NotNull(resultText);
        Output.WriteLine($"Retry policy result: {resultText}");

        // Should work normally with retry policy
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;
        Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

        Output.WriteLine("✅ Retry policy test completed");
    }

    [Fact]
    public async Task Should_handle_large_audio_file_timeout()
    {
        // Create a large temporary file to simulate a large audio file
        var largeAudioFile = Path.GetTempFileName();
        var largeContent = new string('A', 1000000); // 1MB of content
        File.WriteAllText(largeAudioFile, largeContent);

        try
        {
            var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

            var result = await CallToolAsync(
                "azmcp_speech_stt_recognize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "file", largeAudioFile },
                    { "language", "en-US" },
                    { "format", "simple" },
                    { "retry-network-timeout", 30000 } // 30 second timeout
                });

            // Should either succeed or handle timeout gracefully
            if (result != null)
            {
                var resultText = result.ToString();
                Output.WriteLine($"Large file result: {resultText}");
            }

            Output.WriteLine("✅ Large file timeout handling test completed");
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(largeAudioFile))
            {
                File.Delete(largeAudioFile);
            }
        }
    }

    [Theory]
    [InlineData("test-audio.wav", "By voice is my passport. Verify me.")]
    [InlineData("whatstheweatherlike.mp3", "What's the weather like?")]
    public async Task Should_recognize_speech_from_different_audio_formats(string fileName, string expectedText)
    {
        // This test validates speech recognition with different audio file formats
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", fileName);

        // STRICT REQUIREMENT: The test audio file MUST exist in TestResources
        Assert.True(File.Exists(testAudioFile),
            $"Test audio file not found at: {testAudioFile}. Please ensure {fileName} exists in TestResources folder.");

        var fileInfo = new FileInfo(testAudioFile);
        Output.WriteLine($"Using test audio file: {testAudioFile}");
        Output.WriteLine($"Test audio file size: {fileInfo.Length} bytes");
        Output.WriteLine($"Test audio file format: {Path.GetExtension(fileName)}");
        Assert.True(fileInfo.Length > 0, "Test audio file must not be empty");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        // Test with the audio file - expect successful speech recognition
        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "simple" }
            });

        // STRICT REQUIREMENT: Speech recognition must return a result
        Assert.NotNull(result);

        var resultText = result.ToString();
        Output.WriteLine($"Speech recognition result: {resultText}");

        // Validate the result structure
        Assert.NotNull(resultText);
        Assert.NotEmpty(resultText);

        // Parse the JSON result to check the recognition outcome
        try
        {
            var jsonResult = JsonDocument.Parse(resultText);
            var resultObject = jsonResult.RootElement;

            if (resultObject.TryGetProperty("result", out var resultProperty))
            {
                var reason = resultProperty.TryGetProperty("reason", out var reasonProperty)
                    ? reasonProperty.GetString()
                    : "Unknown";

                var text = resultProperty.TryGetProperty("text", out var textProperty)
                    ? textProperty.GetString()
                    : "";

                Output.WriteLine($"Recognition reason: {reason}");
                Output.WriteLine($"Recognition text: '{text}'");

                // Assert that we got the expected text from the test audio file
                Assert.Equal(expectedText, text);

                Output.WriteLine($"✅ Successfully recognized expected speech from {Path.GetExtension(fileName)} format: '{text}'");
            }
            else
            {
                Assert.Fail("Result JSON does not contain expected 'result' property");
            }
        }
        catch (JsonException ex)
        {
            Assert.Fail($"Failed to parse speech recognition result as JSON: {ex.Message}");
        }

        Output.WriteLine($"✅ Speech recognition completed successfully with {Path.GetExtension(fileName)} audio file");
    }

    [Theory]
    [InlineData("test-audio.wav")]
    [InlineData("whatstheweatherlike.mp3")]
    public async Task Should_handle_detailed_format_with_different_audio_formats(string fileName)
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", fileName);
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "detailed" }
            });

        Assert.NotNull(result);
        var resultText = result.ToString();
        Assert.NotNull(resultText);
        Output.WriteLine($"Detailed format result for {Path.GetExtension(fileName)}: {resultText}");

        // Parse the JSON result to verify detailed format structure
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;

        Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

        // Detailed format should include additional properties like NBest, offset, or duration
        var hasNBest = resultProperty.TryGetProperty("NBest", out _) ||
                      resultProperty.TryGetProperty("nBest", out _);
        var hasOffset = resultProperty.TryGetProperty("offset", out _);
        var hasDuration = resultProperty.TryGetProperty("duration", out _);

        Assert.True(hasNBest || hasOffset || hasDuration,
                   "Detailed format should include NBest, offset, or duration properties");

        Output.WriteLine($"✅ Detailed format recognition completed successfully for {Path.GetExtension(fileName)}");
    }

    [Theory]
    [InlineData("test-audio.wav")]
    [InlineData("whatstheweatherlike.mp3")]
    public async Task Should_apply_phrase_hints_with_different_audio_formats(string fileName)
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", fileName);
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        // Use appropriate phrase hints based on the file
        var phrases = fileName.Contains("weather")
            ? new[] { "weather", "like", "what" }
            : new[] { "passport", "verify", "voice" };

        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "simple" },
                { "phrases", phrases }
            });

        Assert.NotNull(result);
        var resultText = result.ToString();
        Assert.NotNull(resultText);
        Output.WriteLine($"Phrase hints result for {Path.GetExtension(fileName)}: {resultText}");

        // Validate JSON structure
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;
        Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

        var text = resultProperty.TryGetProperty("text", out var textProperty)
            ? textProperty.GetString()
            : "";

        // With phrase hints, we should get text containing at least one of the hint words
        var containsHint = phrases.Any(phrase => text?.ToLower().Contains(phrase.ToLower()) == true);
        Assert.True(containsHint, $"Recognition text should contain at least one phrase hint. Text: '{text}', Hints: [{string.Join(", ", phrases)}]");

        Output.WriteLine($"✅ Phrase hints test completed for {Path.GetExtension(fileName)}");
    }

    [Fact]
    public async Task Should_handle_large_mp3_file_timeout()
    {
        // Create a large temporary MP3 file to simulate a large audio file
        var largeMp3File = Path.GetTempFileName();
        var largeMp3FilePath = Path.ChangeExtension(largeMp3File, ".mp3");
        File.Move(largeMp3File, largeMp3FilePath);

        var largeContent = new string('A', 1000000); // 1MB of content
        File.WriteAllText(largeMp3FilePath, largeContent);

        try
        {
            var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

            var result = await CallToolAsync(
                "azmcp_speech_stt_recognize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "file", largeMp3FilePath },
                    { "language", "en-US" },
                    { "format", "simple" },
                    { "retry-network-timeout", 30000 } // 30 second timeout
                });

            // Should either succeed or handle timeout gracefully
            if (result != null)
            {
                var resultText = result.ToString();
                Output.WriteLine($"Large MP3 file result: {resultText}");
            }

            Output.WriteLine("✅ Large MP3 file timeout handling test completed");
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(largeMp3FilePath))
            {
                File.Delete(largeMp3FilePath);
            }
        }
    }

    [Theory]
    [InlineData("test-audio.wav")]
    [InlineData("whatstheweatherlike.mp3")]
    public async Task Should_handle_profanity_filtering_with_different_formats(string fileName)
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", fileName);
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        foreach (var profanityOption in new[] { "masked", "removed", "raw" })
        {
            var result = await CallToolAsync(
                "azmcp_speech_stt_recognize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "file", testAudioFile },
                    { "language", "en-US" },
                    { "format", "simple" },
                    { "profanity", profanityOption }
                });

            Assert.NotNull(result);
            var resultText = result.ToString();
            Assert.NotNull(resultText);
            Output.WriteLine($"Profanity option '{profanityOption}' result for {Path.GetExtension(fileName)}: {resultText}");

            // Validate JSON structure
            var jsonResult = JsonDocument.Parse(resultText);
            var resultObject = jsonResult.RootElement;
            Assert.True(resultObject.TryGetProperty("result", out var resultProperty));
        }

        Output.WriteLine($"✅ Profanity filtering test completed for {Path.GetExtension(fileName)}");
    }
}
