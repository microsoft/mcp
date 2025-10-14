using Azure.Identity;

namespace Azure.Mcp.Core.Areas.Server.Options;

public enum OutgoingAuthenticationTypes
{
    /// <summary>
    /// The value is not set and is in a default state. This is valid for
    /// any non-remote MCP server scenarios.
    /// </summary>
    NotSet = 0,

    /// <summary>
    /// Outgoing requests will use the hosting environment's identity resolving
    /// in the same way as <see cref="DefaultAzureCredential"/>. This is valid
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
