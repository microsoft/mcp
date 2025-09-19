// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Options.Namespace;

public class NamespaceGetOptions : BaseEventHubsOptions
{
    // No additional options needed for namespace get beyond base options
    // ResourceGroup is required and handled by OptionDefinitions.Common.ResourceGroup.AsRequired() in the command
}
