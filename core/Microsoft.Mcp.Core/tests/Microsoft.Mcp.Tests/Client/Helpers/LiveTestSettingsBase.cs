using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Tests.Client.Helpers
{
    public abstract class LiveTestSettingsBase
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public const string TestSettingsFileName = ".testsettings.json";

        /// <summary>
        /// Gets or sets the unique identifier of the Entra tenant associated with the current context.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the Entra tenant associated with the current context.
        /// </summary>
        public string TenantName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user principal name (UPN) associated with the principal running the tests.
        /// </summary>
        public string PrincipalName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the principal running the tests is a service principal or a user principal.
        /// </summary>
        /// <remarks>
        /// This is determined by inspecting the "idtyp" claim in the Entra ID access token, where a value of "app" indicates a service principal and "usr" indicates a user principal.
        /// </remarks>
        public bool IsServicePrincipal { get; set; }

        /// <summary>
        /// Gets or sets the directory path where the .testsettings.json file is located.
        /// </summary>
        public string SettingsDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the npx package being tested.
        /// </summary>
        /// <remarks>
        /// If specified, live tests will start the mcp server using an npx call to the specified package instead of
        /// directly executing the server binary. This allows for testing message flow through the node scripts in the
        /// package to ensure that commands, parameters and responses are all serialized and deserialized correctly.
        /// </remarks>
        public string TestPackage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the test mode. e.g. live, playback or record.
        /// </summary>
        public TestMode TestMode { get; set; } = TestMode.Live;

        /// <summary>
        /// Gets or sets a value indicating whether detailed debug output is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, additional diagnostic information may be written to logs or the console
        /// to assist with troubleshooting. This setting is typically used during development or when investigating
        /// issues.
        /// </remarks>
        public bool DebugOutput { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of environment variables to be set when starting the MCP server process.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = [];

        /// <summary>
        /// Gets or sets a dictionary of outputs from the test resources deployment script.
        /// </summary>
        public Dictionary<string, string> DeploymentOutputs { get; set; } = [];

        /// <summary>
        /// Gets or sets the extensionless mcp server executable name.
        /// </summary>
        /// <remarks>
        /// Derived classes must override this with the correct executable file name (e.g. "azmcp" for Azure MCP, "fabmcp" for Fabric MCP).
        /// </remarks>
        public abstract string ServerExecutableName { get; }

        /// <summary>
        /// Gets the path to the azmcp executable, handling OS-specific executable naming.
        /// </summary>
        /// <returns>The full path to the azmcp executable.</returns>
        public string GetMcpExecutablePath()
        {
            string executableName = OperatingSystem.IsWindows() ? $"{ServerExecutableName}.exe" : ServerExecutableName;
            return Path.Combine(AppContext.BaseDirectory, executableName);
        }

        /// <summary>
        /// Initializes the settings after loading static values from the .testsettings.json file.
        /// </summary>
        /// <remarks>
        /// The default implementation uses an Entra token to set the <see cref="PrincipalName"/> and <see cref="IsServicePrincipal"/> properties.
        /// </remarks>
        protected virtual async Task InitializeAsync()
        {
            if (TestMode == TestMode.Playback)
            {
                TenantId = "00000000-0000-0000-0000-000000000000";
                TenantName = "Sanitized";
                PrincipalName = "Sanitized";
                IsServicePrincipal = false;
                return;
            }

            const string GraphScopeUri = "https://graph.microsoft.com/.default";

            var credential = new CustomChainedCredential(TenantId);
            AccessToken token = await credential.GetTokenAsync(new TokenRequestContext([GraphScopeUri]), TestContext.Current.CancellationToken);
            var jsonToken = new JwtSecurityToken(token.Token);

            var claims = JsonSerializer.Serialize(jsonToken.Claims.Select(x => x.Type));

            var principalType = jsonToken.Claims.FirstOrDefault(c => c.Type == "idtyp")?.Value ??
                throw new Exception($"Unable to locate 'idtyp' claim in Entra ID token: {claims}");

            IsServicePrincipal = string.Equals(principalType, "app", StringComparison.OrdinalIgnoreCase);

            var nameClaim = IsServicePrincipal ? "app_displayname" : "unique_name";

            var principalName = jsonToken.Claims.FirstOrDefault(c => c.Type == nameClaim)?.Value ??
                throw new Exception($"Unable to locate 'unique_name' claim in Entra ID token: {claims}");

            PrincipalName = principalName;
        }

        public static bool TryFindTestSettingsFile([NotNullWhen(true)] out string? path)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var testSettingsFilePath = Path.Combine(directory, TestSettingsFileName);
                if (File.Exists(testSettingsFilePath))
                {
                    path = testSettingsFilePath;
                    return true;
                }

                directory = Path.GetDirectoryName(directory);
            }

            path = null;
            return false;
        }

        public static async Task<T> LoadTestSettingsAsync<T>() where T : LiveTestSettingsBase, new()
        {
            if (TryFindTestSettingsFile(out var path))
            {
                var json = File.ReadAllText(path);

                var settings = JsonSerializer.Deserialize<T>(json, s_jsonOptions)
                    ?? throw new JsonException($"Failed to deserialize test settings from '{path}'.");

                settings.SettingsDirectory = Path.GetDirectoryName(path) ?? string.Empty;

                foreach ((string key, string value) in settings.EnvironmentVariables)
                {
                    Environment.SetEnvironmentVariable(key, value);
                }

                await settings.InitializeAsync();

                return settings;
            }

            // Without a test settings file, we're implicitly in playback mode.
            var playbackSettings = new T
            {
                TestMode = TestMode.Playback
            };

            await playbackSettings.InitializeAsync();

            return playbackSettings;
        }
    }
}
