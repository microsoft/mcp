// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Abstractions;

namespace Microsoft.Mcp.Core.Areas.Server.Options;

internal static class MicrosoftIdentityApplicationOptionsBinder
{
    public static void Bind(
        IConfiguration configuration,
        MicrosoftIdentityApplicationOptions options)
    {
        var source = new MicrosoftIdentityApplicationConfiguration();
        configuration.Bind(source);

        options.Name = source.Name ?? options.Name;
        options.Instance = source.Instance ?? options.Instance;
        options.TenantId = source.TenantId ?? options.TenantId;
        options.AppHomeTenantId = source.AppHomeTenantId ?? options.AppHomeTenantId;
        options.ClientId = source.ClientId ?? options.ClientId;
        options.Authority = source.Authority ?? options.Authority;
        options.AzureRegion = source.AzureRegion ?? options.AzureRegion;
        options.Audience = source.Audience ?? options.Audience;
        options.Domain = source.Domain ?? options.Domain;
        options.EditProfilePolicyId = source.EditProfilePolicyId ?? options.EditProfilePolicyId;
        options.SignUpSignInPolicyId = source.SignUpSignInPolicyId ?? options.SignUpSignInPolicyId;
        options.ResetPasswordPolicyId = source.ResetPasswordPolicyId ?? options.ResetPasswordPolicyId;
        options.ResetPasswordPath = source.ResetPasswordPath ?? options.ResetPasswordPath;
        options.ErrorPath = source.ErrorPath ?? options.ErrorPath;
        options.Audiences = source.Audiences ?? options.Audiences;
        options.ClientCapabilities = source.ClientCapabilities ?? options.ClientCapabilities;
        options.ExtraQueryParameters = source.ExtraQueryParameters ?? options.ExtraQueryParameters;

        if (source.ClientCredentials is not null)
        {
            options.ClientCredentials = source.ClientCredentials.Select(CreateCredentialDescription).ToList();
        }

        if (source.TokenDecryptionCredentials is not null)
        {
            options.TokenDecryptionCredentials = source.TokenDecryptionCredentials
                .Select(CreateCredentialDescription)
                .ToList();
        }

        if (source.AllowWebApiToBeAuthorizedByACL is { } allowWebApiToBeAuthorizedByAcl)
        {
            options.AllowWebApiToBeAuthorizedByACL = allowWebApiToBeAuthorizedByAcl;
        }

        if (source.EnablePiiLogging is { } enablePiiLogging)
        {
            options.EnablePiiLogging = enablePiiLogging;
        }

        if (source.SendX5C is { } sendX5C)
        {
            options.SendX5C = sendX5C;
        }

        if (source.WithSpaAuthCode is { } withSpaAuthCode)
        {
            options.WithSpaAuthCode = withSpaAuthCode;
        }
    }

    private static CredentialDescription CreateCredentialDescription(CredentialConfiguration source)
    {
        var credential = new CredentialDescription
        {
            Algorithm = source.Algorithm,
            KeyVaultUrl = source.KeyVaultUrl,
            CertificateStorePath = source.CertificateStorePath,
            CertificateDistinguishedName = source.CertificateDistinguishedName,
            CertificateSubjectName = source.CertificateSubjectName,
            KeyVaultCertificateName = source.KeyVaultCertificateName,
            CertificateThumbprint = source.CertificateThumbprint,
            CertificateDiskPath = source.CertificateDiskPath,
            CertificatePassword = source.CertificatePassword,
            Base64EncodedValue = source.Base64EncodedValue,
            ClientSecret = source.ClientSecret,
            ManagedIdentityClientId = source.ManagedIdentityClientId,
            SignedAssertionFileDiskPath = source.SignedAssertionFileDiskPath,
            TokenExchangeAuthority = source.TokenExchangeAuthority,
            TokenExchangeUrl = source.TokenExchangeUrl,
            CustomSignedAssertionProviderName = source.CustomSignedAssertionProviderName,
            CustomSignedAssertionProviderData = source.CustomSignedAssertionProviderData
        };

        if (source.SourceType is { } sourceType)
        {
            credential.SourceType = sourceType;
        }

        if (source.Skip is { } skip)
        {
            credential.Skip = skip;
        }

        if (source.UseBoundCredential is { } useBoundCredential)
        {
            credential.UseBoundCredential = useBoundCredential;
        }

        if (source.DecryptKeysAuthenticationOptions is not null)
        {
            credential.DecryptKeysAuthenticationOptions =
                CreateAuthorizationHeaderProviderOptions(source.DecryptKeysAuthenticationOptions);
        }

        return credential;
    }

    private static AuthorizationHeaderProviderOptions CreateAuthorizationHeaderProviderOptions(
        AuthorizationHeaderProviderConfiguration source)
    {
        var options = new AuthorizationHeaderProviderOptions();

        if (source.BaseUrl is not null)
        {
            options.BaseUrl = source.BaseUrl;
        }

        if (source.HttpMethod is not null)
        {
            options.HttpMethod = source.HttpMethod;
        }

        if (source.ProtocolScheme is not null)
        {
            options.ProtocolScheme = source.ProtocolScheme;
        }

        if (source.RelativePath is not null)
        {
            options.RelativePath = source.RelativePath;
        }

        if (source.RequestAppToken is { } requestAppToken)
        {
            options.RequestAppToken = requestAppToken;
        }

        if (source.AcquireTokenOptions is not null)
        {
            options.AcquireTokenOptions = CreateAcquireTokenOptions(source.AcquireTokenOptions);
        }

        return options;
    }

    private static AcquireTokenOptions CreateAcquireTokenOptions(AcquireTokenConfiguration source)
    {
        var options = new AcquireTokenOptions
        {
            AuthenticationOptionsName = source.AuthenticationOptionsName,
            Claims = source.Claims,
            CorrelationId = source.CorrelationId,
            ExtraHeadersParameters = source.ExtraHeadersParameters,
            ExtraParameters = source.ExtraParameters,
            ExtraQueryParameters = source.ExtraQueryParameters,
            FmiPath = source.FmiPath,
            LongRunningWebApiSessionKey = source.LongRunningWebApiSessionKey,
            PopClaim = source.PopClaim,
            PopPublicKey = source.PopPublicKey,
            Tenant = source.Tenant,
            UserFlow = source.UserFlow
        };

        if (source.ForceRefresh is { } forceRefresh)
        {
            options.ForceRefresh = forceRefresh;
        }

        if (source.ManagedIdentity is not null)
        {
            options.ManagedIdentity = new ManagedIdentityOptions
            {
                UserAssignedClientId = source.ManagedIdentity.UserAssignedClientId
            };
        }

        return options;
    }
}
