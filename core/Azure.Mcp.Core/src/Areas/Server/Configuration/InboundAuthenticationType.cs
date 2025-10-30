// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Configuration;

/// <summary>
/// Specifies the type of inbound authentication (incoming request validation).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<InboundAuthenticationType>))]
public enum InboundAuthenticationType
{
    /// <summary>
    /// No authentication validation. All incoming requests are trusted.
    /// </summary>
    None,

    /// <summary>
    /// Validates incoming requests using JWT Bearer token scheme.
    /// Requires AzureAd configuration with TenantId, ClientId, and Audience.
    /// </summary>
    JwtBearerScheme
}
