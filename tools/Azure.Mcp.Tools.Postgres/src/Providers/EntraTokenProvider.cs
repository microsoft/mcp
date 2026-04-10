using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Postgres.Auth
{
    internal class EntraTokenProvider(IAzureCloudConfiguration cloudConfiguration) : IEntraTokenProvider
    {
        public async Task<AccessToken> GetEntraToken(TokenCredential tokenCredential, CancellationToken cancellationToken)
        {
            var tokenRequestContext = new TokenRequestContext([GetOpenSourceRDBMSScope()]);
            var accessToken = await tokenCredential
                .GetTokenAsync(tokenRequestContext, cancellationToken);
            return accessToken;
        }

        private string GetOpenSourceRDBMSScope()
        {
            return cloudConfiguration.CloudType switch
            {
                AzureCloudConfiguration.AzureCloud.AzurePublicCloud =>
                    "https://ossrdbms-aad.database.windows.net/.default",
                AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud =>
                    "https://ossrdbms-aad.database.usgovcloudapi.net/.default",
                AzureCloudConfiguration.AzureCloud.AzureChinaCloud =>
                    "https://ossrdbms-aad.database.chinacloudapi.cn/.default",
                _ =>
                    "https://ossrdbms-aad.database.windows.net/.default"
            };
        }
    }
}
