// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Speech.Models;

namespace Azure.Mcp.Tools.Speech.Services;

public interface ISpeechService
{
    Task<SpeechRecognitionResult> RecognizeSpeechFromFile(
        string endpoint,
        string filePath,
        string? language = null,
        string[]? phrases = null,
        string? format = null,
        string? profanity = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<SynthesisResult> SynthesizeSpeechToFile(
        string endpoint,
        string text,
        string outputFilePath,
        string? language = null,
        string? voice = null,
        string? format = null,
        string? endpointId = null,
        RetryPolicyOptions? retryPolicy = null);
}
