// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Speech.Models;

/// <summary>
/// Defines the type of speech recognizer that produced the result.
/// </summary>
public enum RecognizerType
{
    /// <summary>
    /// Real-time continuous speech recognition using Azure Speech SDK.
    /// </summary>
    Realtime,

    /// <summary>
    /// Fast transcription using Azure AI Services Speech REST API.
    /// </summary>
    Fast
}
