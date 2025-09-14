// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Speech.Options;

namespace Azure.Mcp.Tools.Speech.Commands;

public abstract class BaseSpeechCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : SubscriptionCommand<T>
    where T : BaseSpeechOptions, new()
{
    protected readonly Option<string> _endpointOption = SpeechOptionDefinitions.Endpoint;
}
