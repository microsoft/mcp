// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Communication.Options;

/// <summary>
/// Base options class for Communication Services commands.
/// </summary>
public class BaseCommunicationOptions
{
    [Option(Description = "The Communication Services URI endpoint (e.g., https://myservice.communication.azure.com). Required for credential authentication.")]
    public required string Endpoint { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
