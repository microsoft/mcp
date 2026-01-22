// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using Azure.Mcp.Core.Areas.Server.Commands.Discovery;

namespace Azure.Mcp.Core.Helpers
{
    /// <summary>
    /// DelegatingHandler that adds a Bearer access token to each outgoing request.
    /// </summary>
    public sealed class AccessTokenHandler : DelegatingHandler
    {
        private readonly Func<CancellationToken, Task<string>> _accessTokenProvider;

        public AccessTokenHandler(Func<CancellationToken, Task<string>> accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _accessTokenProvider(cancellationToken);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }

    public sealed class RegistryServerHelper
    {
        public static string GetRegistryServerHttpClientName(string serverName)
        {
            return $"azmcp.{nameof(RegistryServerProvider)}.{serverName}";
        }
    }
}
