// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Azure.Core;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Microsoft.Mcp.Tests.Client.Helpers;

public class LiveTestSettingsFixture : IAsyncLifetime
{
    private static readonly ConcurrentDictionary<string, Lazy<Task<(bool IsServicePrincipal, string PrincipalName)>>> s_principalSettingsCache = new(StringComparer.OrdinalIgnoreCase);

    public LiveTestSettings Settings { get; private set; } = new();

    public virtual ValueTask InitializeAsync()
    {
        // If the TestMode is Playback, skip loading other settings. Skipping will match behaviors in CI when resources aren't deployed,
        // as content is recorded.
        if (Settings.TestMode == TestMode.Playback)
        {
            return ValueTask.CompletedTask;
        }

        if (LiveTestSettings.TryLoadTestSettings(out var settings))
        {
            Settings = settings;
            foreach ((string key, string value) in Settings.EnvironmentVariables)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
        else
        {
            throw new FileNotFoundException($"Test settings file '{LiveTestSettings.TestSettingsFileName}' not found in the assembly directory or its parent directories.");
        }

        return ValueTask.CompletedTask;
    }

    public async Task ResolvePrincipalSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (Settings.TestMode == TestMode.Playback)
        {
            return;
        }

        var principalSettings = s_principalSettingsCache.GetOrAdd(
            Settings.TenantId,
            static tenantId => new(
                () => GetPrincipalSettingsAsync(tenantId),
                LazyThreadSafetyMode.ExecutionAndPublication));

        (Settings.IsServicePrincipal, Settings.PrincipalName) = await principalSettings.Value
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<(bool IsServicePrincipal, string PrincipalName)> GetPrincipalSettingsAsync(string tenantId)
    {
        const string GraphScopeUri = "https://graph.microsoft.com/.default";
        var credential = new CustomChainedCredential(tenantId);
        AccessToken token = await credential.GetTokenAsync(new TokenRequestContext([GraphScopeUri]), CancellationToken.None);
        var jsonToken = new JwtSecurityToken(token.Token);

        var claims = JsonSerializer.Serialize(jsonToken.Claims.Select(x => x.Type));

        var principalType = jsonToken.Claims.FirstOrDefault(c => c.Type == "idtyp")?.Value ??
            throw new Exception($"Unable to locate 'idtyp' claim in Entra ID token: {claims}");

        var isServicePrincipal = string.Equals(principalType, "app", StringComparison.OrdinalIgnoreCase);

        var nameClaim = isServicePrincipal ? "app_displayname" : "unique_name";

        var principalName = jsonToken.Claims.FirstOrDefault(c => c.Type == nameClaim)?.Value ??
            throw new Exception($"Unable to locate '{nameClaim}' claim in Entra ID token: {claims}");

        return (isServicePrincipal, principalName);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
