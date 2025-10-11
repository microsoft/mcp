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
    /// 
    /// </summary>
    /// <param name="config">The server configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    internal static void Validate(ServerConfiguration config)
    {
        ValidateAuthenticationTypes(config);
        // Validate configuration requirements for specific authentication types
        ValidateJwtOboConfiguration(config);
        ValidateJwtPassthroughConfiguration(config);
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

        if (inboundType == InboundAuthenticationType.None)
        {
            // When Inbound=None, only Default, ManagedIdentity, JwtPassthrough are allowed
            if (outboundType != OutboundAuthenticationType.Default &&
                outboundType != OutboundAuthenticationType.ManagedIdentity &&
                outboundType != OutboundAuthenticationType.JwtPassthrough)
            {
                throw new InvalidOperationException(
                    $"InboundAuthentication.Type 'None' requires OutboundAuthentication.Type to be 'Default', 'ManagedIdentity', or 'JwtPassthrough'. " +
                    $"Current OutboundAuthentication.Type is '{outboundType}'.");
            }
        }
        else if (inboundType == InboundAuthenticationType.JwtBearerScheme)
        {
            // When Inbound=JwtBearerScheme, only ManagedIdentity, JwtPassthrough, JwtObo are allowed
            if (outboundType != OutboundAuthenticationType.ManagedIdentity &&
                outboundType != OutboundAuthenticationType.JwtPassthrough &&
                outboundType != OutboundAuthenticationType.JwtObo)
            {
                throw new InvalidOperationException(
                    $"InboundAuthentication.Type 'JwtBearerScheme' requires OutboundAuthentication.Type to be 'ManagedIdentity', 'JwtPassthrough', or 'JwtObo'. " +
                    $"Current OutboundAuthentication.Type is '{outboundType}'.");
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported InboundAuthentication.Type '{inboundType}'. " +
                "Supported types are: 'None', 'JwtBearerScheme'.");
        }
    }

    /// <summary>
    /// Validates JWT exchange (OBO) configuration requirements.
    /// </summary>
    /// <param name="config">The server configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when JWT exchange configuration is invalid.</exception>
    private static void ValidateJwtOboConfiguration(ServerConfiguration config)
    {
        if (config.OutboundAuthentication.Type != OutboundAuthenticationType.JwtObo)
        {
            return; // Only validate when using JWT OBO
        }

        // InboundAuthentication.AzureAd must be present (JwtObo inherits from inbound)
        if (config.InboundAuthentication.AzureAd is null)
        {
            throw new InvalidOperationException(
                "OutboundAuthentication.Type 'JwtObo' requires InboundAuthentication.AzureAd to be configured.");
        }

        // Validate the inbound AzureAd configuration
        ValidateAzureAdConfiguration(config.InboundAuthentication.AzureAd);

        // OutboundAuthentication.ClientCredential must be present
        if (config.OutboundAuthentication.ClientCredential is null)
        {
            throw new InvalidOperationException(
                "OutboundAuthentication.Type 'JwtObo' requires OutboundAuthentication.ClientCredential to be configured.");
        }

        // Validate ClientCredential based on Kind
        ValidateClientCredentialConfiguration(config.OutboundAuthentication.ClientCredential);
    }

    /// <summary>
    /// Validates JWT passthrough configuration requirements.
    /// </summary>
    /// <param name="config">The server configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when JWT passthrough configuration is invalid.</exception>
    private static void ValidateJwtPassthroughConfiguration(ServerConfiguration config)
    {
        if (config.OutboundAuthentication.Type != OutboundAuthenticationType.JwtPassthrough)
        {
            return;
        }

        // If inbound auth is JwtBearerScheme, AzureAd must be present
        if (config.InboundAuthentication.Type == InboundAuthenticationType.JwtBearerScheme
            && config.InboundAuthentication.AzureAd is null)
        {
            throw new InvalidOperationException(
                "InboundAuthentication.Type 'JwtBearerScheme' requires InboundAuthentication.AzureAd to be configured.");
        }

        if (config.InboundAuthentication.Type == InboundAuthenticationType.JwtBearerScheme)
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
            return;
        }

        // If inbound auth is JwtBearerScheme, AzureAd must be present
        if (config.InboundAuthentication.Type == InboundAuthenticationType.JwtBearerScheme
            && config.InboundAuthentication.AzureAd is null)
        {
            throw new InvalidOperationException(
                "InboundAuthentication.Type 'JwtBearerScheme' requires InboundAuthentication.AzureAd to be configured.");
        }

        if (config.InboundAuthentication.Type == InboundAuthenticationType.JwtBearerScheme)
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

    /// <summary>
    /// Validates client credential configuration based on the specified kind.
    /// </summary>
    /// <param name="clientCredential">The client credential configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when client credential configuration is invalid.</exception>
    private static void ValidateClientCredentialConfiguration(ClientCredentialConfig clientCredential)
    {
        switch (clientCredential.Kind)
        {
            case JwtOboClientCredentialKind.ClientSecret:
                if (string.IsNullOrWhiteSpace(clientCredential.Secret))
                {
                    throw new InvalidOperationException(
                        "ClientCredential.Kind 'ClientSecret' requires ClientCredential.Secret to be configured.");
                }
                // Ensure other properties are not set for ClientSecret
                if (!string.IsNullOrWhiteSpace(clientCredential.Thumbprint))
                {
                    throw new InvalidOperationException(
                        "ClientCredential.Kind 'ClientSecret' should not have ClientCredential.Thumbprint configured.");
                }
                if (!string.IsNullOrWhiteSpace(clientCredential.KeyVaultUrl))
                {
                    throw new InvalidOperationException(
                        "ClientCredential.Kind 'ClientSecret' should not have ClientCredential.KeyVaultUrl configured.");
                }
                break;

            case JwtOboClientCredentialKind.CertificateLocal:
                if (string.IsNullOrWhiteSpace(clientCredential.Thumbprint))
                {
                    throw new InvalidOperationException(
                        "ClientCredential.Kind 'CertificateLocal' requires ClientCredential.Thumbprint to be configured.");
                }
                // Ensure other properties are not set for CertificateLocal
                if (!string.IsNullOrWhiteSpace(clientCredential.Secret))
                {
                    throw new InvalidOperationException(
                        "ClientCredential.Kind 'CertificateLocal' should not have ClientCredential.Secret configured.");
                }
                if (!string.IsNullOrWhiteSpace(clientCredential.KeyVaultUrl))
                {
                    throw new InvalidOperationException(
                        "ClientCredential.Kind 'CertificateLocal' should not have ClientCredential.KeyVaultUrl configured.");
                }
                break;

            case JwtOboClientCredentialKind.CertificateKeyVault:
                if (string.IsNullOrWhiteSpace(clientCredential.Thumbprint))
                {
                    throw new InvalidOperationException(
                        "ClientCredential.Kind 'CertificateKeyVault' requires ClientCredential.Thumbprint to be configured.");
                }
                if (string.IsNullOrWhiteSpace(clientCredential.KeyVaultUrl))
                {
                    throw new InvalidOperationException(
                        "ClientCredential.Kind 'CertificateKeyVault' requires ClientCredential.KeyVaultUrl to be configured.");
                }
                // Ensure Secret is not set for CertificateKeyVault
                if (!string.IsNullOrWhiteSpace(clientCredential.Secret))
                {
                    throw new InvalidOperationException(
                        "ClientCredential.Kind 'CertificateKeyVault' should not have ClientCredential.Secret configured.");
                }
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported ClientCredential.Kind '{clientCredential.Kind}'.");
        }
    }
}
