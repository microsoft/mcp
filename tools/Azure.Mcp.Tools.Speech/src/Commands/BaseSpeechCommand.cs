// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.Speech.Options;
using Azure.Mcp.Tools.Speech.Services;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Speech.Commands;

public abstract class BaseSpeechCommand<[DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>
    : AuthenticatedCommand<TOptions, TResult> where TOptions : BaseSpeechOptions
{
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Command validation has no configured cloud context, so parse the resource root once and accept it when any
        // explicitly supported cloud authorizes it. The selected service implementation later authorizes the exact
        // operation endpoint against the configured cloud.
        if (!SpeechEndpointValidator.IsValidForAnySupportedCloud(options.Endpoint))
        {
            validationResult.Errors.Add("Endpoint must be a valid Azure AI Services endpoint. It must be an HTTPS resource root in a supported Azure cloud.");
        }
    }
}
