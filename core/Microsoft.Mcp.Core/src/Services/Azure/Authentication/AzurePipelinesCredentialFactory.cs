// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

internal static class AzurePipelinesCredentialFactory
{
    internal const string TenantIdEnvVarName = "AZURESUBSCRIPTION_TENANT_ID";
    internal const string ClientIdEnvVarName = "AZURESUBSCRIPTION_CLIENT_ID";
    internal const string ServiceConnectionIdEnvVarName = "AZURESUBSCRIPTION_SERVICE_CONNECTION_ID";
    internal const string SystemAccessTokenEnvVarName = "SYSTEM_ACCESSTOKEN";

    internal static TokenCredential? Create(IAzureCloudConfiguration? cloudConfiguration, bool required)
    {
        string? tenantId = Environment.GetEnvironmentVariable(TenantIdEnvVarName);
        string? clientId = Environment.GetEnvironmentVariable(ClientIdEnvVarName);
        string? serviceConnectionId = Environment.GetEnvironmentVariable(ServiceConnectionIdEnvVarName);
        string? systemAccessToken = Environment.GetEnvironmentVariable(SystemAccessTokenEnvVarName);

        string[] missingVariables =
        [
            .. new[]
            {
                (Name: TenantIdEnvVarName, Value: tenantId),
                (Name: ClientIdEnvVarName, Value: clientId),
                (Name: ServiceConnectionIdEnvVarName, Value: serviceConnectionId),
                (Name: SystemAccessTokenEnvVarName, Value: systemAccessToken),
            }
            .Where(variable => string.IsNullOrWhiteSpace(variable.Value))
            .Select(variable => variable.Name)
        ];

        if (missingVariables.Length > 0)
        {
            if (required)
            {
                throw new CredentialUnavailableException(
                    $"AzurePipelinesCredential requires the following environment variables: {string.Join(", ", missingVariables)}.");
            }

            return null;
        }

        var options = new AzurePipelinesCredentialOptions();
        if (cloudConfiguration != null)
        {
            options.AuthorityHost = cloudConfiguration.AuthorityHost;
        }

        return new AzurePipelinesCredential(tenantId, clientId, serviceConnectionId, systemAccessToken, options);
    }
}