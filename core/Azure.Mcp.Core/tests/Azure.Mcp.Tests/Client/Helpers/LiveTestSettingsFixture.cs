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
        private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        public LiveTestSettings Settings { get; private set; } = new();

        public virtual async ValueTask InitializeAsync()
        {
            // If the TestMode is Playback, skip loading other settings. Skipping will match behaviors in CI when resources aren't deployed,
            // as content is recorded.
            if (Settings.TestMode == Tests.Helpers.TestMode.Playback)
            {
                return;
            }

            var testSettingsFileName = ".testsettings.json";
            var directory = Path.GetDirectoryName(typeof(LiveTestSettingsFixture).Assembly.Location);

            while (!string.IsNullOrEmpty(directory))
            {
                var testSettingsFilePath = Path.Combine(directory, testSettingsFileName);
                if (File.Exists(testSettingsFilePath))
                {
                    var content = await File.ReadAllTextAsync(testSettingsFilePath);

                    Settings = JsonSerializer.Deserialize<LiveTestSettings>(content, s_jsonSerializerOptions)
                        ?? throw new Exception("Unable to deserialize live test settings");

                    foreach (var (key, value) in Settings.EnvironmentVariables)
                    {
                        Environment.SetEnvironmentVariable(key, value);
                    }

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
            AccessToken token = await credential.GetTokenAsync(new TokenRequestContext([GraphScopeUri]), TestContext.Current.CancellationToken);
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
