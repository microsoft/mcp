// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Speech.Options;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Speech.Commands;

public abstract class BaseSpeechCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : SubscriptionCommand<T>
    where T : BaseSpeechOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        var endpointOption = SpeechOptionDefinitions.Endpoint.AsRequired();
        command.Options.Add(endpointOption);
        command.Validators.Add(commandResult =>
        {
            // Validate endpoint option
            var endpointValue = commandResult.GetValueOrDefault(endpointOption);

            if (!Uri.TryCreate(endpointValue, UriKind.Absolute, out var uri))
            {
                commandResult.AddError($"Invalid endpoint URL: {endpointValue}");
                return;
            }

            if (uri.Scheme != Uri.UriSchemeHttps)
            {
                commandResult.AddError($"Endpoint must use HTTPS: {endpointValue}");
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
                commandResult.AddError($"Endpoint must be a valid Azure AI Services endpoint. Host must end with '.cognitiveservices.azure.com' (or sovereign cloud equivalent): {uri.Host}");
                return;
            }

            var subdomain = uri.Host[..^matchedSuffix.Length];
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                commandResult.AddError($"Endpoint must include a valid service name before '{matchedSuffix}'");
            }
        });
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(SpeechOptionDefinitions.Endpoint.Name);
        return options;
    }
}
