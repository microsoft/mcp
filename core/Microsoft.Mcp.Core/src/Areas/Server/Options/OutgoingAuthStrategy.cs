// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Identity;

namespace Azure.Mcp.Core.Areas.Server.Options;

/// <summary>
/// The strategy to use for authenticating outgoing requests to downstream services.
/// </summary>
public enum OutgoingAuthStrategy
{
    /// <summary>
    /// The value is not set and is in a default state. A safe default will
    /// be chosen based on other settings.
    /// </summary>
    NotSet = 0,

    /// <summary>
    /// Outgoing requests will use the hosting environment's identity resolving
    /// in a similar way as <see cref="DefaultAzureCredential"/>. This is valid
    /// for all hosting scenarios. This means all outgoing requests will use the
    /// same identity regardless of the incoming authenticate request identity,
    /// if any.
    /// </summary>
    UseHostingEnvironmentIdentity = 1,

    /// <summary>
    /// Outgoing requests will be authenticated based on exchanging the incoming
    /// request's access token for a new access token valid for the downstream
    /// service. This is only valid for remote MCP server scenarios.
    /// </summary>
    UseOnBehalfOf = 2
}
