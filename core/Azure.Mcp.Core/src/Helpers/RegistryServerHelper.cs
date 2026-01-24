// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Reflection;
using Azure.Core;
using Azure.Mcp.Core.Areas.Server;
using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Areas.Server.Models;
using Azure.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Core.Helpers
{
    /// <summary>
    /// DelegatingHandler that adds a Bearer access token to each outgoing request.
    /// </summary>
    public sealed class AccessTokenHandler : DelegatingHandler
    {
        private readonly IAzureTokenCredentialProvider _tokenCredentialProvider;
        private readonly string[] _oauthScopes;

        public AccessTokenHandler(IAzureTokenCredentialProvider tokenCredentialProvider, string[] oauthScopes)
        {
            _tokenCredentialProvider = tokenCredentialProvider;
            _oauthScopes = oauthScopes;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var credential = await _tokenCredentialProvider.GetTokenCredentialAsync(tenantId: null, cancellationToken);
            var tokenContext = new TokenRequestContext(_oauthScopes);
            var token = await credential.GetTokenAsync(tokenContext, cancellationToken);
            if (!string.IsNullOrEmpty(token.Token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
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

        public static IRegistryRoot? GetRegistryRoot()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("registry.json", StringComparison.OrdinalIgnoreCase));
            if (resourceName is null)
            {
                return null;
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                return null;
            }
            var registry = JsonSerializer.Deserialize(stream, ServerJsonContext.Default.RegistryRoot);
            if (registry?.Servers is null)
            {
                return null;
            }

            return registry;
        }
    }
}
