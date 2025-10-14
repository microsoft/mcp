// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Models;

/// <summary>
/// OAuth 2.0 protected resource metadata response model.
/// </summary>
public sealed class OAuthProtectedResourceMetadata
{
    [JsonPropertyName("resource")]
    public required string Resource { get; init; }
    
    [JsonPropertyName("authorization_servers")]
    public required string[] AuthorizationServers { get; init; }
    
    [JsonPropertyName("scopes_supported")]
    public required string[] ScopesSupported { get; init; }
    
    [JsonPropertyName("bearer_methods_supported")]
    public required string[] BearerMethodsSupported { get; init; }
    
    [JsonPropertyName("resource_documentation")]
    public required string ResourceDocumentation { get; init; }
    
    [JsonPropertyName("resource_signing_alg_values_supported")]
    public required string[] ResourceSigningAlgValuesSupported { get; init; }
}

/// <summary>
/// JSON serializer context for AOT-safe serialization.
/// </summary>
[JsonSerializable(typeof(OAuthProtectedResourceMetadata))]
internal partial class OAuthMetadataJsonContext : JsonSerializerContext
{
}
