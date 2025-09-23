// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Speech.Options;

namespace Azure.Mcp.Tools.Speech.Commands;

public abstract class BaseSpeechCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : SubscriptionCommand<T>
    where T : BaseSpeechOptions, new()
{
    protected readonly Option<string> _endpointOption = SpeechOptionDefinitions.Endpoint;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_endpointOption);

        // Command-level validation for endpoint
        command.Validators.Add(commandResult =>
        {
            // Validate endpoint is provided
            if (!commandResult.HasOptionResult(_endpointOption))
            {
                commandResult.AddError("Azure AI Services endpoint is required (e.g., https://your-service.cognitiveservices.azure.com/).");
                return;
            }

            if (commandResult.TryGetValue(_endpointOption, out var endpointValue) && !string.IsNullOrWhiteSpace(endpointValue))
            {
                // Validate endpoint format
                if (!Uri.TryCreate(endpointValue, UriKind.Absolute, out var endpointUri) || !endpointUri.Host.EndsWith(".cognitiveservices.azure.com", StringComparison.OrdinalIgnoreCase))
                {
                    commandResult.AddError("Endpoint must be a valid Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/).");
                }
            }
        });
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault(_endpointOption);
        return options;
    }
}
