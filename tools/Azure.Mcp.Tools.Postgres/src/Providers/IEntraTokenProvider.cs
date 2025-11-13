using Azure.Core;

namespace Azure.Mcp.Tools.Postgres.Auth
{
    public interface IEntraTokenProvider
    {
        Task<AccessToken> GetEntraToken(TokenCredential tokenCredential, CancellationToken cancellationToken);
    }
}
