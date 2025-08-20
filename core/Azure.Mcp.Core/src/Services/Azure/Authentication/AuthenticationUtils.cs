// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// A utility class for handling Azure authentication.
/// </summary>

public static class AuthenticationUtils
{
    /// <summary>
    /// Fetches the Azure credentials from the environment variable AZURE_CREDENTIALS.
    /// </summary>
    public static AzureCredentials? GetAzureCredentials(ILogger logger)
    {
        var credentialsJson = Environment.GetEnvironmentVariable("AZURE_CREDENTIALS");
        if (string.IsNullOrEmpty(credentialsJson))
        {
            return null;
        }

        try
        {
            // Use source-generated serialization to avoid trimmer warnings
            var credentials = JsonSerializer.Deserialize(credentialsJson, JsonSourceGenerationContext.Default.AzureCredentials);
            if (credentials == null)
            {
                logger.LogWarning("Invalid AZURE_CREDENTIALS format. Ensure it contains clientId, clientSecret, and tenantId.");
                return null;
            }
            return credentials;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize AZURE_CREDENTIALS. Ensure it contains clientId, clientSecret, and tenantId.");
            return null;
        }
    }
}
