// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Speech.Options;

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

            if (!uri.Host.EndsWith(".cognitiveservices.azure.com", StringComparison.OrdinalIgnoreCase))
            {
                commandResult.AddError($"Endpoint must be a valid Azure AI Services endpoint. Host must end with '.cognitiveservices.azure.com': {uri.Host}");
                return;
            }

            var subdomain = uri.Host.Replace(".cognitiveservices.azure.com", "", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                commandResult.AddError($"Endpoint must include a valid service name before '.cognitiveservices.azure.com'");
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
