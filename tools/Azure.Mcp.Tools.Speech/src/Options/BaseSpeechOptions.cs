// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Speech.Options;

public abstract class BaseSpeechOptions
{
    [Option(Description = "The Azure AI Services endpoint URL (e.g., https://your-service.cognitiveservices.azure.com/).")]
    public required string Endpoint { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
