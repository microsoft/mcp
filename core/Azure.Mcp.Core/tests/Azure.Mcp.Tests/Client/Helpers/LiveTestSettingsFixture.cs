using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Xunit;
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tests.Client.Helpers
{
    public class LiveTestSettingsFixture : IAsyncLifetime
    {
        public LiveTestSettings Settings { get; private set; } = new();

        public virtual async ValueTask InitializeAsync()
        {
            var testSettingsFileName = ".testsettings.json";
            var directory = Path.GetDirectoryName(typeof(LiveTestSettingsFixture).Assembly.Location);
            while (!string.IsNullOrEmpty(directory))
            {
                var testSettingsFilePath = Path.Combine(directory, testSettingsFileName);
                if (File.Exists(testSettingsFilePath))
                {
                    var content = await File.ReadAllTextAsync(testSettingsFilePath);

                    Settings = JsonSerializer.Deserialize<LiveTestSettings>(content)
                        ?? throw new Exception("Unable to deserialize live test settings");

                    Settings.SettingsDirectory = directory;
                    await SetPrincipalSettingsAsync();

                    return;
                }

                directory = Path.GetDirectoryName(directory);
            }

            throw new FileNotFoundException($"Test settings file '{testSettingsFileName}' not found in the assembly directory or its parent directories.");
        }

        private async Task SetPrincipalSettingsAsync()
        {
            const string GraphScopeUri = "https://graph.microsoft.com/.default";
            var credential = new CustomChainedCredential(Settings.TenantId);
            AccessToken token = await credential.GetTokenAsync(new TokenRequestContext([GraphScopeUri]), CancellationToken.None);
            var jsonToken = new JwtSecurityToken(token.Token);

            var claims = JsonSerializer.Serialize(jsonToken.Claims.Select(x => x.Type));

            var principalType = jsonToken.Claims.FirstOrDefault(c => c.Type == "idtyp")?.Value ??
                throw new Exception($"Unable to locate 'idtyp' claim in Entra ID token: {claims}");

            Settings.IsServicePrincipal = string.Equals(principalType, "app", StringComparison.OrdinalIgnoreCase);

            var nameClaim = Settings.IsServicePrincipal ? "app_displayname" : "unique_name";

            var principalName = jsonToken.Claims.FirstOrDefault(c => c.Type == nameClaim)?.Value ??
                throw new Exception($"Unable to locate 'unique_name' claim in Entra ID token: {claims}");

            Settings.PrincipalName = principalName;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
