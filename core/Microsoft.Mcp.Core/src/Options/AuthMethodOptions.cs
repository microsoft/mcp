// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Option;

namespace Azure.Mcp.Core.Options;

/// <summary>
/// Represents authentication method configuration for Azure SDK clients
/// </summary>
public class AuthMethodOptions
{
    [JsonPropertyName(OptionDefinitions.Common.AuthMethodName)]
    public AuthMethod AuthMethod { get; set; }

    /// <summary>
    /// Gets a display-friendly name for the auth method
    /// </summary>
    public static string GetDisplayName(AuthMethod authMethod) => authMethod switch
    {
        AuthMethod.Credential => "Credential",
        AuthMethod.Key => "Key",
        AuthMethod.ConnectionString => "Connection String",
        _ => authMethod.ToString()
    };

    /// <summary>
    /// Gets the default auth method
    /// </summary>
    public static AuthMethod GetDefaultAuthMethod() => AuthMethod.Credential;
}
