// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Areas.Server.Models;
using Microsoft.Mcp.Core.Helpers;
using ModelContextProtocol.Client;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;

/// <summary>
/// Provides an MCP server implementation based on registry configuration.
/// Supports stdio transport mechanism.
/// </summary>
/// <param name="id">The unique identifier for the server.</param>
/// <param name="serverInfo">Configuration information for the server.</param>
/// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
/// <param name="tokenCredentialProvider">The token credential provider for OAuth authentication.</param>
public sealed class RegistryServerProvider(string id, RegistryServerInfo serverInfo, IHttpClientFactory httpClientFactory) : IMcpServerProvider
{
    private readonly string _id = id;
    private readonly RegistryServerInfo _serverInfo = serverInfo;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private const string DefaultVersionPattern = @"(\d+\.\d+\.\d+)";

    /// <summary>
    /// Creates metadata that describes this registry-based server.
    /// </summary>
    /// <returns>A metadata object containing the server's identity and description.</returns>
    public McpServerMetadata CreateMetadata() => new()
    {
        Id = _id,
        Name = _id,
        Title = _serverInfo.Title,
        Description = _serverInfo.Description ?? string.Empty,
        ToolPrefix = _serverInfo.ToolPrefix
    };

    /// <inheritdoc/>
    public async Task<McpClient> CreateClientAsync(McpClientOptions clientOptions, CancellationToken cancellationToken)
    {
        Func<McpClientOptions, CancellationToken, Task<McpClient>>? clientFactory = null;

        if (!string.IsNullOrWhiteSpace(_serverInfo.Url))
        {
            clientFactory = CreateHttpClientAsync;
        }
        else if (!string.IsNullOrWhiteSpace(_serverInfo.Type) && _serverInfo.Type.Equals("stdio", StringComparison.OrdinalIgnoreCase))
        {
            clientFactory = CreateStdioClientAsync;
        }

        if (clientFactory == null)
        {
            throw new ArgumentException($"Registry server '{_id}' does not have a valid transport type. Either 'url' for HTTP transport or 'type=stdio' with 'command' must be specified.");
        }

        // Pre-flight version check for stdio servers with version requirements
        if (clientFactory == CreateStdioClientAsync &&
            !string.IsNullOrWhiteSpace(_serverInfo.MinVersion) &&
            !string.IsNullOrWhiteSpace(_serverInfo.Command))
        {
            if (_serverInfo.VersionArgs == null || _serverInfo.VersionArgs.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Registry server '{_id}' specifies 'minVersion' but is missing 'versionArgs'. Both are required for version checking.");
            }

            var versionError = await CheckCommandVersionAsync(
                _serverInfo.Command, _serverInfo.VersionArgs, _serverInfo.MinVersion,
                _serverInfo.VersionPattern, cancellationToken);

            if (versionError != null)
            {
                var message = string.IsNullOrWhiteSpace(_serverInfo.InstallInstructions)
                    ? $"Failed to initialize the '{_id}' MCP tool. {versionError}"
                    : $"""
                        Failed to initialize the '{_id}' MCP tool.
                        {versionError}

                        Installation Instructions:
                        {_serverInfo.InstallInstructions}
                        """;

                throw new InvalidOperationException(message.Trim());
            }
        }

        try
        {
            return await clientFactory(clientOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(_serverInfo.InstallInstructions))
            {
                var errorWithInstructions = $"""
                    Failed to initialize the '{_id}' MCP tool.
                    This tool may require dependencies that are not installed.

                    Installation Instructions:
                    {_serverInfo.InstallInstructions}
                    """;

                throw new InvalidOperationException(errorWithInstructions.Trim(), ex);
            }

            throw new InvalidOperationException($"Failed to create MCP client for registry server '{_id}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks whether the specified command is installed and meets the minimum version requirement.
    /// </summary>
    /// <param name="command">The command to check (e.g., "azd").</param>
    /// <param name="versionArgs">Arguments to pass to get version output.</param>
    /// <param name="minVersion">The minimum required version string (e.g., "1.20.0").</param>
    /// <param name="versionPattern">Regex pattern with a capture group for the version. Defaults to semver pattern.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An error message if the check fails, or null if the command meets requirements.</returns>
    internal static async Task<string?> CheckCommandVersionAsync(
        string command, IList<string> versionArgs, string minVersion,
        string? versionPattern, CancellationToken cancellationToken)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            foreach (var arg in versionArgs)
            {
                process.StartInfo.ArgumentList.Add(arg);
            }

            if (!process.Start())
            {
                return $"'{command}' failed to start.";
            }

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);

            // Apply a hard timeout so a hung command cannot block indefinitely
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));

            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Timeout fired, not caller cancellation - kill and fail-open
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Best-effort cleanup
                }

                // Observe stream task to prevent unobserved task exceptions
                try
                {
                    await outputTask;
                }
                catch
                {
                    // Process killed - stream disposed
                }

                return null;
            }

            var output = await outputTask;

            var installedVersion = ParseVersionFromOutput(output, versionPattern);
            if (installedVersion != null && Version.TryParse(minVersion, out var requiredVersion))
            {
                if (installedVersion < requiredVersion)
                {
                    return $"'{command}' version {installedVersion} is installed, but version {requiredVersion} or later is required.";
                }
            }

            return null;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or FileNotFoundException)
        {
            return $"'{command}' is not installed or not found in the system PATH.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Unknown error checking version - let the normal connection attempt proceed
            return null;
        }
    }

    /// <summary>
    /// Extracts a version from command output text using the specified pattern.
    /// </summary>
    /// <param name="output">The stdout text from a version command.</param>
    /// <param name="versionPattern">Regex with a capture group for the version, or null for default semver pattern.</param>
    /// <returns>The parsed <see cref="Version"/>, or null if no version was found.</returns>
    internal static Version? ParseVersionFromOutput(string output, string? versionPattern = null)
    {
        var match = Regex.Match(output, versionPattern ?? DefaultVersionPattern);
        return match.Success && match.Groups.Count > 1 && Version.TryParse(match.Groups[1].Value, out var version) ? version : null;
    }

    /// <summary>
    /// Creates an MCP client that communicates with the server using Server-Sent Events (SSE).
    /// </summary>
    /// <param name="clientOptions">Options to configure the client behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A configured MCP client using SSE transport.</returns>
    private async Task<McpClient> CreateHttpClientAsync(McpClientOptions clientOptions, CancellationToken cancellationToken)
    {
        var transportOptions = new HttpClientTransportOptions
        {
            Name = _id,
            Endpoint = new Uri(_serverInfo.Url!),
            TransportMode = HttpTransportMode.AutoDetect,
            // HttpClientTransportOptions offers an OAuth property to configure client side OAuth parameters, such as RedirectUri and ClientId.
            // When OAuth property is set, the MCP client will attempt to complete the Auth flow following the MCP protocol.
            // However, there is a gap between what MCP protocol requires the OAuth provider to implement and what Entra supports. This MCP client will always send a resource parameter to the token endpoint because it is required by the MCP protocol but Entra doesn't support it. More details in issue #939 and related discussions in modelcontextprotocol/csharp-sdk GitHub repo.
        };

        HttpClientTransport clientTransport;
        if (_serverInfo.OAuthScopes is not null)
        {
            // Registry servers with OAuthScopes must create HttpClient with this key to create an HttpClient that knows how to fetch its access tokens.
            // The HttpClients are registered in RegistryServerServiceCollectionExtensions.cs.
            var client = _httpClientFactory.CreateClient(RegistryServerHelper.GetRegistryServerHttpClientName(_serverInfo.Name!));
            clientTransport = new HttpClientTransport(transportOptions, client, ownsHttpClient: true);
        }
        else
        {
            clientTransport = new HttpClientTransport(transportOptions);
        }

        return await McpClient.CreateAsync(clientTransport, clientOptions, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates an MCP client that communicates with the server using stdio (standard input/output).
    /// </summary>
    /// <param name="clientOptions">Options to configure the client behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A configured MCP client using stdio transport.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the server configuration doesn't specify a valid command for stdio transport.</exception>
    private async Task<McpClient> CreateStdioClientAsync(McpClientOptions clientOptions, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_serverInfo.Command))
        {
            throw new InvalidOperationException($"Registry server '{_id}' does not have a valid command for stdio transport.");
        }

        // Merge current system environment variables with serverInfo.Env (serverInfo.Env overrides system)
        var env = Environment.GetEnvironmentVariables()
            .Cast<System.Collections.DictionaryEntry>()
            .ToDictionary(e => (string)e.Key, e => (string?)e.Value);

        if (_serverInfo.Env != null)
        {
            foreach (var kvp in _serverInfo.Env)
            {
                env[kvp.Key] = kvp.Value;
            }
        }

        var transportOptions = new StdioClientTransportOptions
        {
            Name = _id,
            Command = _serverInfo.Command,
            Arguments = _serverInfo.Args,
            EnvironmentVariables = env
        };

        var clientTransport = new StdioClientTransport(transportOptions);
        return await McpClient.CreateAsync(clientTransport, clientOptions, cancellationToken: cancellationToken);
    }
}
