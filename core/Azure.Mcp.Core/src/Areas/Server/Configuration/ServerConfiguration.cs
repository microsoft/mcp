// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Configuration;

/// <summary>
/// Root configuration for Azure MCP Server authentication.
/// </summary>
public class ServerConfiguration
{
    /// <summary>
    /// Gets the default server configuration instance.
    /// </summary>
    public static readonly ServerConfiguration Default = new()
    {
        InboundAuthentication = new InboundAuthenticationConfig
        {
            Type = InboundAuthenticationType.None
        },
        OutboundAuthentication = new OutboundAuthenticationConfig
        {
            Type = OutboundAuthenticationType.Default
        }
    };

    /// <summary>
    /// Configuration for validating incoming HTTP requests.
    /// </summary>
    [JsonPropertyName("inboundAuthentication")]
    public required InboundAuthenticationConfig InboundAuthentication { get; set; }

    /// <summary>
    /// Configuration for Azure SDK authentication (outbound calls).
    /// </summary>
    [JsonPropertyName("outboundAuthentication")]
    public required OutboundAuthenticationConfig OutboundAuthentication { get; set; }

    /// <summary>
    /// Returns a formatted JSON string representation of the configuration.
    /// </summary>
    /// <returns>A JSON string representation of this configuration</returns>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, ConfigurationJsonContext.Default.ServerConfiguration);
    }

    /// <summary>
    /// Loads server configuration from a JSON file, or returns default configuration if path is not provided.
    /// </summary>
    /// <param name="filePath">The optional path to the JSON configuration file</param>
    /// <returns>The loaded or default server configuration</returns>
    public static ServerConfiguration LoadFromFileOrDefault(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return ServerConfiguration.Default;
        }
        ServerConfiguration config = LoadFromFile(filePath);
        if (IsDefault(config))
        {
            return ServerConfiguration.Default;
        }
        ConfigurationValidator.Validate(config);
        return config;
    }

    /// <summary>
    /// Loads server configuration from a JSON file.
    /// </summary>
    /// <param name="filePath">The absolute path to the JSON configuration file</param>
    /// <returns>The loaded server configuration</returns>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file is not found</exception>
    /// <exception cref="JsonException">Thrown when the configuration file contains invalid JSON</exception>
    /// <exception cref="InvalidOperationException">Thrown when required properties are missing</exception>
    private static ServerConfiguration LoadFromFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Configuration file not found: {filePath}. " +
                "Specify a valid path using --config-file-path option or omit it to use default configuration.",
                filePath);
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize(json, ConfigurationJsonContext.Default.ServerConfiguration);

            if (config == null)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize configuration from {filePath}. The file may be empty or contain invalid JSON.");
            }
            return config;
        }
        catch (JsonException ex)
        {
            throw new JsonException(
                $"Invalid JSON in configuration file {filePath}: {ex.Message}. " +
                "Please verify the file contains valid JSON syntax.",
                ex);
        }
        catch (Exception ex) when (ex is not FileNotFoundException and not JsonException)
        {
            throw new InvalidOperationException(
                $"Failed to load configuration from {filePath}: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Determines whether the specified configuration represents the default configuration.
    /// </summary>
    /// <param name="config">The configuration to check</param>
    /// <returns>true if the configuration matches the default configuration; otherwise, false</returns>
    private static bool IsDefault(ServerConfiguration config)
    {
        return config.InboundAuthentication.Type == InboundAuthenticationType.None &&
               config.OutboundAuthentication.Type == OutboundAuthenticationType.Default &&
               config.InboundAuthentication.AzureAd == null &&
               config.OutboundAuthentication.ClientCredential == null;
    }
}

