using Azure.Core;

namespace Azure.Mcp.Tools.Postgres.Auth
{
    internal class EntraTokenProvider : IEntraTokenProvider
    {
        public async Task<AccessToken> GetEntraToken(TokenCredential tokenCredential)
        {
            var tokenRequestContext = new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]);
            var accessToken = await tokenCredential
                .GetTokenAsync(tokenRequestContext, CancellationToken.None)
                .ConfigureAwait(false);
            return accessToken;
        }
    }
}
