// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Areas.Server.Configuration;

/// <summary>
/// Validates server configuration at startup to catch misconfigurations early.
/// </summary>
internal static class ConfigurationValidator
{
    /// <summary>
    /// Validates the server configuration, checking all authentication requirements.
    /// </summary>
    /// <param name="config">The server configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    internal static void Validate(ServerConfiguration config)
    {
        ValidateAuthenticationTypes(config);
        ValidateOnBehalfOfConfiguration(config);
        ValidateBearerTokenConfiguration(config);
        ValidateManagedIdentityConfiguration(config);
    }

    /// <summary>
    /// Validates the combination of inbound and outbound authentication types.
    /// </summary>
    /// <param name="config">The server configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the authentication type combination is invalid.</exception>
    private static void ValidateAuthenticationTypes(ServerConfiguration config)
    {
        var inboundType = config.InboundAuthentication.Type;
        var outboundType = config.OutboundAuthentication.Type;

        // Default outbound requires None inbound
        if (outboundType == OutboundAuthenticationType.Default
            && inboundType != InboundAuthenticationType.None)
        {
            throw new InvalidOperationException(
                "OutboundAuthentication.Type 'Default' requires InboundAuthentication.Type to be 'None'. " +
                $"Current InboundAuthentication.Type is '{inboundType}'.");
        }

        // ManagedIdentity outbound can have None or EntraIDAccessToken inbound
        if (outboundType == OutboundAuthenticationType.ManagedIdentity
            && inboundType != InboundAuthenticationType.None
            && inboundType != InboundAuthenticationType.EntraIDAccessToken)
        {
            throw new InvalidOperationException(
                "OutboundAuthentication.Type 'ManagedIdentity' requires InboundAuthentication.Type to be either 'None' or 'EntraIDAccessToken'. " +
                $"Current InboundAuthentication.Type is '{inboundType}'.");
        }

        // BearerToken outbound can have None or EntraIDAccessToken inbound
        if (outboundType == OutboundAuthenticationType.BearerToken
            && inboundType != InboundAuthenticationType.None
            && inboundType != InboundAuthenticationType.EntraIDAccessToken)
        {
            throw new InvalidOperationException(
                "OutboundAuthentication.Type 'BearerToken' requires InboundAuthentication.Type to be either 'None' or 'EntraIDAccessToken'. " +
                $"Current InboundAuthentication.Type is '{inboundType}'.");
        }

        // OnBehalfOf requires EntraIDAccessToken inbound
        if (outboundType == OutboundAuthenticationType.OnBehalfOf
            && inboundType != InboundAuthenticationType.EntraIDAccessToken)
        {
            throw new InvalidOperationException(
                "OutboundAuthentication.Type 'OnBehalfOf' requires InboundAuthentication.Type to be 'EntraIDAccessToken'. " +
                $"Current InboundAuthentication.Type is '{inboundType}'.");
        }
    }

    /// <summary>
    /// Validates On-Behalf-Of configuration requirements.
    /// </summary>
    /// <param name="config">The server configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when OBO configuration is invalid.</exception>
    private static void ValidateOnBehalfOfConfiguration(ServerConfiguration config)
    {
        if (config.OutboundAuthentication.Type != OutboundAuthenticationType.OnBehalfOf)
        {
            return; // Only validate when using OBO
        }

        // OutboundAuthentication.AzureAd must be present
        if (config.OutboundAuthentication.AzureAd is null)
        {
            throw new InvalidOperationException(
                "OutboundAuthentication.Type 'OnBehalfOf' requires OutboundAuthentication.AzureAd to be configured.");
        }

        // InboundAuthentication.AzureAd must be present
        if (config.InboundAuthentication.AzureAd is null)
        {
            throw new InvalidOperationException(
                "OutboundAuthentication.Type 'OnBehalfOf' requires InboundAuthentication.AzureAd to be configured.");
        }

        ValidateAzureAdConfiguration(config.InboundAuthentication.AzureAd);
        ValidateAzureAdConfiguration(config.OutboundAuthentication.AzureAd);

        // ClientSecret must be present in OutboundAuthentication.AzureAd
        if (string.IsNullOrWhiteSpace(config.OutboundAuthentication.AzureAd.ClientSecret))
        {
            throw new InvalidOperationException(
                "OutboundAuthentication.Type 'OnBehalfOf' requires OutboundAuthentication.AzureAd.ClientSecret to be configured.");
        }

        // Verify that Instance, TenantId, ClientId, and Audience match between inbound and outbound
        var inbound = config.InboundAuthentication.AzureAd;
        var outbound = config.OutboundAuthentication.AzureAd;

        if (!string.Equals(inbound.Instance, outbound.Instance, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"OutboundAuthentication.AzureAd.Instance ('{outbound.Instance}') must match InboundAuthentication.AzureAd.Instance ('{inbound.Instance}').");
        }

        if (!string.Equals(inbound.TenantId, outbound.TenantId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"OutboundAuthentication.AzureAd.TenantId ('{outbound.TenantId}') must match InboundAuthentication.AzureAd.TenantId ('{inbound.TenantId}').");
        }

        if (!string.Equals(inbound.ClientId, outbound.ClientId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"OutboundAuthentication.AzureAd.ClientId ('{outbound.ClientId}') must match InboundAuthentication.AzureAd.ClientId ('{inbound.ClientId}').");
        }

        if (!string.Equals(inbound.Audience, outbound.Audience, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"OutboundAuthentication.AzureAd.Audience ('{outbound.Audience}') must match InboundAuthentication.AzureAd.Audience ('{inbound.Audience}').");
        }
    }

