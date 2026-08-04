// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.Speech.Options;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Speech.Commands;

public abstract class BaseSpeechCommand<[DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>
    : AuthenticatedCommand<TOptions, TResult> where TOptions : BaseSpeechOptions
{
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Validate endpoint option
        if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var uri))
        {
            validationResult.Errors.Add($"Invalid endpoint URL: {options.Endpoint}");
            return;
        }

        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            validationResult.Errors.Add($"Endpoint must use HTTPS: {options.Endpoint}");
            return;
        }

        // Accept sovereign cloud endpoint suffixes
        string[] validSuffixes =
        [
            ".cognitiveservices.azure.com",
                ".cognitiveservices.azure.cn",
                ".cognitiveservices.azure.us"
        ];
        var matchedSuffix = Array.Find(validSuffixes, suffix => uri.Host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        if (matchedSuffix == null)
        {
            validationResult.Errors.Add($"Endpoint must be a valid Azure AI Services endpoint. Host must end with '.cognitiveservices.azure.com' (or sovereign cloud equivalent): {uri.Host}");
            return;
        }

        var subdomain = uri.Host[..^matchedSuffix.Length];
        if (string.IsNullOrWhiteSpace(subdomain))
        {
            validationResult.Errors.Add($"Endpoint must include a valid service name before '{matchedSuffix}'");
        }
    }
}
