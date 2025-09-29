// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests.Client;
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

    [Theory]
    [InlineData("test-audio.wav", "By voice is my passport. Verify me.")]
    [InlineData("TheGreatGatsby.wav", "In my younger and more vulnerable years, my father gave me some advice that I've been turning over in my mind ever since. Whenever you feel like criticizing anyone, he told me, just remember that all the people in this world haven't had the advantages that you've had. He didn't say anymore, but we've always been unusually commutative in a reserved way, and I understood that he meant a great deal more than that. In consequence, I'm inclined to reserve all judgments, a habit that has opened up many curious natures to me.")]
    public async Task Should_recognize_speech_from_real_audio_file(string fileName, string expectedText)
    {
        // This test validates complete end-to-end speech recognition with real audio files of different durations from TestResources
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", fileName);

        // STRICT REQUIREMENT: The test audio file MUST exist in TestResources
        Assert.True(File.Exists(testAudioFile),
            $"Test audio file not found at: {testAudioFile}. Please ensure {fileName} exists in TestResources folder.");

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
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;

        Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

        // Assert that we got the exact expected text from the test audio file
        Assert.True(resultProperty.TryGetProperty("fullText", out var textProperty));
        var fullText = textProperty.GetString() ?? "";
        Output.WriteLine($"Recognition text: '{fullText}'");
        Assert.Equal(expectedText, fullText);

        Assert.True(resultProperty.TryGetProperty("segments", out var segmentsProperty));
        var segments = segmentsProperty.EnumerateArray().ToArray();
        Assert.True(segmentsProperty.GetArrayLength() > 0, $"Expected at least one segment, but got {segments.Length}");

        // Verify each segment has RecognizedSpeech reason
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            var segmentReason = segment.TryGetProperty("reason", out var segmentReasonProperty)
                ? segmentReasonProperty.GetString()
                : "Unknown";

            Output.WriteLine($"Segment {i + 1} reason: {segmentReason}");
            Assert.Equal("RecognizedSpeech", segmentReason);
        }
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

        Assert.True(resultProperty.TryGetProperty("segments", out var segmentsProperty));
        Assert.True(segmentsProperty.GetArrayLength() > 0);

        var firstSegment = segmentsProperty[0];
        var hasNBest = firstSegment.TryGetProperty("nBest", out _);
        var hasOffset = firstSegment.TryGetProperty("offset", out _);
        var hasDuration = firstSegment.TryGetProperty("duration", out _);

        Assert.True(hasNBest || hasOffset || hasDuration,
                   "Detailed format should include NBest, offset, or duration properties in segments");

        Output.WriteLine("✅ Detailed format recognition completed successfully");
    }

    [Theory]
    [InlineData("ar-AE", "ar-rewind-music.wav", "ارجع الموسيقى 20 ثانية.")]
    [InlineData("es-ES", "es-ES.wav", "Rebobinar la música 20 segundos.")]
    [InlineData("fr-FR", "fr-FR.wav", "Rembobiner la musique de 20 secondes.")]
    [InlineData("de-DE", "de-DE.wav", "Treffen heute um 17:00 Uhr.")]
    public async Task Should_handle_different_languages(string language, string fileName, string expectedText)
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
                { "language", language },
                { "format", "simple" }
            });

        Assert.NotNull(result);

        var resultText = result.ToString();
        Assert.NotNull(resultText);

        // Validate JSON structure is correct
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;
        Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

        // Validate full text property
        Assert.True(resultProperty.TryGetProperty("fullText", out var textProperty));
        var fullText = textProperty.GetString() ?? "";
        Output.WriteLine($"Recognition text: '{fullText}'");
        Assert.Equal(expectedText, fullText);

        Assert.True(resultProperty.TryGetProperty("segments", out var segmentsProperty));
        var segments = segmentsProperty.EnumerateArray().ToArray();
        Assert.True(segmentsProperty.GetArrayLength() > 0, $"Expected at least one segment, but got {segments.Length}");

        // Verify each segment has RecognizedSpeech reason
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            var segmentReason = segment.TryGetProperty("reason", out var segmentReasonProperty)
                ? segmentReasonProperty.GetString()
                : "Unknown";

            Output.WriteLine($"Segment {i + 1} reason: {segmentReason}");
            Assert.Equal("RecognizedSpeech", segmentReason);
        }

        Output.WriteLine($"✅ Language {language} test completed");
    }

    [Theory]
    [InlineData("masked", "You don't deserve it, you *******. **** you.")]
    [InlineData("removed", "You don't deserve it, you .  you.")]
    [InlineData("raw", "You don't deserve it, you bastard. Fuck you.")]
    public async Task Should_apply_profanity_filtering_correctly(string profanityOption, string expectedText)
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "en-US-with-profanity.wav");
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

        Assert.True(resultProperty.TryGetProperty("fullText", out var textProperty));
        var fullText = textProperty.GetString() ?? "";
        Output.WriteLine($"Recognition text: '{fullText}'");
        Assert.Equal(expectedText, fullText);

        Output.WriteLine($"✅ Profanity filtering '{profanityOption}' test completed");
    }

    [Fact]
    public async Task Should_use_phrase_hints_effectively()
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "en-US-phraselist.wav");
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
                { "phrases", new[] { "Douzi", "Shitou", "Cheng Dieyi", "Duan Xiaolou" } }
            });

        Assert.NotNull(result);
        var resultText = result.ToString();
        Assert.NotNull(resultText);
        Output.WriteLine($"Phrase hints result: {resultText}");

        // Validate JSON structure
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;
        Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

        var fullText = resultProperty.TryGetProperty("fullText", out var textProperty)
            ? textProperty.GetString()
            : "";
        Output.WriteLine($"Recognition text: '{fullText}'");
        var expectedText = "Years later, Douzi and Shitou have become packing opera stars, taking the names Cheng Dieyi and Duan Xiaolou, respectively.";
        Assert.Equal(expectedText, fullText);

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

        Assert.NotNull(result);
        var resultText = result.ToString();
        Assert.NotNull(resultText);
        Output.WriteLine($"Invalid endpoint result: {resultText}");

        Assert.True(resultText.Contains("Invalid endpoint or connectivity issue.", StringComparison.OrdinalIgnoreCase));

        Output.WriteLine("✅ Invalid endpoint error handling test completed");
    }

    [Fact]
    public async Task Should_handle_empty_audio_file_gracefully()
    {
        // Create a valid empty WAV file
        var emptyWavFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        CreateWavFile(emptyWavFile);

        try
        {
            var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

            var result = await CallToolAsync(
                "azmcp_speech_stt_recognize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "file", emptyWavFile },
                    { "language", "en-US" },
                    { "format", "simple" }
                });

            // Should handle empty file gracefully
            Assert.NotNull(result);
            var resultText = result.ToString();
            Assert.NotNull(resultText);
            Output.WriteLine($"Empty file result: {resultText}");

            // Parse to ensure valid JSON structure
            var jsonResult = JsonDocument.Parse(resultText);
            var resultObject = jsonResult.RootElement;

            Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

            Assert.True(resultProperty.TryGetProperty("segments", out var segmentsProperty));
            var segments = segmentsProperty.EnumerateArray().ToArray();
            Assert.True(segmentsProperty.GetArrayLength() > 0, $"Expected at least one segment, but got {segments.Length}");

            // Verify each segment has NoMatch reason
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                var segmentReason = segment.TryGetProperty("reason", out var segmentReasonProperty)
                    ? segmentReasonProperty.GetString()
                    : "Unknown";

                Output.WriteLine($"Segment {i + 1} reason: {segmentReason}");
                Assert.Equal("NoMatch", segmentReason);
            }

            Output.WriteLine("✅ Empty file handling test completed");
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(emptyWavFile))
            {
                File.Delete(emptyWavFile);
            }
        }
    }

    [Fact]
    public async Task Should_handle_broken_file_gracefully()
    {
        // Create a temporary empty file
        var emptyWavFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        File.WriteAllText(emptyWavFile, ""); // Empty content

        try
        {
            var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

            var result = await CallToolAsync(
                "azmcp_speech_stt_recognize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "file", emptyWavFile },
                    { "language", "en-US" },
                    { "format", "simple" }
                });

            // Should handle empty file gracefully
            Assert.NotNull(result);
            var resultText = result.ToString();
            Assert.NotNull(resultText);
            Output.WriteLine($"Empty file result: {resultText}");

            // Parse to ensure valid JSON structure
            var jsonResult = JsonDocument.Parse(resultText);
            var resultObject = jsonResult.RootElement;

            // Validate Error message for corrupted file
            Assert.True(resultObject.TryGetProperty("message", out var messageProperty));
            var message = messageProperty.GetString() ?? "";
            Assert.True(message.Contains("The audio file appears to be empty or corrupted. Please provide a valid audio file.", StringComparison.OrdinalIgnoreCase));

            // Validate exception type
            Assert.True(resultObject.TryGetProperty("type", out var exceptionTypeProperty));
            var exceptionType = exceptionTypeProperty.GetString() ?? "";
            Assert.True(exceptionType.Contains("InvalidOperationException", StringComparison.OrdinalIgnoreCase));

            Output.WriteLine("✅ Empty file handling test completed");
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(emptyWavFile))
            {
                File.Delete(emptyWavFile);
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

    [Theory]
    [InlineData("whatstheweatherlike.mp3")]
    public async Task Should_fail_to_recognize_compressed_audio_without_gstreamer(string fileName)
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

        // Should handle empty file gracefully
        Assert.NotNull(result);
        var resultText = result.ToString();
        Assert.NotNull(resultText);
        Output.WriteLine($"Empty file result: {resultText}");

        // Parse to ensure valid JSON structure
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;

        // Validate Error message for corrupted file
        Assert.True(resultObject.TryGetProperty("message", out var messageProperty));
        var message = messageProperty.GetString() ?? "";
        Assert.True(message.Contains("Cannot process compressed audio file", StringComparison.OrdinalIgnoreCase));
        Assert.True(message.Contains("because GStreamer is not properly installed or configured.", StringComparison.OrdinalIgnoreCase));

        // Validate exception type
        Assert.True(resultObject.TryGetProperty("type", out var exceptionTypeProperty));
        var exceptionType = exceptionTypeProperty.GetString() ?? "";
        Assert.True(exceptionType.Contains("InvalidOperationException", StringComparison.OrdinalIgnoreCase));
    }

    [Theory(Skip = "Requires GStreamer installed to handle compressed audio formats (e.g., MP3). Skipped if GStreamer is not available.")]
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

        // With ContinuousRecognitionResult, detailed format properties are in the segments
        Assert.True(resultProperty.TryGetProperty("segments", out var segmentsProperty));
        Assert.True(segmentsProperty.GetArrayLength() > 0);

        var firstSegment = segmentsProperty[0];
        var hasNBest = firstSegment.TryGetProperty("nBest", out _);
        var hasOffset = firstSegment.TryGetProperty("offset", out _);
        var hasDuration = firstSegment.TryGetProperty("duration", out _);

        Assert.True(hasNBest || hasOffset || hasDuration,
                   "Detailed format should include NBest, offset, or duration properties in segments");

        Output.WriteLine($"✅ Detailed format recognition completed successfully for {Path.GetExtension(fileName)}");
    }

    [Theory(Skip = "Requires GStreamer installed to handle compressed audio formats (e.g., MP3). Skipped if GStreamer is not available.")]
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

        var text = resultProperty.TryGetProperty("fullText", out var textProperty)
            ? textProperty.GetString()
            : "";

        // With phrase hints, we should get text containing at least one of the hint words
        var containsHint = phrases.Any(phrase => text?.ToLower().Contains(phrase.ToLower()) == true);
        Assert.True(containsHint, $"Recognition text should contain at least one phrase hint. Text: '{text}', Hints: [{string.Join(", ", phrases)}]");

        Output.WriteLine($"✅ Phrase hints test completed for {Path.GetExtension(fileName)}");
    }

    [Theory(Skip = "Requires GStreamer installed to handle compressed audio formats (e.g., MP3). Skipped if GStreamer is not available.")]
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

    /// <summary>
    /// Create a WAV file with given duration (seconds). 
    /// If durationSeconds = 0, generates an empty WAV file with header only.
    /// </summary>
    private static void CreateWavFile(string filePath, int durationSeconds = 0)
    {
        int sampleRate = 16000;    // 16kHz
        short bitsPerSample = 16;  // 16-bit
        short channels = 1;        // mono
        int totalSamples = sampleRate * durationSeconds;
        int byteRate = sampleRate * channels * (bitsPerSample / 8);

        using var fs = new FileStream(filePath, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        // RIFF header
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + totalSamples * 2); // RIFF size
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        // fmt chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);          // PCM chunk size
        writer.Write((short)1);    // PCM format
        writer.Write(channels);    // channels
        writer.Write(sampleRate);  // sample rate
        writer.Write(byteRate);    // byte rate
        writer.Write((short)(channels * bitsPerSample / 8)); // block align
        writer.Write(bitsPerSample);

        // data chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(totalSamples * 2); // data chunk size

        // Write silence (zeros) for the specified duration
        for (int i = 0; i < totalSamples; i++)
        {
            writer.Write((short)0);
        }
    }
}