    /// <summary>
    /// Validates Bearer Token configuration requirements.
    /// </summary>
    /// <param name="config">The server configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when Bearer Token configuration is invalid.</exception>
    private static void ValidateBearerTokenConfiguration(ServerConfiguration config)
    {
        if (config.OutboundAuthentication.Type != OutboundAuthenticationType.BearerToken)
        {
            return; // Only validate when using Bearer Token
        }

        // HeaderName must be present
        if (string.IsNullOrWhiteSpace(config.OutboundAuthentication.HeaderName))
        {
            throw new InvalidOperationException(
                "OutboundAuthentication.Type 'BearerToken' requires OutboundAuthentication.HeaderName to be configured.");
        }

        // If inbound auth is EntraIDAccessToken, AzureAd must be present
        if (config.InboundAuthentication.Type == InboundAuthenticationType.EntraIDAccessToken
            && config.InboundAuthentication.AzureAd is null)
        {
            throw new InvalidOperationException(
                "InboundAuthentication.Type 'EntraIDAccessToken' requires InboundAuthentication.AzureAd to be configured.");
        }

        if (config.InboundAuthentication.Type == InboundAuthenticationType.EntraIDAccessToken)
        {
            ValidateAzureAdConfiguration(config.InboundAuthentication.AzureAd!);
        }
    }

    /// <summary>
    /// Validates Managed Identity configuration requirements.
    /// </summary>
    /// <param name="config">The server configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when Managed Identity configuration is invalid.</exception>
    private static void ValidateManagedIdentityConfiguration(ServerConfiguration config)
    {
        if (config.OutboundAuthentication.Type != OutboundAuthenticationType.ManagedIdentity)
        {
            return; // Only validate when using Managed Identity
        }

        // If inbound auth is EntraIDAccessToken, AzureAd must be present
        if (config.InboundAuthentication.Type == InboundAuthenticationType.EntraIDAccessToken
            && config.InboundAuthentication.AzureAd is null)
        {
            throw new InvalidOperationException(
                "InboundAuthentication.Type 'EntraIDAccessToken' requires InboundAuthentication.AzureAd to be configured.");
        }

        if (config.InboundAuthentication.Type == InboundAuthenticationType.EntraIDAccessToken)
        {
            ValidateAzureAdConfiguration(config.InboundAuthentication.AzureAd!);
        }
    }

    /// <summary>
    /// Validates Azure AD configuration requirements.
    /// </summary>
    /// <param name="azureAd">The Azure AD configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when Azure AD configuration is invalid.</exception>
    private static void ValidateAzureAdConfiguration(AzureAdConfig azureAd)
    {
        if (string.IsNullOrWhiteSpace(azureAd.Instance))
        {
            throw new InvalidOperationException("AzureAd.Instance cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(azureAd.TenantId))
        {
            throw new InvalidOperationException("AzureAd.TenantId cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(azureAd.ClientId))
        {
            throw new InvalidOperationException("AzureAd.ClientId cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(azureAd.Audience))
        {
            throw new InvalidOperationException("AzureAd.Audience cannot be null or empty.");
        }
    }
}
